using ErsatzTV.Core.Domain.Filler;

namespace ErsatzTV.Core.Domain;

public class FillerMetadata : Metadata
{
    public int FillerId { get; set; }
    public FillerMediaItem Filler { get; set; }
    public string ContentRating { get; set; }
    public int? MovieId { get; set; }
    public int? ShowId { get; set; }
}