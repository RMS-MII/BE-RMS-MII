using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Features.StageTypes.Commands.CreateStageType;

public sealed class CreateStageTypeHandler(RmsDbContext db, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreateStageTypeCommand, Result<StageTypeDto>>
{
    public async Task<Result<StageTypeDto>> Handle(CreateStageTypeCommand request, CancellationToken ct)
    {
        var trimmedName = request.Name.Trim();

        var exists = await db.StageTypes
            .AnyAsync(st => st.Name.ToLower() == trimmedName.ToLower(), ct);

        if (exists)
        {
            return Result<StageTypeDto>.Failure(
                Error.Conflict($"Tipe tahapan dengan nama '{trimmedName}' sudah terdaftar."));
        }

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        var stageType = new StageType
        {
            Id = Guid.CreateVersion7(),
            Name = trimmedName,
            Description = request.Description?.Trim(),
            StageCategory = request.StageCategory,
            IsActive = request.IsActive,
            CreatedAt = now
        };

        db.StageTypes.Add(stageType);
        await db.SaveChangesAsync(ct);

        return Result<StageTypeDto>.Success(stageType.ToDto());
    }
}

