using NodaTime;

namespace DeskSync.Api.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string Token { get; private set; }
    public Instant CreatedAt { get; private set; }
    public Instant ExpiresAt { get; private set; }
    public Instant? RevokedAt { get; private set; }

    public bool IsActive => RevokedAt == null && ExpiresAt > SystemClockNow();

    private static Instant SystemClockNow() => NodaTime.SystemClock.Instance.GetCurrentInstant();

    public RefreshToken(Guid id, Guid userId, string token, Instant createdAt, Instant expiresAt)
    {
        Id = id;
        UserId = userId;
        Token = token;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

#pragma warning disable CS8618
    private RefreshToken() { }
#pragma warning restore CS8618

    public void Revoke()
    {
        if (RevokedAt == null)
            RevokedAt = SystemClockNow();
    }
}