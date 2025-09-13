using ErsatzTV.Core.Domain;
using ErsatzTV.Core.Domain.Filler;
using ErsatzTV.Core.Metadata;

namespace ErsatzTV.Core.Interfaces.Repositories;

public interface IFillerRepository
{
    Task<bool> AllFillerExists(List<int> fillerIds);
    Task<Option<FillerMediaItem>> FindFillerByMovieTitle(string title);
    Task<Option<FillerMediaItem>> GetFiller(int fillerId);
    Task<Either<BaseError, MediaItemScanResult<FillerMediaItem>>> GetOrAdd(
        LibraryPath libraryPath,
        LibraryFolder libraryFolder,
        string path);

    Task<IEnumerable<string>> FindFillerPaths(LibraryPath libraryPath);
    Task<List<int>> DeleteByPath(LibraryPath libraryPath, string path);
    Task<bool> AddGenre(FillerMetadata metadata, Genre genre);
    Task<bool> AddTag(FillerMetadata metadata, Tag tag);
    Task<Unit> AddMovieMetadata(FillerMetadata metadata, MovieMetadata movieMetadata);

    Task<List<FillerMetadata>> GetFillerForCards(List<int> ids);
    Task<bool> AddStudio(FillerMetadata arg1, Studio arg2);
    Task<bool> AddActor(FillerMetadata arg1, Actor arg2);
}
