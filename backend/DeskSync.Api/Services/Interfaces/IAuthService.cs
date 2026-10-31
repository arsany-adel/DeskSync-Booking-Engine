using DeskSync.Api.DTOs.Users;
using ErrorOr;

namespace DeskSync.Api.Services.Interfaces;

public interface IAuthService
{
    Task<ErrorOr<AuthResponse>> RegisterStandardUserAsync(RegisterStandardUserRequestDto dto);
    Task<ErrorOr<AuthResponse>> LoginStandardUserAsync(LoginUserRequestDto dto);
    Task<ErrorOr<AuthResponse>> RefreshTokenAsync(string refreshToken);
}