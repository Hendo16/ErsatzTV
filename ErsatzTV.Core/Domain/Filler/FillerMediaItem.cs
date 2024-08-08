namespace ErsatzTV.Core.Domain.Filler;

public class FillerMediaItem : MediaItem
{
    public List<FillerMetadata> FillerMetadata { get; set; }
    public List<MediaVersion> MediaVersions { get; set; }
}
