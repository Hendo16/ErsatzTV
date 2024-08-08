using Bugsnag;
using ErsatzTV.Application.MediaCards;
using ErsatzTV.Core.Interfaces.Repositories;
using ErsatzTV.Core.Interfaces.Search;
using ErsatzTV.Core.Search;
using static ErsatzTV.Application.MediaCards.Mapper;

namespace ErsatzTV.Application.Search;

public class QuerySearchIndexFillerHandler : IRequestHandler<QuerySearchIndexFiller,
    FillerCardResultsViewModel>
{
    private readonly IClient _client;
    private readonly IFillerRepository _fillerRepository;
    private readonly ISearchIndex _searchIndex;

    public QuerySearchIndexFillerHandler(
        IClient client,
        ISearchIndex searchIndex,
        IFillerRepository fillerRepository)
    {
        _client = client;
        _searchIndex = searchIndex;
        _fillerRepository = fillerRepository;
    }

    public async Task<FillerCardResultsViewModel> Handle(
        QuerySearchIndexFiller request,
        CancellationToken cancellationToken)
    {
        SearchResult searchResult = await _searchIndex.Search(
            _client,
            request.Query,
            (request.PageNumber - 1) * request.PageSize,
            request.PageSize);

        List<FillerCardViewModel> items = await _fillerRepository
            .GetFillerForCards(searchResult.Items.Map(i => i.Id).ToList())
            .Map(list => list.Map(ProjectToViewModel).ToList());

        return new FillerCardResultsViewModel(searchResult.TotalCount, items, searchResult.PageMap);
    }
}