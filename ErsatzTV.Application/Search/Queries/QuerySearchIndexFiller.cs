using ErsatzTV.Application.MediaCards;

namespace ErsatzTV.Application.Search;

public record QuerySearchIndexFiller(string Query, int PageNumber, int PageSize)
    : IRequest<FillerCardResultsViewModel>;

