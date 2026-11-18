using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Takwene.Domain.Entities;

namespace Takwene.Infrastructure.Persistence.Configurations;

internal sealed class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.ToTable("Tracks");

        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Isrc).HasMaxLength(12).IsRequired();
        builder.Property(t => t.Genre).HasMaxLength(50).IsRequired();

        // Stored as text ("Draft") so the data reads well.
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(t => t.Artist)
            .WithMany(a => a.Tracks)
            .HasForeignKey(t => t.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.Isrc).IsUnique();

        // Covers the artist-scoped list and the status filter; also serves as the ArtistId FK index.
        builder.HasIndex(t => new { t.ArtistId, t.Status });

        // Trigram indexes for ILIKE '%term%' search.
        builder.HasIndex(t => t.Title, "IX_Tracks_Title_Trgm").HasMethod("gin").HasOperators("gin_trgm_ops");
        builder.HasIndex(t => t.Isrc, "IX_Tracks_Isrc_Trgm").HasMethod("gin").HasOperators("gin_trgm_ops");
    }
}
