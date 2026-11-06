using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class CandidateMapping
{
    public static CandidateDto ToDto(this Candidate candidate) => new()
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
        Gpa = candidate.Gpa,
        CreatedAt = candidate.CreatedAt,
        UpdatedAt = candidate.UpdatedAt
    };

    public static IReadOnlyCollection<CandidateDto> ToDtoList(this IEnumerable<Candidate> candidates)
        => candidates.Select(ToDto).ToList();

    public static CandidateWithBatchesDto ToWithBatchesDto(this Candidate candidate) => new()
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
        Gpa = candidate.Gpa,
        CreatedAt = candidate.CreatedAt,
        UpdatedAt = candidate.UpdatedAt,
        AssignedBatches = candidate.BatchApplications?
            .Select(ba => new CandidateAssignedBatchDto
            {
                ApplicationId = ba.Id,
                BatchId = ba.RecruitmentBatchId,
                BatchCode = ba.RecruitmentBatch?.BatchCode ?? string.Empty,
                BatchName = ba.RecruitmentBatch?.Name ?? string.Empty,
                CodeApplication = ba.CodeApplication,
                ApplicationStatus = ba.Status,
                FinalResultStatus = ba.FinalResultStatus,
                AppliedAt = ba.AppliedAt
            })
            .ToList() ?? []
    };

    public static IReadOnlyCollection<CandidateWithBatchesDto> ToWithBatchesDtoList(this IEnumerable<Candidate> candidates)
        => candidates.Select(ToWithBatchesDto).ToList();

    public static Candidate ToEntity(this CreateCandidateDto dto) => new()
    {
        FullName = dto.FullName.Trim(),
        Gender = dto.Gender,
        Email = dto.Email.Trim(),
        PhoneNumber = dto.PhoneNumber.Trim(),
        BirthDate = dto.BirthDate,
        Cv = dto.Cv?.Trim(),
        Portfolio = dto.Portfolio?.Trim(),
        Education = dto.Education.Trim(),
        Institution = dto.Institution.Trim(),
        Address = dto.Address.Trim(),
        ExpectedSalary = dto.ExpectedSalary,
        Gpa = dto.Gpa
    };

    public static void ApplyTo(this UpdateCandidateDto dto, Candidate candidate)
    {
        candidate.FullName = dto.FullName.Trim();
        candidate.Gender = dto.Gender;
        candidate.Email = dto.Email.Trim();
        candidate.PhoneNumber = dto.PhoneNumber.Trim();
        candidate.BirthDate = dto.BirthDate;
        candidate.Cv = dto.Cv?.Trim();
        candidate.Portfolio = dto.Portfolio?.Trim();
        candidate.Education = dto.Education.Trim();
        candidate.Institution = dto.Institution.Trim();
        candidate.Address = dto.Address.Trim();
        candidate.ExpectedSalary = dto.ExpectedSalary;
        candidate.Gpa = dto.Gpa;
    }
}
