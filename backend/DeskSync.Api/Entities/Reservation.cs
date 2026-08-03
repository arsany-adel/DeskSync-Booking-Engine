using NodaTime;

namespace DeskSync.Api.Entities;

public class Reservation
{
    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid UserId { get; private set; }

    // What the user intended
    public LocalDateTime LocalStartTime { get; private set; }
    public LocalDateTime LocalEndTime { get; private set; }
    public string TimezoneId { get; private set; }

    // For DB optimization
    public Instant UtcStartTime { get; private set; }
    public Instant UtcEndTime { get; private set; }
    public string TzdbVersion { get; private set; } = string.Empty;

    // Metadata
    public Instant CreatedAt { get; private set; }
    public string? Notes { get; private set; }

    public Reservation(
        Guid id,
        Guid roomId,
        Guid userId,
        LocalDateTime localStartTime,
        LocalDateTime localEndTime,
        string timezoneId,
        IDateTimeZoneProvider dateTimeZoneProvider,
        IClock clock,
        string? notes = null
    )
    {
        if (localStartTime > localEndTime)
            throw new ArgumentException("Start time must be before end time");

        if (string.IsNullOrWhiteSpace(timezoneId))
            throw new ArgumentException("Timezone ID is required");

        Id = id;
        RoomId = roomId;
        UserId = userId;
        LocalStartTime = localStartTime;
        LocalEndTime = localEndTime;
        TimezoneId = timezoneId;
        Notes = notes;

        CreatedAt = clock.GetCurrentInstant();

        CalculateUtcCaches(dateTimeZoneProvider);
    }

#pragma warning disable CS8618
    private Reservation() { }
#pragma warning restore CS8618

    private void CalculateUtcCaches(IDateTimeZoneProvider tzProvider)
    {
        var timezone = tzProvider[TimezoneId];
        UtcStartTime = LocalStartTime.InZoneStrictly(timezone).ToInstant();
        UtcEndTime = LocalEndTime.InZoneStrictly(timezone).ToInstant();
        TzdbVersion = tzProvider.VersionId;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
    }
}
