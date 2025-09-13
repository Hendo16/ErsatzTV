namespace ErsatzTV.Application.Filler;


public record GetFillerById(int Id) : IRequest<Option<FillerViewModel>>;
