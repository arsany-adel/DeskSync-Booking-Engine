using System.Security.Authentication;
using DeskSync.Api.DTOs.Users;
using DeskSync.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Mvc;

namespace DeskSync.Api.Controllers;

[ApiController]
[Route("/api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterStandardUserRequestDto dto)
    {
        try
        {
            await _authService.RegisterStandardUserAsync(dto);
            return StatusCode(StatusCodes.Status201Created, new {Message = "Registeration Successful."});
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new {Error = ex.Message});
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AccessTokenResponse) ,StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserRequestDto dto)
    {
        try
        {
            var principal = await _authService.LoginStandardUserAsync(dto);
            return SignIn(principal, authenticationScheme: "Bearer");
        }
        catch (InvalidCredentialException ex)
        {
            return Unauthorized(new {Error = ex.Message});
        }
    }

    // This route expects the refresh token (and not the access token) in the Authorization header instead of a JSON body
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AccessTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        var result = await HttpContext.AuthenticateAsync("Bearer_Refresh");

        if (!result.Succeeded)
            return Unauthorized("Invalid or expired refresh token");

        return SignIn(result.Principal, authenticationScheme: "Bearer");
    }
}