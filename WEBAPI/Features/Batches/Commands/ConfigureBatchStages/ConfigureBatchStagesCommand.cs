using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Commands.ConfigureBatchStages;

public sealed record ConfigureBatchStagesCommand : IRequest<Result<IReadOnlyCollection<BatchStageDto>>>
{
    public Guid BatchId { get; init; }
    public IReadOnlyList<ConfigureBatchStageItem> Stages { get; init; } = [];
}

public sealed record ConfigureBatchStageItem
{
    public Guid StageTypeId { get; init; }
    public string StageName { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public decimal? PassingScore { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
