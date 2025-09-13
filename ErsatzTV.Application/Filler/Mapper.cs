using System.Globalization;
using ErsatzTV.Application.Movies;
using ErsatzTV.Core.Domain;
using ErsatzTV.Core.Domain.Filler;
using ErsatzTV.Core.Extensions;

namespace ErsatzTV.Application.Filler;

internal static class Mapper
{
    internal static FillerPresetViewModel ProjectToViewModel(FillerPreset fillerPreset) =>
        new(
            fillerPreset.Id,
            fillerPreset.Name,
            fillerPreset.FillerKind,
            fillerPreset.FillerMode,
            fillerPreset.Duration,
            fillerPreset.Count,
            fillerPreset.PadToNearestMinute,
            fillerPreset.AllowWatermarks,
            fillerPreset.CollectionType,
            fillerPreset.CollectionId,
            fillerPreset.MediaItemId,
            fillerPreset.MultiCollectionId,
            fillerPreset.SmartCollectionId);

    internal static FillerViewModel ProjectToViewModel(
        FillerMediaItem filler,
        string localPath,
        List<string> languageCodes)
    {
        FillerMetadata metadata = Optional(filler.FillerMetadata).Flatten().Head();
        return new FillerViewModel(
            metadata.Title,
            metadata.Year?.ToString(CultureInfo.InvariantCulture),
            metadata.Tags,
            (metadata.ContentRating ?? string.Empty).Split("/").Map(s => s.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
            metadata.MovieId?.ToString(CultureInfo.InvariantCulture),
            filler.GetHeadVersion().MediaFiles.Head().Path,
            localPath,
            filler.State)
        {
            Poster = "",
            FanArt = ""
        };
    }

    private static List<string> LanguagesForFiller(List<string> languageCodes)
    {
        CultureInfo[] allCultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);

        return languageCodes
            .Map(
                lang => allCultures.Filter(
                    ci => string.Equals(ci.ThreeLetterISOLanguageName, lang, StringComparison.OrdinalIgnoreCase)))
            .Flatten()
            .Map(ci => ci.EnglishName)
            .Distinct()
            .ToList();
    }
}
