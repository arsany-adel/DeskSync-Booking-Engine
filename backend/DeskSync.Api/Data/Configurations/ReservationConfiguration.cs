using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeskSync.Api.Entities;

namespace DeskSync.Api.Data.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoomId)
               .IsRequired();

        builder.Property(r => r.UserId)
               .IsRequired();

        builder.HasOne<Room>()
               .WithMany()
               .HasForeignKey(r => r.RoomId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.LocalStartTime)
               .IsRequired();

        builder.Property(r => r.LocalEndTime)
               .IsRequired();

        builder.Property(r => r.TimezoneId)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(r => r.UtcStartTime)
               .IsRequired();

        builder.Property(r => r.UtcEndTime)
               .IsRequired();

        builder.Property(r => r.TzdbVersion)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(r => r.CreatedAt)
               .IsRequired();

        builder.Property(r => r.Notes)
               .IsRequired(false)
               .HasMaxLength(1000);
    }
}
