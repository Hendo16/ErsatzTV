using ErsatzTV.Core.Domain;
using ErsatzTV.Core.Domain.Filler;
using ErsatzTV.Core.Metadata;

namespace ErsatzTV.Core.Interfaces.Repositories;

public interface IMediaServerFillerRepository<in TLibrary, TFiller, TEtag> where TLibrary : Library
    where TFiller : FillerMediaItem
    where TEtag : MediaServerItemEtag
{
    Task<List<TEtag>> GetExistingFillers(TLibrary library);
    Task<Option<int>> FlagNormal(TLibrary library, TFiller filler);
    Task<Option<int>> FlagUnavailable(TLibrary library, TFiller filler);
    Task<Option<int>> FlagRemoteOnly(TLibrary library, TFiller filler);
    Task<List<int>> FlagFileNotFound(TLibrary library, List<string> movieItemIds);
    Task<Either<BaseError, MediaItemScanResult<TFiller>>> GetOrAdd(TLibrary library, TFiller item, bool deepScan);
    Task<Unit> SetEtag(TFiller filler, string etag);
}