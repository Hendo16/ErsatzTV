using ErsatzTV.Core.Search;

namespace ErsatzTV.Application.MediaCards;

public record FillerCardResultsViewModel(
    int Count,
    List<FillerCardViewModel> Cards,
    SearchPageMap PageMap);
