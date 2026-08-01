namespace DeskSync.Api.Entities;

public class Bookmark
{
    public Guid RoomId { get; private set; }
    public Guid UserId { get; private set; }

    public Bookmark(Guid roomId, Guid userId)
    {
        RoomId = roomId;
        UserId = userId;
    }

#pragma warning disable CS8618
    private Bookmark() { }
#pragma warning restore CS8618
}
