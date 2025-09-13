using ErsatzTV.Core;

namespace ErsatzTV.Application.MediaCollections;

public record AddFillerToPlaylist(int PlaylistId, int FillerId) : IRequest<Either<BaseError, Unit>>;
