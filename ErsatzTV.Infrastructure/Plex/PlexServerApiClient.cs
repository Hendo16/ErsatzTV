﻿using System.Globalization;
using System.Xml.Serialization;
using ErsatzTV.Core;
using ErsatzTV.Core.Domain;
using ErsatzTV.Core.Interfaces.Plex;
using ErsatzTV.Core.Metadata;
using ErsatzTV.Core.Plex;
using ErsatzTV.Infrastructure.Plex.Models;
using Microsoft.Extensions.Logging;
using Refit;

namespace ErsatzTV.Infrastructure.Plex;

public class PlexServerApiClient : IPlexServerApiClient
{
    private readonly ILogger<PlexServerApiClient> _logger;
    private readonly PlexEtag _plexEtag;

    public PlexServerApiClient(PlexEtag plexEtag, ILogger<PlexServerApiClient> logger)
    {
        _plexEtag = plexEtag;
        _logger = logger;
    }

    public async Task<bool> Ping(PlexConnection connection, PlexServerAuthToken token)
    {
        try
        {
            IPlexServerApi service = XmlServiceFor(connection.Uri, TimeSpan.FromSeconds(5));
            PlexXmlMediaContainerPingResponse pingResult = await service.Ping(token.AuthToken);
            return token.ClientIdentifier == pingResult.MachineIdentifier;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<Either<BaseError, List<PlexLibrary>>> GetLibraries(
        PlexConnection connection,
        PlexServerAuthToken token)
    {
        try
        {
            IPlexServerApi service = RestService.For<IPlexServerApi>(
                new HttpClient
                {
                    BaseAddress = new Uri(connection.Uri),
                    Timeout = TimeSpan.FromSeconds(10)
                });
            List<PlexLibraryResponse> directory =
                await service.GetLibraries(token.AuthToken).Map(r => r.MediaContainer.Directory);
            return directory
                // .Filter(l => l.Hidden == 0)
                .Filter(l => (l.Agent ?? string.Empty).ToLowerInvariant() is not "com.plexapp.agents.none")
                .Filter(l => l.Type.ToLowerInvariant() is "movie" or "show")
                .Map(Project)
                .Somes()
                .ToList();
        }
        catch (Exception ex)
        {
            return BaseError.New(ex.ToString());
        }
    }

    public IAsyncEnumerable<PlexMovie> GetMovieLibraryContents(
        PlexLibrary library,
        PlexConnection connection,
        PlexServerAuthToken token)
    {
        Task<PlexXmlMediaContainerStatsResponse> CountItems(IPlexServerApi service)
        {
            return service.GetLibrarySection(library.Key, token.AuthToken);
        }

        Task<IEnumerable<PlexMovie>> GetItems(IPlexServerApi _, IPlexServerApi jsonService, int skip, int pageSize)
        {
            return jsonService
                .GetLibrarySectionContents(library.Key, skip, pageSize, token.AuthToken)
                .Map(r => r.MediaContainer.Metadata.Filter(m => m.Media.Count > 0 && m.Media[0].Part.Count > 0))
                .Map(list => list.Map(metadata => ProjectToMovie(metadata, library.MediaSourceId)));
        }

        return GetPagedLibraryContents(connection, CountItems, GetItems);
    }

    public IAsyncEnumerable<PlexShow> GetShowLibraryContents(
        PlexLibrary library,
        PlexConnection connection,
        PlexServerAuthToken token)
    {
        Task<PlexXmlMediaContainerStatsResponse> CountItems(IPlexServerApi service)
        {
            return service.GetLibrarySection(library.Key, token.AuthToken);
        }

        Task<IEnumerable<PlexShow>> GetItems(IPlexServerApi _, IPlexServerApi jsonService, int skip, int pageSize)
        {
            return jsonService
                .GetLibrarySectionContents(library.Key, skip, pageSize, token.AuthToken)
                .Map(r => r.MediaContainer.Metadata ?? new List<PlexMetadataResponse>())
                .Map(list => list.Map(metadata => ProjectToShow(metadata, library.MediaSourceId)));
        }

        return GetPagedLibraryContents(connection, CountItems, GetItems);
    }

    public async Task<Either<BaseError, int>> CountShowSeasons(
        PlexShow show,
        PlexConnection connection,
        PlexServerAuthToken token)
    {
        try
        {
            string showMetadataKey = show.Key.Split("/").Reverse().Skip(1).Head();
            IPlexServerApi service = XmlServiceFor(connection.Uri);
            return await service.CountShowChildren(showMetadataKey, token.AuthToken).Map(r => r.TotalSize);
        }
        catch (Exception ex)
        {
            return BaseError.New(ex.ToString());
        }
    }

    public IAsyncEnumerable<PlexSeason> GetShowSeasons(
        PlexLibrary library,
        PlexShow show,
        PlexConnection connection,
        PlexServerAuthToken token)
    {
        string showMetadataKey = show.Key.Split("/").Reverse().Skip(1).Head();

        Task<PlexXmlMediaContainerStatsResponse> CountItems(IPlexServerApi service)
        {
            return service.CountShowChildren(showMetadataKey, token.AuthToken);
        }

        Task<IEnumerable<PlexSeason>> GetItems(IPlexServerApi xmlService, IPlexServerApi _, int skip, int pageSize)
        {
            return xmlService.GetShowChildren(showMetadataKey, skip, pageSize, token.AuthToken)
                .Map(r => r.Metadata.Filter(m => !m.Key.Contains("allLeaves")))
                .Map(list => list.Map(metadata => ProjectToSeason(metadata, library.MediaSourceId)));
        }

        return GetPagedLibraryContents(connection, CountItems, GetItems);
    }

    public async Task<Either<BaseError, int>> CountSeasonEpisodes(
        PlexSeason season,
        PlexConnection connection,
        PlexServerAuthToken token)
    {
        try
        {
            string seasonMetadataKey = season.Key.Split("/").Reverse().Skip(1).Head();
            IPlexServerApi service = XmlServiceFor(connection.Uri);
            return await service.CountSeasonChildren(seasonMetadataKey, token.AuthToken).Map(r => r.TotalSize);
        }
        catch (Exception ex)
        {
            return BaseError.New(ex.ToString());
        }
    }

    public IAsyncEnumerable<PlexEpisode> GetSeasonEpisodes(
        PlexLibrary library,
        PlexSeason season,
        PlexConnection connection,
        PlexServerAuthToken token)
    {
        string seasonMetadataKey = season.Key.Split("/").Reverse().Skip(1).Head();

        Task<PlexXmlMediaContainerStatsResponse> CountItems(IPlexServerApi service)
        {
            return service.CountSeasonChildren(seasonMetadataKey, token.AuthToken);
        }

        Task<IEnumerable<PlexEpisode>> GetItems(IPlexServerApi xmlService, IPlexServerApi _, int skip, int pageSize)
        {
            return xmlService.GetSeasonChildren(seasonMetadataKey, skip, pageSize, token.AuthToken)
                .Map(r => r.Metadata.Filter(m => m.Media.Count > 0 && m.Media[0].Part.Count > 0))
                .Map(list => list.Map(metadata => ProjectToEpisode(metadata, library.MediaSourceId)));
        }

        return GetPagedLibraryContents(connection, CountItems, GetItems);
    }

    public async Task<Either<BaseError, MovieMetadata>> GetMovieMetadata(
        PlexLibrary library,
        string key,
        PlexConnection connection,
        PlexServerAuthToken token)
    {
        try
        {
            IPlexServerApi service = XmlServiceFor(connection.Uri);
            return await service.GetVideoMetadata(key, token.AuthToken)
                .Map(Optional)
                .Map(r => r.Filter(m => m.Metadata.Media.Count > 0 && m.Metadata.Media[0].Part.Count > 0))
                .MapT(response => ProjectToMovieMetadata(response.Metadata, library.MediaSourceId))
                .Map(o => o.ToEither<BaseError>("Unable to locate metadata"));
        }
        catch (Exception ex)
        {
            return BaseError.New(ex.ToString());
        }
    }

    public async Task<Either<BaseError, ShowMetadata>> GetShowMetadata(
        PlexLibrary library,
        string key,
        PlexConnection connection,
        PlexServerAuthToken token)
    {
        try
        {
            IPlexServerApi service = XmlServiceFor(connection.Uri);
            return await service.GetDirectoryMetadata(key, token.AuthToken)
                .Map(Optional)
                .MapT(response => ProjectToShowMetadata(response.Metadata, library.MediaSourceId))
                .Map(o => o.ToEither<BaseError>("Unable to locate metadata"));
        }
        catch (Exception ex)
        {
            return BaseError.New(ex.ToString());
        }
    }

    public async Task<Either<BaseError, Tuple<MovieMetadata, MediaVersion>>> GetMovieMetadataAndStatistics(
        PlexLibrary library,
        string key,
        PlexConnection connection,
        PlexServerAuthToken token)
    {
        try
        {
            IPlexServerApi service = XmlServiceFor(connection.Uri);
            Option<PlexXmlVideoMetadataResponseContainer> maybeResponse = await service
                .GetVideoMetadata(key, token.AuthToken)
                .Map(Optional)
                .Map(r => r.Filter(m => m.Metadata.Media.Count > 0 && m.Metadata.Media[0].Part.Count > 0));
            return maybeResponse.Match(
                response =>
                {
                    Option<MediaVersion> maybeVersion = ProjectToMediaVersion(response.Metadata);
                    return maybeVersion.Match<Either<BaseError, Tuple<MovieMetadata, MediaVersion>>>(
                        version => Tuple(ProjectToMovieMetadata(response.Metadata, library.MediaSourceId), version),
                        () => BaseError.New("Unable to locate metadata"));
                },
                () => BaseError.New("Unable to locate metadata"));
        }
        catch (Exception ex)
        {
            return BaseError.New(ex.ToString());
        }
    }

    public async Task<Either<BaseError, Tuple<EpisodeMetadata, MediaVersion>>> GetEpisodeMetadataAndStatistics(
        PlexLibrary library,
        string key,
        PlexConnection connection,
        PlexServerAuthToken token)
    {
        try
        {
            IPlexServerApi service = XmlServiceFor(connection.Uri);
            Option<PlexXmlVideoMetadataResponseContainer> maybeResponse = await service
                .GetVideoMetadata(key, token.AuthToken)
                .Map(Optional)
                .Map(r => r.Filter(m => m.Metadata.Media.Count > 0 && m.Metadata.Media[0].Part.Count > 0));
            return maybeResponse.Match(
                response =>
                {
                    Option<MediaVersion> maybeVersion = ProjectToMediaVersion(response.Metadata);
                    return maybeVersion.Match<Either<BaseError, Tuple<EpisodeMetadata, MediaVersion>>>(
                        version => Tuple(
                            ProjectToEpisodeMetadata(response.Metadata, library.MediaSourceId),
                            version),
                        () => BaseError.New("Unable to locate metadata"));
                },
                () => BaseError.New("Unable to locate metadata"));
        }
        catch (Exception ex)
        {
            return BaseError.New(ex.ToString());
        }
    }

    public async Task<Either<BaseError, int>> GetLibraryItemCount(
        PlexLibrary library,
        PlexConnection connection,
        PlexServerAuthToken token)
    {
        try
        {
            IPlexServerApi service = XmlServiceFor(connection.Uri);
            return await service.GetLibrarySection(library.Key, token.AuthToken).Map(r => r.TotalSize);
        }
        catch (Exception ex)
        {
            return BaseError.New(ex.ToString());
        }
    }

    private async IAsyncEnumerable<TItem> GetPagedLibraryContents<TItem>(
        PlexConnection connection,
        Func<IPlexServerApi, Task<PlexXmlMediaContainerStatsResponse>> countItems,
        Func<IPlexServerApi, IPlexServerApi, int, int, Task<IEnumerable<TItem>>> getItems)
    {
        IPlexServerApi xmlService = XmlServiceFor(connection.Uri);

        int size = await countItems(xmlService).Map(r => r.TotalSize);

        const int PAGE_SIZE = 10;

        IPlexServerApi jsonService = RestService.For<IPlexServerApi>(connection.Uri);
        int pages = (size - 1) / PAGE_SIZE + 1;

        for (var i = 0; i < pages; i++)
        {
            int skip = i * PAGE_SIZE;

            Task<IEnumerable<TItem>> result = getItems(xmlService, jsonService, skip, PAGE_SIZE);

            foreach (TItem item in await result)
            {
                yield return item;
            }
        }
    }

    // TODO: fix this with the addition of paging
    private List<PlexEpisode> ProcessMultiEpisodeFiles(IEnumerable<PlexEpisode> episodes)
    {
        // add all metadata from duplicate paths to first entry with given path
        // i.e. s1e1 episode will add s1e2 metadata if s1e1 and s1e2 have same physical path
        var result = new Dictionary<string, PlexEpisode>();
        foreach (PlexEpisode episode in episodes.OrderBy(e => e.EpisodeMetadata.Head().EpisodeNumber))
        {
            string path = episode.MediaVersions.Head().MediaFiles.Head().Path;
            if (result.TryGetValue(path, out PlexEpisode existing))
            {
                existing.EpisodeMetadata.Add(episode.EpisodeMetadata.Head());
            }
            else
            {
                result.Add(path, episode);
            }
        }

        return result.Values.ToList();
    }

    private static IPlexServerApi XmlServiceFor(string uri, TimeSpan? timeout = null)
    {
        var overrides = new XmlAttributeOverrides();
        var attrs = new XmlAttributes { XmlIgnore = true };
        overrides.Add(typeof(PlexMetadataResponse), "Media", attrs);

        TimeSpan httpClientTimeout = timeout ?? TimeSpan.FromSeconds(30);

        return RestService.For<IPlexServerApi>(
            new HttpClient
            {
                BaseAddress = new Uri(uri),
                Timeout = httpClientTimeout
            },
            new RefitSettings
            {
                ContentSerializer = new XmlContentSerializer(
                    new XmlContentSerializerSettings
                    {
                        XmlAttributeOverrides = overrides
                    })
            });
    }

    private static Option<PlexLibrary> Project(PlexLibraryResponse response) =>
        response.Type switch
        {
            "show" => new PlexLibrary
            {
                Key = response.Key,
                Name = response.Title,
                MediaKind = LibraryMediaKind.Shows,
                ShouldSyncItems = false,
                Paths = new List<LibraryPath> { new() { Path = $"plex://{response.Uuid}" } }
            },
            "movie" => new PlexLibrary
            {
                Key = response.Key,
                Name = response.Title,
                MediaKind = LibraryMediaKind.Movies,
                ShouldSyncItems = false,
                Paths = new List<LibraryPath> { new() { Path = $"plex://{response.Uuid}" } }
            },
            // TODO: "artist" for music libraries
            _ => None
        };

    private PlexMovie ProjectToMovie(PlexMetadataResponse response, int mediaSourceId)
    {
        PlexMediaResponse<PlexPartResponse> media = response.Media.Head();
        PlexPartResponse part = media.Part.Head();
        DateTime dateAdded = DateTimeOffset.FromUnixTimeSeconds(response.AddedAt).DateTime;
        DateTime lastWriteTime = DateTimeOffset.FromUnixTimeSeconds(response.UpdatedAt).DateTime;

        MovieMetadata metadata = ProjectToMovieMetadata(response, mediaSourceId);

        var version = new MediaVersion
        {
            Name = "Main",
            Duration = TimeSpan.FromMilliseconds(media.Duration),
            Width = media.Width,
            Height = media.Height,
            // specifically omit sample aspect ratio
            DateAdded = dateAdded,
            DateUpdated = lastWriteTime,
            MediaFiles = new List<MediaFile>
            {
                new PlexMediaFile
                {
                    PlexId = part.Id,
                    Key = part.Key,
                    Path = part.File
                }
            },
            Streams = new List<MediaStream>()
        };

        var movie = new PlexMovie
        {
            Etag = _plexEtag.ForMovie(response),
            Key = response.Key,
            MovieMetadata = new List<MovieMetadata> { metadata },
            MediaVersions = new List<MediaVersion> { version },
            TraktListItems = new List<TraktListItem>()
        };

        return movie;
    }

    private MovieMetadata ProjectToMovieMetadata(PlexMetadataResponse response, int mediaSourceId)
    {
        DateTime dateAdded = DateTimeOffset.FromUnixTimeSeconds(response.AddedAt).DateTime;
        DateTime lastWriteTime = DateTimeOffset.FromUnixTimeSeconds(response.UpdatedAt).DateTime;

        var metadata = new MovieMetadata
        {
            MetadataKind = MetadataKind.External,
            Title = response.Title,
            SortTitle = SortTitle.GetSortTitle(response.Title),
            Plot = response.Summary,
            Year = response.Year,
            Tagline = response.Tagline,
            ContentRating = response.ContentRating,
            DateAdded = dateAdded,
            DateUpdated = lastWriteTime,
            Genres = Optional(response.Genre).Flatten().Map(g => new Genre { Name = g.Tag }).ToList(),
            Tags = new List<Tag>(),
            Studios = new List<Studio>(),
            Actors = Optional(response.Role).Flatten().Map(r => ProjectToModel(r, dateAdded, lastWriteTime))
                .ToList(),
            Directors = Optional(response.Director).Flatten().Map(d => new Director { Name = d.Tag }).ToList(),
            Writers = Optional(response.Writer).Flatten().Map(w => new Writer { Name = w.Tag }).ToList()
        };

        if (response is PlexXmlMetadataResponse xml)
        {
            metadata.Guids = Optional(xml.Guid).Flatten().Map(g => new MetadataGuid { Guid = g.Id }).ToList();
            if (!string.IsNullOrWhiteSpace(xml.PlexGuid))
            {
                Option<string> normalized = NormalizeGuid(xml.PlexGuid);
                foreach (string guid in normalized)
                {
                    if (metadata.Guids.All(g => g.Guid != guid))
                    {
                        metadata.Guids.Add(new MetadataGuid { Guid = guid });
                    }
                }
            }
        }
        else
        {
            metadata.Guids = new List<MetadataGuid>();
        }

        foreach (PlexCollectionResponse collection in Optional(response.Collection).Flatten())
        {
            metadata.Tags.Add(new Tag { Name = collection.Tag, ExternalCollectionId = collection.Id.ToString() });
        }

        foreach (PlexLabelResponse label in Optional(response.Label).Flatten())
        {
            metadata.Tags.Add(new Tag { Name = label.Tag, ExternalCollectionId = label.Id.ToString() });
        }

        if (!string.IsNullOrWhiteSpace(response.Studio))
        {
            metadata.Studios.Add(new Studio { Name = response.Studio });
        }

        if (DateTime.TryParse(response.OriginallyAvailableAt, out DateTime releaseDate))
        {
            metadata.ReleaseDate = releaseDate;
        }

        if (!string.IsNullOrWhiteSpace(response.Thumb))
        {
            string path = $"plex/{mediaSourceId}{response.Thumb}";
            var artwork = new Artwork
            {
                ArtworkKind = ArtworkKind.Poster,
                Path = path,
                DateAdded = dateAdded,
                DateUpdated = lastWriteTime
            };

            metadata.Artwork ??= new List<Artwork>();
            metadata.Artwork.Add(artwork);
        }

        if (!string.IsNullOrWhiteSpace(response.Art))
        {
            string path = $"plex/{mediaSourceId}{response.Art}";
            var artwork = new Artwork
            {
                ArtworkKind = ArtworkKind.FanArt,
                Path = path,
                DateAdded = dateAdded,
                DateUpdated = lastWriteTime
            };

            metadata.Artwork ??= new List<Artwork>();
            metadata.Artwork.Add(artwork);
        }

        return metadata;
    }

    private Option<MediaVersion> ProjectToMediaVersion(PlexXmlMetadataResponse response)
    {
        PlexMediaResponse<PlexXmlPartResponse> media = response.Media.Head();
        List<PlexStreamResponse> streams = media.Part.Head().Stream;
        DateTime dateUpdated = DateTimeOffset.FromUnixTimeSeconds(response.UpdatedAt).DateTime;
        Option<PlexStreamResponse> maybeVideoStream = streams.Find(s => s.StreamType == 1);
        return maybeVideoStream.Map(
            videoStream =>
            {
                var version = new MediaVersion
                {
                    Duration = TimeSpan.FromMilliseconds(media.Duration),
                    SampleAspectRatio = string.IsNullOrWhiteSpace(videoStream.PixelAspectRatio) ? "1:1"
                        : videoStream.PixelAspectRatio,
                    VideoScanKind = videoStream.ScanType switch
                    {
                        "interlaced" => VideoScanKind.Interlaced,
                        "progressive" => VideoScanKind.Progressive,
                        _ => VideoScanKind.Unknown
                    },
                    Streams = new List<MediaStream>(),
                    DateUpdated = dateUpdated,
                    Width = videoStream.Width,
                    Height = videoStream.Height,
                    RFrameRate = videoStream.FrameRate,
                    DisplayAspectRatio = media.AspectRatio == 0
                        ? string.Empty
                        : media.AspectRatio.ToString("0.00###", CultureInfo.InvariantCulture),
                    Chapters = Optional(response.Chapters).Flatten().Map(ProjectToModel).ToList()
                };

                version.Streams.Add(
                    new MediaStream
                    {
                        MediaVersionId = version.Id,
                        MediaStreamKind = MediaStreamKind.Video,
                        Index = videoStream.Index,
                        Codec = videoStream.Codec,
                        Profile = (videoStream.Profile ?? string.Empty).ToLowerInvariant(),
                        Default = videoStream.Default,
                        Language = videoStream.LanguageCode,
                        Forced = videoStream.Forced,
                        BitsPerRawSample = videoStream.BitDepth,
                        ColorRange = (videoStream.ColorRange ?? string.Empty).ToLowerInvariant(),
                        ColorSpace = (videoStream.ColorSpace ?? string.Empty).ToLowerInvariant(),
                        ColorTransfer = (videoStream.ColorTrc ?? string.Empty).ToLowerInvariant(),
                        ColorPrimaries = (videoStream.ColorPrimaries ?? string.Empty).ToLowerInvariant()
                    });
                
                foreach (PlexStreamResponse audioStream in streams.Filter(s => s.StreamType == 2))
                {
                    var stream = new MediaStream
                    {
                        MediaVersionId = version.Id,
                        MediaStreamKind = MediaStreamKind.Audio,
                        Index = audioStream.Index,
                        Codec = audioStream.Codec,
                        Profile = (audioStream.Profile ?? string.Empty).ToLowerInvariant(),
                        Channels = audioStream.Channels,
                        Default = audioStream.Default,
                        Forced = audioStream.Forced,
                        Language = audioStream.LanguageCode,
                        Title = audioStream.Title ?? string.Empty
                    };
                    
                    version.Streams.Add(stream);
                }

                foreach (PlexStreamResponse subtitleStream in streams.Filter(s => s.StreamType == 3))
                {
                    var stream = new MediaStream
                    {
                        MediaVersionId = version.Id,
                        MediaStreamKind = MediaStreamKind.Subtitle,
                        Index = subtitleStream.Index,
                        Codec = subtitleStream.Codec,
                        Default = subtitleStream.Default,
                        Forced = subtitleStream.Forced,
                        Language = subtitleStream.LanguageCode
                    };

                    version.Streams.Add(stream);
                }

                return version;
            });
    }

    private PlexShow ProjectToShow(PlexMetadataResponse response, int mediaSourceId)
    {
        ShowMetadata metadata = ProjectToShowMetadata(response, mediaSourceId);

        var show = new PlexShow
        {
            Key = response.Key,
            Etag = _plexEtag.ForShow(response),
            ShowMetadata = new List<ShowMetadata> { metadata },
            TraktListItems = new List<TraktListItem>()
        };

        return show;
    }

    private ShowMetadata ProjectToShowMetadata(PlexMetadataResponse response, int mediaSourceId)
    {
        DateTime dateAdded = DateTimeOffset.FromUnixTimeSeconds(response.AddedAt).DateTime;
        DateTime lastWriteTime = DateTimeOffset.FromUnixTimeSeconds(response.UpdatedAt).DateTime;

        var metadata = new ShowMetadata
        {
            MetadataKind = MetadataKind.External,
            Title = response.Title,
            SortTitle = SortTitle.GetSortTitle(response.Title),
            Plot = response.Summary,
            Year = response.Year,
            Tagline = response.Tagline,
            ContentRating = response.ContentRating,
            DateAdded = dateAdded,
            DateUpdated = lastWriteTime,
            Genres = Optional(response.Genre).Flatten().Map(g => new Genre { Name = g.Tag }).ToList(),
            Tags = new List<Tag>(),
            Studios = new List<Studio>(),
            Actors = Optional(response.Role).Flatten().Map(r => ProjectToModel(r, dateAdded, lastWriteTime))
                .ToList()
        };

        if (response is PlexXmlMetadataResponse xml)
        {
            metadata.Guids = Optional(xml.Guid).Flatten().Map(g => new MetadataGuid { Guid = g.Id }).ToList();
            if (!string.IsNullOrWhiteSpace(xml.PlexGuid))
            {
                Option<string> normalized = NormalizeGuid(xml.PlexGuid);
                foreach (string guid in normalized)
                {
                    if (metadata.Guids.All(g => g.Guid != guid))
                    {
                        metadata.Guids.Add(new MetadataGuid { Guid = guid });
                    }
                }
            }
        }
        else
        {
            metadata.Guids = new List<MetadataGuid>();
        }

        if (!string.IsNullOrWhiteSpace(response.Studio))
        {
            metadata.Studios.Add(new Studio { Name = response.Studio });
        }

        foreach (PlexCollectionResponse collection in Optional(response.Collection).Flatten())
        {
            metadata.Tags.Add(new Tag { Name = collection.Tag, ExternalCollectionId = collection.Id.ToString() });
        }

        foreach (PlexLabelResponse label in Optional(response.Label).Flatten())
        {
            metadata.Tags.Add(new Tag { Name = label.Tag, ExternalCollectionId = label.Id.ToString() });
        }

        if (DateTime.TryParse(response.OriginallyAvailableAt, out DateTime releaseDate))
        {
            metadata.ReleaseDate = releaseDate;
        }

        if (!string.IsNullOrWhiteSpace(response.Thumb))
        {
            string path = $"plex/{mediaSourceId}{response.Thumb}";
            var artwork = new Artwork
            {
                ArtworkKind = ArtworkKind.Poster,
                Path = path,
                DateAdded = dateAdded,
                DateUpdated = lastWriteTime
            };

            metadata.Artwork ??= new List<Artwork>();
            metadata.Artwork.Add(artwork);
        }

        if (!string.IsNullOrWhiteSpace(response.Art))
        {
            string path = $"plex/{mediaSourceId}{response.Art}";
            var artwork = new Artwork
            {
                ArtworkKind = ArtworkKind.FanArt,
                Path = path,
                DateAdded = dateAdded,
                DateUpdated = lastWriteTime
            };

            metadata.Artwork ??= new List<Artwork>();
            metadata.Artwork.Add(artwork);
        }

        return metadata;
    }

    private PlexSeason ProjectToSeason(PlexXmlMetadataResponse response, int mediaSourceId)
    {
        DateTime dateAdded = DateTimeOffset.FromUnixTimeSeconds(response.AddedAt).DateTime;
        DateTime lastWriteTime = DateTimeOffset.FromUnixTimeSeconds(response.UpdatedAt).DateTime;

        var metadata = new SeasonMetadata
        {
            MetadataKind = MetadataKind.External,
            Title = response.Title,
            SortTitle = SortTitle.GetSortTitle(response.Title),
            Year = response.Year,
            DateAdded = dateAdded,
            DateUpdated = lastWriteTime,
            Tags = new List<Tag>()
        };

        metadata.Guids = Optional(response.Guid).Flatten().Map(g => new MetadataGuid { Guid = g.Id }).ToList();
        if (!string.IsNullOrWhiteSpace(response.PlexGuid))
        {
            Option<string> normalized = NormalizeGuid(response.PlexGuid);
            foreach (string guid in normalized)
            {
                if (metadata.Guids.All(g => g.Guid != guid))
                {
                    metadata.Guids.Add(new MetadataGuid { Guid = guid });
                }
            }
        }

        foreach (PlexCollectionResponse collection in Optional(response.Collection).Flatten())
        {
            metadata.Tags.Add(new Tag { Name = collection.Tag, ExternalCollectionId = collection.Id.ToString() });
        }

        if (!string.IsNullOrWhiteSpace(response.Thumb))
        {
            string path = $"plex/{mediaSourceId}{response.Thumb}";
            var artwork = new Artwork
            {
                ArtworkKind = ArtworkKind.Poster,
                Path = path,
                DateAdded = dateAdded,
                DateUpdated = lastWriteTime
            };

            metadata.Artwork ??= new List<Artwork>();
            metadata.Artwork.Add(artwork);
        }

        if (!string.IsNullOrWhiteSpace(response.Art))
        {
            string path = $"plex/{mediaSourceId}{response.Art}";
            var artwork = new Artwork
            {
                ArtworkKind = ArtworkKind.FanArt,
                Path = path,
                DateAdded = dateAdded,
                DateUpdated = lastWriteTime
            };

            metadata.Artwork ??= new List<Artwork>();
            metadata.Artwork.Add(artwork);
        }

        var season = new PlexSeason
        {
            Key = response.Key,
            Etag = _plexEtag.ForSeason(response),
            SeasonNumber = response.Index,
            SeasonMetadata = new List<SeasonMetadata> { metadata },
            TraktListItems = new List<TraktListItem>()
        };

        return season;
    }

    private PlexEpisode ProjectToEpisode(PlexXmlMetadataResponse response, int mediaSourceId)
    {
        PlexMediaResponse<PlexXmlPartResponse> media = response.Media.Head();
        PlexXmlPartResponse part = media.Part.Head();
        DateTime dateAdded = DateTimeOffset.FromUnixTimeSeconds(response.AddedAt).DateTime;
        DateTime lastWriteTime = DateTimeOffset.FromUnixTimeSeconds(response.UpdatedAt).DateTime;

        EpisodeMetadata metadata = ProjectToEpisodeMetadata(response, mediaSourceId);
        var version = new MediaVersion
        {
            Name = "Main",
            Duration = TimeSpan.FromMilliseconds(media.Duration),
            Width = media.Width,
            Height = media.Height,
            DateAdded = dateAdded,
            DateUpdated = lastWriteTime,
            MediaFiles = new List<MediaFile>
            {
                new PlexMediaFile
                {
                    PlexId = part.Id,
                    Key = part.Key,
                    Path = part.File
                }
            },
            // specifically omit stream details
            Streams = new List<MediaStream>()
        };

        var episode = new PlexEpisode
        {
            Key = response.Key,
            Etag = _plexEtag.ForEpisode(response),
            EpisodeMetadata = new List<EpisodeMetadata> { metadata },
            MediaVersions = new List<MediaVersion> { version },
            TraktListItems = new List<TraktListItem>()
        };

        return episode;
    }

    private EpisodeMetadata ProjectToEpisodeMetadata(PlexMetadataResponse response, int mediaSourceId)
    {
        DateTime dateAdded = DateTimeOffset.FromUnixTimeSeconds(response.AddedAt).DateTime;
        DateTime lastWriteTime = DateTimeOffset.FromUnixTimeSeconds(response.UpdatedAt).DateTime;

        var metadata = new EpisodeMetadata
        {
            MetadataKind = MetadataKind.External,
            Title = response.Title,
            SortTitle = SortTitle.GetSortTitle(response.Title),
            EpisodeNumber = response.Index,
            Plot = response.Summary,
            Year = response.Year,
            Tagline = response.Tagline,
            DateAdded = dateAdded,
            DateUpdated = lastWriteTime,
            Actors = Optional(response.Role).Flatten().Map(r => ProjectToModel(r, dateAdded, lastWriteTime))
                .ToList(),
            Directors = Optional(response.Director).Flatten().Map(d => new Director { Name = d.Tag }).ToList(),
            Writers = Optional(response.Writer).Flatten().Map(w => new Writer { Name = w.Tag }).ToList(),
            Tags = new List<Tag>()
        };

        if (response is PlexXmlMetadataResponse xml)
        {
            metadata.Guids = Optional(xml.Guid).Flatten().Map(g => new MetadataGuid { Guid = g.Id }).ToList();
            if (!string.IsNullOrWhiteSpace(xml.PlexGuid))
            {
                Option<string> normalized = NormalizeGuid(xml.PlexGuid);
                foreach (string guid in normalized)
                {
                    if (metadata.Guids.All(g => g.Guid != guid))
                    {
                        metadata.Guids.Add(new MetadataGuid { Guid = guid });
                    }
                }
            }
        }
        else
        {
            metadata.Guids = new List<MetadataGuid>();
        }

        if (DateTime.TryParse(response.OriginallyAvailableAt, out DateTime releaseDate))
        {
            metadata.ReleaseDate = releaseDate;
        }

        foreach (PlexCollectionResponse collection in Optional(response.Collection).Flatten())
        {
            metadata.Tags.Add(new Tag { Name = collection.Tag, ExternalCollectionId = collection.Id.ToString() });
        }

        if (!string.IsNullOrWhiteSpace(response.Thumb))
        {
            string path = $"plex/{mediaSourceId}{response.Thumb}";
            var artwork = new Artwork
            {
                ArtworkKind = ArtworkKind.Thumbnail,
                Path = path,
                DateAdded = dateAdded,
                DateUpdated = lastWriteTime
            };

            metadata.Artwork ??= new List<Artwork>();
            metadata.Artwork.Add(artwork);
        }

        return metadata;
    }

    private Actor ProjectToModel(PlexRoleResponse role, DateTime dateAdded, DateTime lastWriteTime)
    {
        var actor = new Actor { Name = role.Tag, Role = role.Role };
        if (!string.IsNullOrWhiteSpace(role.Thumb))
        {
            actor.Artwork = new Artwork
            {
                Path = role.Thumb,
                ArtworkKind = ArtworkKind.Thumbnail,
                DateAdded = dateAdded,
                DateUpdated = lastWriteTime
            };
        }

        return actor;
    }

    private static MediaChapter ProjectToModel(PlexChapterResponse chapter) =>
        new()
        {
            ChapterId = chapter.Index,
            StartTime = TimeSpan.FromMilliseconds(chapter.StartTimeOffset),
            EndTime = TimeSpan.FromMilliseconds(chapter.EndTimeOffset)
        };

    private Option<string> NormalizeGuid(string guid)
    {
        if (guid.StartsWith("plex://show") ||
            guid.StartsWith("plex://season") ||
            guid.StartsWith("plex://episode") ||
            guid.StartsWith("plex://movie"))
        {
            return guid;
        }

        if (guid.StartsWith("com.plexapp.agents.imdb"))
        {
            string strip1 = guid.Replace("com.plexapp.agents.imdb://", string.Empty);
            string strip2 = strip1.Split("?").Head();
            return $"imdb://{strip2}";
        }

        if (guid.StartsWith("com.plexapp.agents.thetvdb"))
        {
            string strip1 = guid.Replace("com.plexapp.agents.thetvdb://", string.Empty);
            string strip2 = strip1.Split("?").Head();
            return $"tvdb://{strip2}";
        }

        if (guid.StartsWith("com.plexapp.agents.themoviedb"))
        {
            string strip1 = guid.Replace("com.plexapp.agents.themoviedb://", string.Empty);
            string strip2 = strip1.Split("?").Head();
            return $"tmdb://{strip2}";
        }

        if (guid.StartsWith("local://"))
        {
            _logger.LogDebug("Ignoring local Plex guid: {Guid}", guid);
        }
        else
        {
            _logger.LogWarning("Unsupported guid format from Plex; ignoring: {Guid}", guid);
        }

        return None;
    }
}
