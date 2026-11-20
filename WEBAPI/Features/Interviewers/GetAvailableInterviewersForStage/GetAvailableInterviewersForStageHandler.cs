using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetAvailableInterviewersForStage;

public sealed class GetAvailableInterviewersForStageHandler(RmsDbContext db)
    : IRequestHandler<GetAvailableInterviewersForStageQuery, Result<IReadOnlyCollection<AssignedInterviewerDto>>>
{
    public async Task<Result<IReadOnlyCollection<AssignedInterviewerDto>>> Handle(
        GetAvailableInterviewersForStageQuery request, CancellationToken ct)
    {
        var appStage = await db.ApplicationStages
            .AsNoTracking()
            .Include(a => a.BatchStage)
                .ThenInclude(bs => bs.StageType)
            .Include(a => a.BatchStage)
                .ThenInclude(bs => bs.StageEmployees)
                    .ThenInclude(se => se.Employee)
                        .ThenInclude(e => e.Department)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationStageId, ct);

        if (appStage is null)
        {
            return Result<IReadOnlyCollection<AssignedInterviewerDto>>.Failure(
                Error.NotFound($"ApplicationStage with ID '{request.ApplicationStageId}' was not found."));
        }

        if (appStage.BatchStage.StageType.StageCategory != StageCategory.Interview)
        {
            return Result<IReadOnlyCollection<AssignedInterviewerDto>>.Failure(
                Error.BusinessRule($"Stage '{appStage.BatchStage.StageName}' is not an interview stage."));
        }

        var availableInterviewers = appStage.BatchStage.StageEmployees
            .Where(se => se.IsActive && se.Status == StageEmployeeStatus.Accepted)
            .OrderBy(se => se.Employee.FullName)
            .ToAssignedInterviewerDtoList();

        return Result<IReadOnlyCollection<AssignedInterviewerDto>>.Success(availableInterviewers);
    }
}

