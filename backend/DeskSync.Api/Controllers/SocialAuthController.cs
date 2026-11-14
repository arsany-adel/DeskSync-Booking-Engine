using System.Net.Http.Json;
using System.Text.Json;
using DeskSync.Api.Configuration;
using DeskSync.Api.Constants;
using DeskSync.Api.DTOs.Users;
using DeskSync.Api.Entities;
using DeskSync.Api.Extensions;
using DeskSync.Api.Services.Interfaces;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DeskSync.Api.Controllers;

[ApiController]
[Route("api/auth/social")]
public class SocialAuthController(
    ISocialAuthService socialAuthService,
    ITokenService tokenService,
    IOptions<FrontendSettingsOptions> frontendOptions,
    IOptions<OAuthSettingsOptions> oauthOptions,
    IHttpClientFactory httpClientFactory) : BaseApiController
{
    private readonly ISocialAuthService _socialAuthService = socialAuthService;
    private readonly ITokenService _tokenService = tokenService;
    private readonly string _frontendBaseUrl = frontendOptions.Value.BaseUrl;
    private readonly OAuthSettingsOptions _oauth = oauthOptions.Value;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private const string GoogleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string GoogleTokenUrl = "https://oauth2.googleapis.com/token";
    private const string RedirectUri = "https://localhost:7297/api/auth/social/google/callback";

    private string BuildGoogleChallengeUrl(string state)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", _oauth.Google.ClientId },
            { "redirect_uri", RedirectUri },
            { "response_type", "code" },
            { "scope", "openid email profile" },
            { "access_type", "offline" },
            { "prompt", "consent" },
            { "state", state }
        };
        var qs = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        return $"{GoogleAuthUrl}?{qs}";
    }

    [HttpGet("google")]
    public IActionResult ChallengeGoogle()
    {
        var state = _tokenService.GenerateOAuthState(userId: null, provider: "google");
        return Redirect(BuildGoogleChallengeUrl(state));
    }

    private async Task<ErrorOr<ExternalUserInfo>> FetchGoogleIdentityAsync(string code)
    {
        var client = _httpClientFactory.CreateClient();

        var tokenRequest = new Dictionary<string, string>
        {
            { "client_id", _oauth.Google.ClientId },
            { "client_secret", _oauth.Google.ClientSecret },
            { "code", code },
            { "grant_type", "authorization_code" },
            { "redirect_uri", RedirectUri }
        };

        var tokenResponse = await client.PostAsync(GoogleTokenUrl, new FormUrlEncodedContent(tokenRequest));
        if (!tokenResponse.IsSuccessStatusCode)
            return DomainErrors.Auth.OAuthFailed;

        var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = tokenJson.GetProperty("access_token").GetString();
        if (string.IsNullOrEmpty(accessToken))
            return DomainErrors.Auth.OAuthFailed;

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var userInfoResponse = await client.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
        if (!userInfoResponse.IsSuccessStatusCode)
            return DomainErrors.Auth.OAuthFailed;

        var userInfo = await userInfoResponse.Content.ReadFromJsonAsync<JsonElement>();

        var email = userInfo.TryGetProperty("email", out var e) ? e.GetString() : null;
        var providerKey = userInfo.TryGetProperty("id", out var id) ? id.GetString() : null;
        var firstName = userInfo.TryGetProperty("given_name", out var fn) ? fn.GetString() : null;
        var lastName = userInfo.TryGetProperty("family_name", out var ln) ? ln.GetString() : null;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey))
            return DomainErrors.Auth.OAuthFailed;

        return new ExternalUserInfo(
            ProviderKey: providerKey,
            Email: email,
            EmailVerified: true,
            FirstName: firstName,
            LastName: lastName,
            Username: null);
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> HandleGoogleCallback(
        [FromQuery] string code, [FromQuery] string state)
    {
        var stateResult = _tokenService.ValidateOAuthState(state, "google");
        if (stateResult.IsError)
            return Redirect($"{_frontendBaseUrl}/login?error=invalid_state");

        if (string.IsNullOrEmpty(code))
            return Redirect($"{_frontendBaseUrl}/login?error=missing_code");

        var identity = await FetchGoogleIdentityAsync(code);
        if (identity.IsError)
            return Redirect($"{_frontendBaseUrl}/login?error={Uri.EscapeDataString(identity.Errors[0].Code)}");

        var linkedUserId = stateResult.Value; // null = login, guid = link

        return linkedUserId is null
            ? await CompleteLoginAsync(ProviderType.Google, identity.Value)
            : await CompleteLinkingAsync(linkedUserId.Value, ProviderType.Google, identity.Value);
    }

    private const string MicrosoftAuthUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
    private const string MicrosoftTokenUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
    private const string MicrosoftUserInfoUrl = "https://graph.microsoft.com/v1.0/me";
    private const string MicrosoftRedirectUri = "https://localhost:7297/api/auth/social/microsoft/callback";

    private string BuildMicrosoftChallengeUrl(string state)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", _oauth.Microsoft.ClientId },
            { "response_type", "code" },
            { "redirect_uri", MicrosoftRedirectUri },
            { "response_mode", "query" },
            { "scope", "openid email profile User.Read" },
            { "state", state }
        };
        var qs = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        return $"{MicrosoftAuthUrl}?{qs}";
    }

    [HttpGet("microsoft")]
    public IActionResult ChallengeMicrosoft()
    {
        var state = _tokenService.GenerateOAuthState(userId: null, provider: "microsoft");
        return Redirect(BuildMicrosoftChallengeUrl(state));
    }

    private async Task<ErrorOr<ExternalUserInfo>> FetchMicrosoftIdentityAsync(string code)
    {
        var client = _httpClientFactory.CreateClient();

        var tokenRequest = new Dictionary<string, string>
        {
            { "client_id", _oauth.Microsoft.ClientId },
            { "client_secret", _oauth.Microsoft.ClientSecret },
            { "code", code },
            { "grant_type", "authorization_code" },
            { "redirect_uri", MicrosoftRedirectUri }
        };

        var tokenResponse = await client.PostAsync(MicrosoftTokenUrl, new FormUrlEncodedContent(tokenRequest));
        if (!tokenResponse.IsSuccessStatusCode)
            return DomainErrors.Auth.OAuthFailed;

        var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = tokenJson.GetProperty("access_token").GetString();
        if (string.IsNullOrEmpty(accessToken))
            return DomainErrors.Auth.OAuthFailed;

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var userInfoResponse = await client.GetAsync(MicrosoftUserInfoUrl);
        if (!userInfoResponse.IsSuccessStatusCode)
            return DomainErrors.Auth.OAuthFailed;

        var userInfo = await userInfoResponse.Content.ReadFromJsonAsync<JsonElement>();

        var email = userInfo.TryGetProperty("mail", out var m) && m.ValueKind != JsonValueKind.Null
            ? m.GetString()
            : userInfo.TryGetProperty("userPrincipalName", out var upn) ? upn.GetString() : null;
        var providerKey = userInfo.TryGetProperty("id", out var id) ? id.GetString() : null;
        var firstName = userInfo.TryGetProperty("givenName", out var gn) ? gn.GetString() : null;
        var lastName = userInfo.TryGetProperty("surname", out var sn) ? sn.GetString() : null;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey))
            return DomainErrors.Auth.OAuthFailed;

        return new ExternalUserInfo(
            ProviderKey: providerKey,
            Email: email,
            EmailVerified: true,
            FirstName: firstName,
            LastName: lastName,
            Username: null);
    }

    [HttpGet("microsoft/callback")]
    public async Task<IActionResult> HandleMicrosoftCallback(
        [FromQuery] string code, [FromQuery] string state)
    {
        var stateResult = _tokenService.ValidateOAuthState(state, "microsoft");
        if (stateResult.IsError)
            return Redirect($"{_frontendBaseUrl}/login?error=invalid_state");

        if (string.IsNullOrEmpty(code))
            return Redirect($"{_frontendBaseUrl}/login?error=missing_code");

        var identity = await FetchMicrosoftIdentityAsync(code);
        if (identity.IsError)
            return Redirect($"{_frontendBaseUrl}/login?error={Uri.EscapeDataString(identity.Errors[0].Code)}");

        var linkedUserId = stateResult.Value;

        return linkedUserId is null
            ? await CompleteLoginAsync(ProviderType.Microsoft, identity.Value)
            : await CompleteLinkingAsync(linkedUserId.Value, ProviderType.Microsoft, identity.Value);
    }

    private const string LinkingLandingPath = "/settings/linked-accounts";

    private async Task<IActionResult> CompleteLoginAsync(
        ProviderType provider, ExternalUserInfo externalUser)
    {
        var authResult = await _socialAuthService.HandleExternalLoginAsync(provider, externalUser);
        if (authResult.IsError)
            return Redirect($"{_frontendBaseUrl}/login?error={Uri.EscapeDataString(authResult.Errors[0].Code)}");

        var a = authResult.Value;
        return Redirect(
            $"{_frontendBaseUrl}/auth/callback" +
            $"?accessToken={Uri.EscapeDataString(a.AccessToken)}" +
            $"&refreshToken={Uri.EscapeDataString(a.RefreshToken)}" +
            $"&userId={a.UserId}" +
            $"&email={Uri.EscapeDataString(a.Email)}" +
            $"&username={Uri.EscapeDataString(a.Username)}" +
            $"&role={a.Role}");
    }

    private async Task<IActionResult> CompleteLinkingAsync(
        Guid userId, ProviderType provider, ExternalUserInfo externalUser)
    {
        var result = await _socialAuthService.LinkExternalLoginAsync(userId, provider, externalUser);
        var providerName = provider.ToString().ToLowerInvariant();

        return result.IsError
            ? Redirect($"{_frontendBaseUrl}{LinkingLandingPath}?error={Uri.EscapeDataString(result.Errors[0].Code)}")
            : Redirect($"{_frontendBaseUrl}{LinkingLandingPath}?linked={providerName}");
    }

    [Authorize]
    [HttpGet("link/{provider}")]
    [ProducesResponseType(typeof(LinkChallengeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult StartLinking(string provider)
    {
        if (!Enum.TryParse<ProviderType>(provider, ignoreCase: true, out var providerType))
            return ErrorResult([DomainErrors.Auth.OAuthProviderNotSupported]);

        var userId = User.GetUserId();
        var state = _tokenService.GenerateOAuthState(userId, provider.ToLowerInvariant());

        var challengeUrl = providerType switch
        {
            ProviderType.Google => BuildGoogleChallengeUrl(state),
            ProviderType.Microsoft => BuildMicrosoftChallengeUrl(state),
            _ => null
        };

        if (challengeUrl is null)
            return ErrorResult([DomainErrors.Auth.OAuthProviderNotSupported]);

        return Ok(new LinkChallengeResponse(challengeUrl));
    }

    [Authorize]
    [HttpDelete("link/{provider}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Unlink(string provider)
    {
        if (!Enum.TryParse<ProviderType>(provider, ignoreCase: true, out var providerType))
            return ErrorResult([DomainErrors.Auth.OAuthProviderNotSupported]);

        var result = await _socialAuthService.UnlinkAsync(User.GetUserId(), providerType);
        return result.IsError ? ErrorResult(result.Errors) : NoContent();
    }
}