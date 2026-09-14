using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeskSync.Api.Entities;

namespace DeskSync.Api.Data.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.WorkspaceId)
               .IsRequired();

        builder.HasOne<Workspace>()
               .WithMany()
               .HasForeignKey(r => r.WorkspaceId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(r => r.Description)
               .IsRequired(false)
               .HasMaxLength(1000);

        builder.Property(r => r.NoOfChairs)
               .IsRequired()
               .HasDefaultValue(1);

        builder.Property(r => r.Status)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.Property(r => r.HasProjector)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(r => r.HasBoard)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(r => r.RecommendedUse)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.Property(r => r.PricePerHour)
               .IsRequired()
               .HasPrecision(10, 2)
               .HasDefaultValue(0m);
       
       //Save the enum as text (a string) in the DB instead of an integer
       builder.Property(r => r.Status)
               .HasConversion<string>();

        builder.Property(r => r.RecommendedUse)
               .HasConversion<string>();
       

    }
}
