using DeskSync.Api.Entities;
using DeskSync.Api.DTOs.Users;

namespace DeskSync.Api.Extensions.Mappers;

public static class UserMapperExtension
{
    public static User ToEntity(this RegisterStandardUserRequestDto dto)
    {
        return new User(
            id: Guid.NewGuid(),
            username: dto.Username,
            firstName: dto.FirstName,
            lastName: dto.LastName,
            email: dto.Email,
            password: dto.Password,
            role: UserRole.Standard,
            emailNotificationEnabled: true
        );
    }
}
