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
    }
}
