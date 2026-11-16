using NodaTime;

namespace DeskSync.Api.DTOs;

public record NotificationResponseDto(
    Guid Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    Instant CreatedAt
);