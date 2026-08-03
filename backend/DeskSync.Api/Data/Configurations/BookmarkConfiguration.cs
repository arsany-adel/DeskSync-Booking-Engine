using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeskSync.Api.Entities;

namespace DeskSync.Api.Data.Configurations;

public class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        builder.HasKey(b => new { b.RoomId, b.UserId });

        builder.Property(b => b.RoomId)
               .IsRequired();

        builder.Property(b => b.UserId)
               .IsRequired();

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(b => b.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Room>()
               .WithMany()
               .HasForeignKey(b => b.RoomId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
