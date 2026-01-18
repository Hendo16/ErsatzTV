namespace ErsatzTV.Scanner.Core.Metadata.Nfo;

public class FillerNfo
{
    public FillerNfo()
    {
        Genres = new List<string>();
        Studios = new List<string>();
        Tags = new List<string>();
        UniqueIds = new List<UniqueIdNfo>();
    }

    public string? Title { get; set; }
    public string? SortTitle { get; set; }
    public string? Outline { get; set; }
    public int Year { get; set; }
    public string? ContentRating { get; set; }
    public Option<DateTime> Premiered { get; set; }
    public string? Plot { get; set; }
    public string? Tagline { get; set; }
    public List<string> Genres { get; }
    public List<string> Studios { get; }
    public List<string> Tags { get; }
    public List<UniqueIdNfo> UniqueIds { get; }
}
