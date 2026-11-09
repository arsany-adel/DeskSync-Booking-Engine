using System.Net.Http.Json;
using System.Text.Json;
using DeskSync.Api.Configuration;
using DeskSync.Api.DTOs.Users;
using DeskSync.Api.Entities;
using DeskSync.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DeskSync.Api.Controllers;

[ApiController]
[Route("api/auth/social")]
public class SocialAuthController(
    ISocialAuthService socialAuthService,
    IOptions<FrontendSettingsOptions> frontendOptions,
    IOptions<OAuthSettingsOptions> oauthOptions,
    IHttpClientFactory httpClientFactory) : ControllerBase
{
    private readonly ISocialAuthService _socialAuthService = socialAuthService;
    private readonly string _frontendBaseUrl = frontendOptions.Value.BaseUrl;
    private readonly OAuthSettingsOptions _oauth = oauthOptions.Value;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private const string GoogleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string GoogleTokenUrl = "https://oauth2.googleapis.com/token";
    private const string RedirectUri = "https://localhost:7297/api/auth/social/google/callback";

    [HttpGet("google")]
    public IActionResult ChallengeGoogle()
    {
        // Manually build the Google login URL
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", _oauth.Google.ClientId },
            { "redirect_uri", RedirectUri },
            { "response_type", "code" },
            { "scope", "openid email profile" },
            { "access_type", "offline" },
            { "prompt", "consent" }
        };

        var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        return Redirect($"{GoogleAuthUrl}?{queryString}");
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> HandleGoogleCallback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return Redirect($"{_frontendBaseUrl}/login?error=missing_code");

        var client = _httpClientFactory.CreateClient();

        // 1. Exchange the code for an access token (Server-to-Server)
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
            return Redirect($"{_frontendBaseUrl}/login?error=token_exchange_failed");

        var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = tokenJson.GetProperty("access_token").GetString();

        // 2. Fetch the user's profile
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var userInfoResponse = await client.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
        
        if (!userInfoResponse.IsSuccessStatusCode)
            return Redirect($"{_frontendBaseUrl}/login?error=userinfo_failed");

        var userInfo = await userInfoResponse.Content.ReadFromJsonAsync<JsonElement>();
        
        var email = userInfo.GetProperty("email").GetString();
        var providerKey = userInfo.GetProperty("id").GetString();
        var firstName = userInfo.TryGetProperty("given_name", out var fn) ? fn.GetString() : null;
        var lastName = userInfo.TryGetProperty("family_name", out var ln) ? ln.GetString() : null;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey))
            return Redirect($"{_frontendBaseUrl}/login?error=missing_claims");

        var externalUser = new ExternalUserInfo(
            ProviderKey: providerKey,
            Email: email,
            EmailVerified: true, // Google API guarantees verified emails here
            FirstName: firstName,
            LastName: lastName,
            Username: null);

        // 3. Log the user in
        var authResult = await _socialAuthService.HandleExternalLoginAsync(ProviderType.Google, externalUser);

        if (authResult.IsError)
            return Redirect($"{_frontendBaseUrl}/login?error={Uri.EscapeDataString(authResult.Errors[0].Code)}");

        var authResponse = authResult.Value;

        // 4. Redirect to React frontend with tokens
        return Redirect(
            $"{_frontendBaseUrl}/auth/callback" +
            $"?accessToken={Uri.EscapeDataString(authResponse.AccessToken)}" +
            $"&refreshToken={Uri.EscapeDataString(authResponse.RefreshToken)}" +
            $"&userId={authResponse.UserId}" +
            $"&email={Uri.EscapeDataString(authResponse.Email)}" +
            $"&username={Uri.EscapeDataString(authResponse.Username)}" +
            $"&role={authResponse.Role}");
    }

    private const string MicrosoftAuthUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
    private const string MicrosoftTokenUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
    private const string MicrosoftUserInfoUrl = "https://graph.microsoft.com/v1.0/me";
    private const string MicrosoftRedirectUri = "https://localhost:7297/api/auth/social/microsoft/callback";

    [HttpGet("microsoft")]
    public IActionResult ChallengeMicrosoft()
    {
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", _oauth.Microsoft.ClientId },
            { "response_type", "code" },
            { "redirect_uri", MicrosoftRedirectUri },
            { "response_mode", "query" },
            { "scope", "openid email profile User.Read" }
        };

        var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        return Redirect($"{MicrosoftAuthUrl}?{queryString}");
    }

    [HttpGet("microsoft/callback")]
    public async Task<IActionResult> HandleMicrosoftCallback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
            return Redirect($"{_frontendBaseUrl}/login?error=missing_code");

        var client = _httpClientFactory.CreateClient();

        // 1. Exchange the code for an access token
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
            return Redirect($"{_frontendBaseUrl}/login?error=token_exchange_failed");

        var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = tokenJson.GetProperty("access_token").GetString();

        // 2. Fetch the user's profile from Microsoft Graph
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var userInfoResponse = await client.GetAsync(MicrosoftUserInfoUrl);

        if (!userInfoResponse.IsSuccessStatusCode)
            return Redirect($"{_frontendBaseUrl}/login?error=userinfo_failed");

        var userInfo = await userInfoResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Microsoft Graph returns 'mail' for work accounts and 'userPrincipalName' for personal
        var email = userInfo.TryGetProperty("mail", out var m) && m.ValueKind != JsonValueKind.Null
            ? m.GetString()
            : userInfo.TryGetProperty("userPrincipalName", out var upn) ? upn.GetString() : null;
        
        var providerKey = userInfo.GetProperty("id").GetString();
        var firstName = userInfo.TryGetProperty("givenName", out var gn) ? gn.GetString() : null;
        var lastName = userInfo.TryGetProperty("surname", out var sn) ? sn.GetString() : null;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey))
            return Redirect($"{_frontendBaseUrl}/login?error=missing_claims");

        var externalUser = new ExternalUserInfo(
            ProviderKey: providerKey,
            Email: email,
            EmailVerified: true, // Microsoft Graph guarantees verified emails
            FirstName: firstName,
            LastName: lastName,
            Username: null);

        // 3. Log the user in
        var authResult = await _socialAuthService.HandleExternalLoginAsync(ProviderType.Microsoft, externalUser);

        if (authResult.IsError)
            return Redirect($"{_frontendBaseUrl}/login?error={Uri.EscapeDataString(authResult.Errors[0].Code)}");

        var authResponse = authResult.Value;

        // 4. Redirect to React frontend with tokens
        return Redirect(
            $"{_frontendBaseUrl}/auth/callback" +
            $"?accessToken={Uri.EscapeDataString(authResponse.AccessToken)}" +
            $"&refreshToken={Uri.EscapeDataString(authResponse.RefreshToken)}" +
            $"&userId={authResponse.UserId}" +
            $"&email={Uri.EscapeDataString(authResponse.Email)}" +
            $"&username={Uri.EscapeDataString(authResponse.Username)}" +
            $"&role={authResponse.Role}");
    }
}