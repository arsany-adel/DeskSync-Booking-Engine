using DeskSync.Api.DTOs.Users;
using DeskSync.Api.Extensions;
using DeskSync.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskSync.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : BaseApiController
{
    private readonly IAuthService _authService = authService;

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterStandardUserRequestDto dto)
    {
        var result = await _authService.RegisterStandardUserAsync(dto);
        if (result.IsError) return ErrorResult(result.Errors);
        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginUserRequestDto dto)
    {
        var result = await _authService.LoginStandardUserAsync(dto);
        if (result.IsError) return ErrorResult(result.Errors);
        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        if (result.IsError) return ErrorResult(result.Errors);
        return Ok(result.Value);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest dto)
    {
        var result = await _authService.ForgotPasswordAsync(dto);
        if (result.IsError) return ErrorResult(result.Errors);
        return Ok(new { message = "If the email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest dto)
    {
        var result = await _authService.ResetPasswordAsync(dto);
        if (result.IsError) return ErrorResult(result.Errors);
        return Ok(new { message = "Password has been reset successfully." });
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest dto)
    {
        var result = await _authService.ConfirmEmailAsync(dto);
        if (result.IsError) return ErrorResult(result.Errors);
        return Ok(new { message = "Email confirmed successfully." });
    }

    [HttpPost("2fa/enable")]
    [Authorize]
    [ProducesResponseType(typeof(EnableTwoFactorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EnableTwoFactorResponse>> EnableTwoFactor()
    {
        var userId = User.GetUserId();
        var result = await _authService.EnableTwoFactorAsync(userId);
        if (result.IsError) return ErrorResult(result.Errors);
        return Ok(result.Value);
    }

    [HttpPost("2fa/verify")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyAndActivateTwoFactor([FromBody] VerifyTwoFactorRequest dto)
    {
        var userId = User.GetUserId();
        var result = await _authService.VerifyAndActivateTwoFactorAsync(userId, dto);
        if (result.IsError) return ErrorResult(result.Errors);
        return Ok(new { message = "Two-factor authentication enabled successfully." });
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorRequest dto)
    {
        var userId = User.GetUserId();
        var result = await _authService.DisableTwoFactorAsync(userId, dto);
        if (result.IsError) return ErrorResult(result.Errors);
        return Ok(new { message = "Two-factor authentication disabled." });
    }

    [HttpPost("2fa/login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> VerifyTwoFactorLogin([FromBody] VerifyTwoFactorLoginRequest dto)
    {
        var result = await _authService.VerifyTwoFactorLoginAsync(dto);
        if (result.IsError) return ErrorResult(result.Errors);
        return Ok(result.Value);
    }
}