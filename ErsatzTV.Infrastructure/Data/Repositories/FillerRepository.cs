using Dapper;
using ErsatzTV.Core;
using ErsatzTV.Core.Domain;
using ErsatzTV.Core.Domain.Filler;
using ErsatzTV.Core.Errors;
using ErsatzTV.Core.Interfaces.Repositories;
using ErsatzTV.Core.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ErsatzTV.Infrastructure.Data.Repositories;

public class FillerRepository : IFillerRepository
{
    private readonly IDbContextFactory<TvContext> _dbContextFactory;
    private readonly ILogger<FillerRepository> _logger;

    public FillerRepository(IDbContextFactory<TvContext> dbContextFactory, ILogger<FillerRepository> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<Either<BaseError, MediaItemScanResult<FillerMediaItem>>> GetOrAdd(
        LibraryPath libraryPath,
        LibraryFolder libraryFolder,
        string path)
    {
        await using TvContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        Option<FillerMediaItem> maybeExisting = await dbContext.FillerMediaItems
            .AsNoTracking()
            .Include(i => i.FillerMetadata)
            .ThenInclude(ovm => ovm.Genres)
            .Include(i => i.FillerMetadata)
            .ThenInclude(ovm => ovm.Tags)
            .Include(i => i.FillerMetadata)
            .ThenInclude(ovm => ovm.Studios)
            .Include(i => i.FillerMetadata)
            .ThenInclude(ovm => ovm.Guids)
            .Include(i => i.FillerMetadata)
            .ThenInclude(ovm => ovm.Actors)
            .Include(i => i.FillerMetadata)
            .ThenInclude(ovm => ovm.Actors)
            .ThenInclude(a => a.Artwork)
            .Include(ov => ov.LibraryPath)
            .ThenInclude(lp => lp.Library)
            .Include(ov => ov.MediaVersions)
            .ThenInclude(ov => ov.MediaFiles)
            .Include(ov => ov.MediaVersions)
            .ThenInclude(ov => ov.Streams)
            .Include(ov => ov.TraktListItems)
            .ThenInclude(tli => tli.TraktList)
            .OrderBy(i => i.MediaVersions.First().MediaFiles.First().Path)
            .SingleOrDefaultAsync(i => i.MediaVersions.First().MediaFiles.First().Path == path);

        return await maybeExisting.Match(
            mediaItem =>
                Right<BaseError, MediaItemScanResult<FillerMediaItem>>(
                    new MediaItemScanResult<FillerMediaItem>(mediaItem) { IsAdded = false }).AsTask(),
            async () => await AddFiller(dbContext, libraryPath.Id, libraryFolder.Id, path));
    }

    public async Task<IEnumerable<string>> FindFillerPaths(LibraryPath libraryPath)
    {
        await using TvContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Connection.QueryAsync<string>(
            @"SELECT MF.Path
                FROM MediaFile MF
                INNER JOIN MediaVersion MV on MF.MediaVersionId = MV.Id
                INNER JOIN FillerMediaItem O on MV.FillerMediaItemId = O.Id
                INNER JOIN MediaItem MI on O.Id = MI.Id
                WHERE MI.LibraryPathId = @LibraryPathId",
            new { LibraryPathId = libraryPath.Id });
    }

    public async Task<List<int>> DeleteByPath(LibraryPath libraryPath, string path)
    {
        await using TvContext dbContext = await _dbContextFactory.CreateDbContextAsync();

        List<int> ids = await dbContext.Connection.QueryAsync<int>(
            @"SELECT O.Id
            FROM FillerMediaItem O
            INNER JOIN MediaItem MI on O.Id = MI.Id
            INNER JOIN MediaVersion MV on O.Id = MV.FillerId
            INNER JOIN MediaFile MF on MV.Id = MF.MediaVersionId
            WHERE MI.LibraryPathId = @LibraryPathId AND MF.Path = @Path",
            new { LibraryPathId = libraryPath.Id, Path = path }).Map(result => result.ToList());

        foreach (int fillerId in ids)
        {
            FillerMediaItem filler = await dbContext.FillerMediaItems.FindAsync(fillerId);
            if (filler != null)
            {
                dbContext.FillerMediaItems.Remove(filler);
            }
        }

        await dbContext.SaveChangesAsync();

        return ids;
    }

    public async Task<bool> AddGenre(FillerMetadata metadata, Genre genre)
    {
        await using TvContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Connection.ExecuteAsync(
            "INSERT INTO Genre (Name, FillerMetadataId) VALUES (@Name, @MetadataId)",
            new { genre.Name, MetadataId = metadata.Id }).Map(result => result > 0);
    }

    public async Task<bool> AddTag(FillerMetadata metadata, Tag tag)
    {
        await using TvContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Connection.ExecuteAsync(
            "INSERT INTO Tag (Name, FillerMetadataId, ExternalCollectionId) VALUES (@Name, @MetadataId, @ExternalCollectionId)",
            new { tag.Name, MetadataId = metadata.Id, tag.ExternalCollectionId }).Map(result => result > 0);
    }

    public async Task<bool> AddStudio(FillerMetadata metadata, Studio studio)
    {
        await using TvContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Connection.ExecuteAsync(
            "INSERT INTO Studio (Name, FillerMetadataId) VALUES (@Name, @MetadataId)",
            new { studio.Name, MetadataId = metadata.Id }).Map(result => result > 0);
    }

    public async Task<bool> AddActor(FillerMetadata metadata, Actor actor)
    {
        await using TvContext dbContext = await _dbContextFactory.CreateDbContextAsync();

        int? artworkId = null;

        if (actor.Artwork != null)
        {
            artworkId = await dbContext.Connection.QuerySingleAsync<int>(
                $"""
                 INSERT INTO Artwork (ArtworkKind, DateAdded, DateUpdated, Path)
                     VALUES (@ArtworkKind, @DateAdded, @DateUpdated, @Path);
                 SELECT {TvContext.LastInsertedRowId}
                 """,
                new
                {
                    ArtworkKind = (int)actor.Artwork.ArtworkKind,
                    actor.Artwork.DateAdded,
                    actor.Artwork.DateUpdated,
                    actor.Artwork.Path
                });
        }

        return await dbContext.Connection.ExecuteAsync(
                "INSERT INTO Actor (Name, Role, `Order`, FillerMetadataId, ArtworkId) VALUES (@Name, @Role, @Order, @MetadataId, @ArtworkId)",
                new { actor.Name, actor.Role, actor.Order, MetadataId = metadata.Id, ArtworkId = artworkId })
            .Map(result => result > 0);
    }

    public async Task<bool> AddDirector(FillerMetadata metadata, Director director)
    {
        await using TvContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Connection.ExecuteAsync(
            "INSERT INTO Director (Name, FillerMetadataId) VALUES (@Name, @MetadataId)",
            new { director.Name, MetadataId = metadata.Id }).Map(result => result > 0);
    }

    public async Task<bool> AddWriter(FillerMetadata metadata, Writer writer)
    {
        await using TvContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Connection.ExecuteAsync(
            "INSERT INTO Writer (Name, FillerMetadataId) VALUES (@Name, @MetadataId)",
            new { writer.Name, MetadataId = metadata.Id }).Map(result => result > 0);
    }

    public async Task<List<FillerMetadata>> GetFillerForCards(List<int> ids)
    {
        await using TvContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.FillerMetadata
            .AsNoTracking()
            .Filter(ovm => ids.Contains(ovm.FillerId))
            .Include(ovm => ovm.Filler)
            .Include(ovm => ovm.Artwork)
            .Include(ovm => ovm.Filler)
            .ThenInclude(ov => ov.MediaVersions)
            .ThenInclude(mv => mv.MediaFiles)
            .OrderBy(ovm => ovm.SortTitle)
            .ToListAsync();
    }

    private async Task<Either<BaseError, MediaItemScanResult<FillerMediaItem>>> AddFiller(
        TvContext dbContext,
        int libraryPathId,
        int libraryFolderId,
        string path)
    {
        try
        {
            if (await MediaItemRepository.MediaFileAlreadyExists(path, libraryPathId, dbContext, _logger))
            {
                return new MediaFileAlreadyExists();
            }

            var filler = new FillerMediaItem
            {
                LibraryPathId = libraryPathId,
                MediaVersions =
                [
                    new MediaVersion
                    {
                        MediaFiles = [new MediaFile { Path = path, LibraryFolderId = libraryFolderId }],
                        Streams = []
                    }
                ],
                TraktListItems = []
            };

            await dbContext.FillerMediaItems.AddAsync(filler);
            await dbContext.SaveChangesAsync();
            await dbContext.Entry(filler).Reference(m => m.LibraryPath).LoadAsync();
            await dbContext.Entry(filler.LibraryPath).Reference(lp => lp.Library).LoadAsync();
            return new MediaItemScanResult<FillerMediaItem>(filler) { IsAdded = true };
        }
        catch (Exception ex)
        {
            return BaseError.New(ex.Message);
        }
    }
}
