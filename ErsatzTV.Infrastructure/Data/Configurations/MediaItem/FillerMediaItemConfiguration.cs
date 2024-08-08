using ErsatzTV.Core.Domain.Filler;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ErsatzTV.Infrastructure.Data.Configurations;

public class FillerMediaItemConfiguration : IEntityTypeConfiguration<FillerMediaItem>
{
    public void Configure(EntityTypeBuilder<FillerMediaItem> builder)
    {
        builder.ToTable("FillerMediaItem");

        builder.HasMany(m => m.FillerMetadata)
            .WithOne(m => m.Filler)
            .HasForeignKey(m => m.FillerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.MediaVersions)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}