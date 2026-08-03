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
    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int NoOfChairs { get; private set; }
    public RoomStatus Status { get; private set; }
    public bool HasProjector { get; private set; }
    public bool HasBoard { get; private set; }
    public RoomRecommendedUse RecommendedUse { get; private set; }
    public decimal PricePerHour { get; private set; }

    public Room (
        Guid id,
        Guid workspaceId,
        string name,
        string? description,
        int noOfChairs,
        decimal pricePerHour,
        RoomStatus status = RoomStatus.Free,
        bool hasProjector = false,
        bool hasBoard = false,
        RoomRecommendedUse recommendedUse = RoomRecommendedUse.Solo
    )
    {   
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        if (noOfChairs < 1)
            throw new ArgumentException("Number of chairs must be at least 1");

        if (pricePerHour < 0)
            throw new ArgumentException("Price per hour must be at least 0");

        Id = id;
        WorkspaceId = workspaceId;
        Name = name;
        Description = description;
        NoOfChairs = noOfChairs;
        Status = status;
        HasProjector = hasProjector;
        HasBoard = hasBoard;
        RecommendedUse = recommendedUse;
        PricePerHour = pricePerHour;
    }

#pragma warning disable CS8618 
    private Room() { }
#pragma warning restore CS8618

    public void UpdateRoom(
        string name,
        string? description,
        int noOfChairs,
        decimal pricePerHour,
        RoomStatus status = RoomStatus.Free,
        bool hasProjector = false,
        bool hasBoard = false,
        RoomRecommendedUse recommendedUse = RoomRecommendedUse.Solo
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        if (noOfChairs < 1)
            throw new ArgumentException("Number of chairs must be at least 1");

        if (pricePerHour < 0)
            throw new ArgumentException("Price per hour must be at least 0");

        Name = name;
        Description = description;
        NoOfChairs = noOfChairs;
        Status = status;
        HasProjector = hasProjector;
        HasBoard = hasBoard;
        RecommendedUse = recommendedUse;
        PricePerHour = pricePerHour;
    }
}
