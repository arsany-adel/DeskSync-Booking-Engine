using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeskSync.Api.Entities;

namespace DeskSync.Api.Data.Configurations;

public class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
{
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        builder.HasKey(l => new { l.Provider, l.ProviderKey });

        builder.Property(l => l.Provider)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.Property(l => l.ProviderKey)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(l => l.UserId)
               .IsRequired();

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(l => l.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
