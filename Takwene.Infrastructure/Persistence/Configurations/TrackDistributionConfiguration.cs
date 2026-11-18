using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Takwene.Domain.Entities;

namespace Takwene.Infrastructure.Persistence.Configurations;

internal sealed class TrackDistributionConfiguration : IEntityTypeConfiguration<TrackDistribution>
{
    public void Configure(EntityTypeBuilder<TrackDistribution> builder)
    {
        builder.ToTable("TrackDistributions");

        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);

        // One row per track per DSP; also serves as the TrackId FK index.
        builder.HasIndex(d => new { d.TrackId, d.DspId }).IsUnique();

        builder.HasOne(d => d.Track)
            .WithMany(t => t.Distributions)
            .HasForeignKey(d => d.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Dsp)
            .WithMany()
            .HasForeignKey(d => d.DspId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
