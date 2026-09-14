using System.Security.Claims;
using DeskSync.Api.DTOs.Users;

namespace DeskSync.Api.Services.Interfaces;

public interface IAuthService
{
    Task RegisterStandardUserAsync(RegisterStandardUserRequestDto dto);

    Task<ClaimsPrincipal> LoginStandardUserAsync(LoginUserRequestDto dto);    
}
