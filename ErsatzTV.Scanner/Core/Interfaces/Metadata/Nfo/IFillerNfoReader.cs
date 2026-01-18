using ErsatzTV.Core;
using ErsatzTV.Scanner.Core.Metadata.Nfo;

namespace ErsatzTV.Scanner.Core.Interfaces.Metadata.Nfo;

public interface IFillerNfoReader
{
    Task<Either<BaseError, FillerNfo>> ReadFromFile(string fileName);
}
