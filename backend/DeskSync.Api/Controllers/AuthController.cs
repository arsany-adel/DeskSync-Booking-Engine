using DeskSync.Api.DTOs.Users;
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

        if (result.IsError)
            return ErrorResult(result.Errors);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginUserRequestDto dto)
    {
        var result = await _authService.LoginStandardUserAsync(dto);

        if (result.IsError)
            return ErrorResult(result.Errors);

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);

        if (result.IsError)
            return ErrorResult(result.Errors);

        return Ok(result.Value);
    }
}