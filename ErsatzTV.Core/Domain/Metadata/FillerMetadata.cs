using ErsatzTV.Core.Domain.Filler;

namespace ErsatzTV.Core.Domain;

public class FillerMetadata : Metadata
{
    public int FillerId { get; set; }
    public FillerMediaItem Filler { get; set; }
    public string Country { get; set; }
    public string Brand { get; set; }
    public string Product { get; set; }
    public List<Director> Directors { get; set; }
    public List<Writer> Writers { get; set; }
}
