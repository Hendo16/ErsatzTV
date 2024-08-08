using ErsatzTV.Core.Domain;

namespace ErsatzTV.Application.MediaCards;

public record FillerCardViewModel(
    int FillerId,
    string Title,
    string Subtitle,
    string SortTitle,
    string Poster,
    MediaItemState State) : MediaCardViewModel(
    FillerId,
    Title,
    Subtitle,
    SortTitle,
    Poster,
    State)
{
    public int CustomIndex { get; set; }
}