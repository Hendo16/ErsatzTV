using ErsatzTV.Core.Domain;
using ErsatzTV.Core.Domain.Filler;
using ErsatzTV.Core.Metadata;

namespace ErsatzTV.Core.Interfaces.Repositories;

public interface IFillerRepository
{
    Task<Either<BaseError, MediaItemScanResult<FillerMediaItem>>> GetOrAdd(
        LibraryPath libraryPath,
        LibraryFolder libraryFolder,
        string path);

    Task<IEnumerable<string>> FindFillerPaths(LibraryPath libraryPath);
    Task<List<int>> DeleteByPath(LibraryPath libraryPath, string path);
    Task<bool> AddGenre(FillerMetadata metadata, Genre genre);
    Task<bool> AddTag(FillerMetadata metadata, Tag tag);
    Task<bool> AddStudio(FillerMetadata metadata, Studio studio);
    Task<bool> AddActor(FillerMetadata metadata, Actor actor);
    Task<bool> AddDirector(FillerMetadata metadata, Director director);
    Task<bool> AddWriter(FillerMetadata metadata, Writer writer);

    Task<List<FillerMetadata>> GetFillerForCards(List<int> ids);
}
