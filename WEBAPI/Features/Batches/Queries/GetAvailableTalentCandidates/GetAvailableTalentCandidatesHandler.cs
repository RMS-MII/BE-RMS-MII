using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Batches.Queries.GetAvailableTalentCandidates;

public sealed class GetAvailableTalentCandidatesHandler(RmsDbContext db)
    : IRequestHandler<GetAvailableTalentCandidatesQuery, Result<AvailableTalentCandidatesPageDto>>
{
    public async Task<Result<AvailableTalentCandidatesPageDto>> Handle(
        GetAvailableTalentCandidatesQuery request, CancellationToken ct)
    {
        if (request.Page < 1 || request.PageSize is < 1 or > 100
            || (long)(request.Page - 1) * request.PageSize > int.MaxValue)
            return Result<AvailableTalentCandidatesPageDto>.Failure(
                Error.Validation("Page must be positive and page size must be between 1 and 100."));

        if (!await db.RecruitmentBatches.AsNoTracking()
                .AnyAsync(batch => batch.Id == request.BatchId, ct))
            return Result<AvailableTalentCandidatesPageDto>.Failure(
                Error.NotFound("Batch was not found."));

        var query = db.Candidates.AsNoTracking()
            .Where(candidate => candidate.IsInTalentPool
                && !candidate.BatchApplications.Any(application =>
                    application.RecruitmentBatchId == request.BatchId));

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(candidate =>
                candidate.FullName.Contains(search)
                || candidate.Email.Contains(search)
                || candidate.Institution.Contains(search)
                || candidate.PhoneNumber.Contains(search));

        var candidates = await query
            .OrderBy(candidate => candidate.FullName)
            .ThenBy(candidate => candidate.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Select(candidate => new AvailableTalentCandidateDto
            {
                Id = candidate.Id,
                FullName = candidate.FullName,
                Gender = candidate.Gender,
                Email = candidate.Email,
                PhoneNumber = candidate.PhoneNumber,
                BirthDate = candidate.BirthDate,
                Cv = candidate.Cv,
                Portfolio = candidate.Portfolio,
                Education = candidate.Education,
                Institution = candidate.Institution,
                Address = candidate.Address,
                ExpectedSalary = candidate.ExpectedSalary,
                Gpa = candidate.Gpa
            })
            .Take(request.PageSize + 1)
            .ToListAsync(ct);

        return Result<AvailableTalentCandidatesPageDto>.Success(new AvailableTalentCandidatesPageDto
        {
            Items = candidates.Take(request.PageSize).ToList(),
            HasMore = candidates.Count > request.PageSize
        });
    }
}
