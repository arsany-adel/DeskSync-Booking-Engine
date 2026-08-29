using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeskSync.Api.Entities;

namespace DeskSync.Api.Data.Configurations;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(w => w.Description)
               .IsRequired(false)
               .HasMaxLength(1000);

        builder.Property(w => w.LogoUrl)
               .IsRequired(false)
               .HasMaxLength(1000);

        builder.Property(w => w.Address)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(w => w.GoogleMapsLocation)
               .IsRequired(false)
               .HasMaxLength(1000);

        // Database Seeding
        builder.HasData(
            new Workspace(
                id: Guid.Parse("11111111-1111-1111-1111-111111111111"), 
                name: "Default Workspace",
                description: "The primary workspace configured via database seeding.",
                logoUrl: null,
                address: "Main HQ Address",
                googleMapsLocation: null
            )
        );
    }
}