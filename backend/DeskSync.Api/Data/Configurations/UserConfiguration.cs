using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeskSync.Api.Entities;

namespace DeskSync.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Role)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(50)
               .HasDefaultValue(UserRole.Standard);

        builder.Property(u => u.FirstName)
               .IsRequired(false)
               .HasMaxLength(100);

        builder.Property(u => u.LastName)
               .IsRequired(false)
               .HasMaxLength(100);

        builder.Property(u => u.Username)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(u => u.Password)
               .IsRequired(false)
               .HasMaxLength(100);

        builder.Property(u => u.EmailNotificationEnabled)
               .IsRequired()
               .HasDefaultValue(false);
    }
}