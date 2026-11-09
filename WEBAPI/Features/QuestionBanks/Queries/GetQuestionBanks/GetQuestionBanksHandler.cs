using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.QuestionBanks.Queries.GetQuestionBanks;

public sealed class GetQuestionBanksHandler(RmsDbContext db)
    : IRequestHandler<GetQuestionBanksQuery, Result<PagedResult<QuestionBankDto>>>
{
    public async Task<Result<PagedResult<QuestionBankDto>>> Handle(
        GetQuestionBanksQuery request, CancellationToken ct)
    {
        var query = db.QuestionBanks
            .AsNoTracking()
            .AsQueryable();

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(questionBank =>
                questionBank.DepartmentId == request.DepartmentId.Value);
        }

        if (request.StageTypeId.HasValue)
        {
            query = query.Where(questionBank =>
                questionBank.StageTypeId == request.StageTypeId.Value);
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(questionBank =>
                questionBank.Department.Name.Contains(search) ||
                questionBank.StageType.Name.Contains(search));
        }

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .OrderBy(questionBank => questionBank.Department.Name)
            .ThenBy(questionBank => questionBank.StageType.Name)
            .ThenBy(questionBank => questionBank.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(questionBank => new QuestionBankDto
            {
                Id = questionBank.Id,
                DepartmentId = questionBank.DepartmentId,
                DepartmentName = questionBank.Department.Name,
                StageTypeId = questionBank.StageTypeId,
                StageTypeName = questionBank.StageType.Name,
                CreatedBy = questionBank.CreatedBy,
                DurationInMinutes = questionBank.DurationInMinutes,
                QuestionCount = questionBank.Questions.Count,
                ActiveQuestionCount = questionBank.Questions.Count(question => question.IsActive),
                CreatedAt = questionBank.CreatedAt,
                UpdatedAt = questionBank.UpdatedAt
            })
            .ToListAsync(ct);

        var result = new PagedResult<QuestionBankDto>(
            items,
            request.Page,
            request.PageSize,
            totalItems);

        return Result<PagedResult<QuestionBankDto>>.Success(result);
    }
}
