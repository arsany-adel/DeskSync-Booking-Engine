namespace DeskSync.Api.Entities;

public enum RoomStatus
{
    OutOfService,
    Occupied,
    Free
}

public enum RoomRecommendedUse
{
    Events,
    Solo,
    Group,
}

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = null;
    public int NoOfChairs { get; set; } = 0;
    public RoomStatus Status { get; set; }
    public bool HasProjector { get; set; } = false;
    public bool HasBoard { get; set; } = false;
    public RoomRecommendedUse RecommendedUse { get; set; }
    public decimal PricePerHour { get; set; } = 0m;
}
