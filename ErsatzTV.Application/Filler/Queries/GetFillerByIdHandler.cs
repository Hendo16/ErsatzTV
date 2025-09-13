using ErsatzTV.Core.Domain;
using ErsatzTV.Core.Domain.Filler;
using ErsatzTV.Core.Extensions;
using ErsatzTV.Core.Interfaces.Emby;
using ErsatzTV.Core.Interfaces.Jellyfin;
using ErsatzTV.Core.Interfaces.Plex;
using ErsatzTV.Core.Interfaces.Repositories;
using ErsatzTV.Infrastructure.Data;
using ErsatzTV.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using static ErsatzTV.Application.Filler.Mapper;

namespace ErsatzTV.Application.Filler;

public class GetFillerByIdHandler : IRequestHandler<GetFillerById, Option<FillerViewModel>>
{
    private readonly IDbContextFactory<TvContext> _dbContextFactory;
    private readonly IEmbyPathReplacementService _embyPathReplacementService;
    private readonly IJellyfinPathReplacementService _jellyfinPathReplacementService;
    private readonly IMediaSourceRepository _mediaSourceRepository;
    private readonly IFillerRepository _fillerRepository;
    private readonly IPlexPathReplacementService _plexPathReplacementService;

    public GetFillerByIdHandler(
        IDbContextFactory<TvContext> dbContextFactory,
        IFillerRepository fillerRepository,
        IMediaSourceRepository mediaSourceRepository,
        IPlexPathReplacementService plexPathReplacementService,
        IJellyfinPathReplacementService jellyfinPathReplacementService,
        IEmbyPathReplacementService embyPathReplacementService)
    {
        _dbContextFactory = dbContextFactory;
        _fillerRepository = fillerRepository;
        _mediaSourceRepository = mediaSourceRepository;
        _plexPathReplacementService = plexPathReplacementService;
        _jellyfinPathReplacementService = jellyfinPathReplacementService;
        _embyPathReplacementService = embyPathReplacementService;
    }

    public async Task<Option<FillerViewModel>> Handle(
        GetFillerById request,
        CancellationToken cancellationToken)
    {
        await using TvContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        Option<JellyfinMediaSource> maybeJellyfin = await _mediaSourceRepository.GetAllJellyfin()
            .Map(list => list.HeadOrNone());

        Option<EmbyMediaSource> maybeEmby = await _mediaSourceRepository.GetAllEmby()
            .Map(list => list.HeadOrNone());

        Option<FillerMediaItem> maybeFiller = await _fillerRepository.GetFiller(request.Id);

        Option<MediaVersion> maybeVersion = maybeFiller.Map(m => m.MediaVersions.HeadOrNone()).Flatten();
        var languageCodes = new List<string>();
        foreach (MediaVersion version in maybeVersion)
        {
            var mediaCodes = version.Streams
                .Filter(ms => ms.MediaStreamKind == MediaStreamKind.Audio)
                .Map(ms => ms.Language)
                .ToList();

            languageCodes.AddRange(await dbContext.LanguageCodes.GetAllLanguageCodes(mediaCodes));
        }

        foreach (FillerMediaItem filler in maybeFiller)
        {
            string localPath = await filler.GetLocalPath(
                _plexPathReplacementService,
                _jellyfinPathReplacementService,
                _embyPathReplacementService,
                false);
            return ProjectToViewModel(filler, localPath, languageCodes);
        }

        return None;
    }
}
