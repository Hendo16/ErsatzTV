using ErsatzTV.Core.Domain;
using ErsatzTV.Core.Domain.Filler;

namespace ErsatzTV.Core.Interfaces.Metadata;

public interface IFallbackMetadataProvider
{
    Option<int> GetSeasonNumberForFolder(string folder);
    ShowMetadata GetFallbackMetadataForShow(string showFolder);
    ArtistMetadata GetFallbackMetadataForArtist(string artistFolder);
    List<EpisodeMetadata> GetFallbackMetadata(Episode episode);
    MovieMetadata GetFallbackMetadata(Movie movie);
    Option<MusicVideoMetadata> GetFallbackMetadata(MusicVideo musicVideo);
    Option<OtherVideoMetadata> GetFallbackMetadata(OtherVideo otherVideo);
    Option<FillerMetadata> GetFallbackMetadata(FillerMediaItem filler);
    Option<SongMetadata> GetFallbackMetadata(Song song);
    Option<ImageMetadata> GetFallbackMetadata(Image image);
}
