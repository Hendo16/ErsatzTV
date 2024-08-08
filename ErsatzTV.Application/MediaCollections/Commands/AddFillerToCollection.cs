using ErsatzTV.Core;

namespace ErsatzTV.Application.MediaCollections;

public record AddFillerToCollection(int CollectionId, int FillerId) : IRequest<Either<BaseError, Unit>>;
