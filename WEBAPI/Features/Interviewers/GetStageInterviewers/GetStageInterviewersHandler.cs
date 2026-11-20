using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.Interviewers.GetStageInterviewers;

public sealed class GetStageInterviewersHandler(RmsDbContext db)
    : IRequestHandler<GetStageInterviewersQuery, Result<AssignInterviewersResponse>>
{
    public async Task<Result<AssignInterviewersResponse>> Handle(
        GetStageInterviewersQuery request, CancellationToken ct)
    {
        var stage = await db.BatchStages
            .AsNoTracking()
            .Include(bs => bs.RecruitmentBatch)
            .Include(bs => bs.StageType)
            .Include(bs => bs.StageEmployees)
                .ThenInclude(se => se.Employee)
                    .ThenInclude(e => e.Department)
            .FirstOrDefaultAsync(bs => bs.Id == request.StageId && bs.RecruitmentBatchId == request.BatchId, ct);

        if (stage is null)
        {
            return Result<AssignInterviewersResponse>.Failure(
                Error.NotFound($"Stage with ID '{request.StageId}' in batch '{request.BatchId}' was not found."));
        }

        var stageEmployees = stage.StageEmployees.AsEnumerable();

        if (request.Status.HasValue)
        {
            stageEmployees = stageEmployees.Where(se => se.Status == request.Status.Value);
        }

        var interviewerDtos = stageEmployees
            .OrderBy(se => se.Employee.FullName)
            .ToAssignedInterviewerDtoList();

        return Result<AssignInterviewersResponse>.Success(stage.ToAssignInterviewersResponse(interviewerDtos));
    }
}

