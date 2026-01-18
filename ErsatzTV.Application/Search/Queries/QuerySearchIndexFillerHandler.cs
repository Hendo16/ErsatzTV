using Bugsnag;
using ErsatzTV.Application.MediaCards;
using ErsatzTV.Core.Interfaces.Search;
using ErsatzTV.Core.Search;
using ErsatzTV.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static ErsatzTV.Application.MediaCards.Mapper;

namespace ErsatzTV.Application.Search;

public class QuerySearchIndexFillerHandler(
    IClient client,
    ISearchIndex searchIndex,
    IDbContextFactory<TvContext> dbContextFactory) : IRequestHandler<QuerySearchIndexFiller,
    FillerCardResultsViewModel>
{
    public async Task<FillerCardResultsViewModel> Handle(
        QuerySearchIndexFiller request,
        CancellationToken cancellationToken)
    {
        SearchResult searchResult = await searchIndex.Search(
            client,
            request.Query,
            string.Empty,
            (request.PageNumber - 1) * request.PageSize,
            request.PageSize,
            cancellationToken);

        await using TvContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var ids = searchResult.Items.Map(i => i.Id).ToHashSet();

        List<FillerCardViewModel> items = await dbContext.FillerMetadata
            .AsNoTracking()
            .Filter(fm => ids.Contains(fm.Id))
            .Include(fm => fm.Filler)
            .Include(fm => fm.Artwork)
            .Include(fm => fm.Filler)
            .ThenInclude(f => f.MediaVersions)
            .ThenInclude(mv => mv.MediaFiles)
            .OrderBy(fm => fm.SortTitle)
            .ToListAsync(cancellationToken)
            .Map(list => list.Map(ProjectToViewModel).ToList());

        return new FillerCardResultsViewModel(searchResult.TotalCount, items, searchResult.PageMap);
    }
}
