using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Applications.Commands.UploadOfferingLetter;

public sealed record UploadOfferingLetterCommand : IRequest<Result<BatchApplicationDto>>
{
    public Guid ApplicationId { get; init; }
    public string FileOfferingLetter { get; init; } = string.Empty;
}
