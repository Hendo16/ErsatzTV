using ErsatzTV.Core.Domain;

namespace ErsatzTV.Application.Filler;

public record FillerViewModel(
    string Title,
    string Year,
    List<Tag> Tags,
    List<string> ContentRatings,
    string MovieId,
    string Path,
    string LocalPath,
    MediaItemState MediaItemState)
{
    public string Poster { get; set; }
    public string FanArt { get; set; }

    public static bool SaveTag(Tag newTag)
    {

        return false;
    }
}
