using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Takwene.Domain.Entities;
using Takwene.Infrastructure.Identity;

namespace Takwene.Infrastructure.Persistence.Configurations;

internal sealed class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.ToTable("Artists");

        builder.Property(a => a.Name).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Email).HasMaxLength(256).IsRequired();
        builder.Property(a => a.Country).HasMaxLength(100).IsRequired();

        builder.HasIndex(a => a.Email).IsUnique();

        // Trigram index for case-insensitive "contains" search on artist name.
        builder.HasIndex(a => a.Name).HasMethod("gin").HasOperators("gin_trgm_ops");

        // One login per artist; the FK targets Identity without a navigation in Domain.
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Artist>(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
