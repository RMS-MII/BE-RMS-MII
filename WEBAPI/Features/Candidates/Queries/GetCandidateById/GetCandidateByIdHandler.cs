using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Candidates.Queries.GetCandidateById;

public sealed class GetCandidateByIdHandler(RmsDbContext db)
    : IRequestHandler<GetCandidateByIdQuery, Result<CandidateWithBatchesDto>>
{
    public async Task<Result<CandidateWithBatchesDto>> Handle(
        GetCandidateByIdQuery request, CancellationToken ct)
    {
        var candidate = await db.Candidates
            .AsNoTracking()
            .Include(c => c.BatchApplications)
                .ThenInclude(ba => ba.RecruitmentBatch)
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.IsInTalentPool, ct);

        if (candidate is null)
        {
            return Result<CandidateWithBatchesDto>.Failure(
                Error.NotFound($"Candidate with ID '{request.Id}' was not found."));
        }

        return Result<CandidateWithBatchesDto>.Success(candidate.ToWithBatchesDto());
    }
}

