using WEBAPI.Features.Batches;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Services;

namespace WEBAPI.Models;

public static class ApplicationRecapMapping
{
    public static ApplicationRecapDto ToRecapDto(this BatchApplication application)
    {
        var applicationStages = application.ApplicationStages
            .ToDictionary(stage => stage.BatchStageId);

        var activeStages = BatchStageSequence.GetOrderedStages(application.RecruitmentBatch.BatchStages.ToList())
            .Where(stage => stage.IsActive || applicationStages.ContainsKey(stage.Id))
            .ToList();

        var stages = activeStages
            .Select((batchStage, index) => batchStage.ToRecapDto(
                applicationStages.GetValueOrDefault(batchStage.Id),
                index + 1))
            .ToList();

        return new ApplicationRecapDto
        {
            Id = application.Id,
            CandidateId = application.CandidateId,
            RecruitmentBatchId = application.RecruitmentBatchId,
            IsStandalone = application.IsStandalone,
            CodeApplication = application.CodeApplication,
            AppliedAt = application.AppliedAt,
            FinalResultStatus = application.FinalResultStatus,
            Status = application.Status,
            Notes = application.Notes,
            OfferingLetterStatus = application.OfferingLetterStatus,
            FileOfferingLetter = application.FileOfferingLetter,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt,
            Candidate = application.Candidate.ToDto(),
            Batch = new RecruitmentBatchSummaryDto
            {
                Id = application.RecruitmentBatch.Id,
                DepartmentId = application.RecruitmentBatch.DepartmentId,
                DepartmentName = application.RecruitmentBatch.Department?.Name,
                BatchCode = application.RecruitmentBatch.BatchCode,
                Name = application.RecruitmentBatch.Name,
                Description = application.RecruitmentBatch.Description,
                StartDate = application.RecruitmentBatch.StartDate,
                EndDate = application.RecruitmentBatch.EndDate,
                Status = application.RecruitmentBatch.Status
            },
            Stages = stages,
            Summary = BuildSummary(stages, application)
        };
    }

    private static ApplicationStageRecapDto ToRecapDto(
        this BatchStage batchStage,
        ApplicationStage? applicationStage,
        int sequence)
    {
        var status = applicationStage?.Status;

        return new ApplicationStageRecapDto
        {
            ApplicationStageId = applicationStage?.Id,
            BatchStageId = batchStage.Id,
            LifecycleStatus = batchStage.LifecycleStatus,
            Sequence = sequence,
            StageName = batchStage.StageName,
            StageTypeId = batchStage.StageTypeId,
            StageTypeName = batchStage.StageType?.Name,
            StageCategory = batchStage.StageType?.StageCategory,
            PassingScore = batchStage.PassingScore,
            StartDate = batchStage.StartDate,
            EndDate = batchStage.EndDate,
            NextStageId = batchStage.NextStageId,
            Status = status,
            TotalScore = applicationStage?.TotalScore,
            DiscSummaries = applicationStage is null ||
                            batchStage.StageType?.StageCategory != StageCategory.Test
                ? []
                : DiscSummaryCalculator.Summarize(applicationStage.CandidateAnswers),
            StartedAt = applicationStage?.StartedAt,
            EndAt = applicationStage?.EndAt,
            CompletedAt = applicationStage?.CompletedAt,
            Notes = applicationStage?.Notes,
            IsAttempts = applicationStage?.IsAttempts ?? false,
            DurationTest = applicationStage?.DurationTest,
            IsStarted = applicationStage is not null,
            IsCompleted = status?.IsTerminalStage() == true,
            IsPassed = status?.IsPassingStage() == true,
            Interviewers = batchStage.StageEmployees
                .Where(employee => employee.IsActive)
                .OrderBy(employee => employee.Employee.FullName)
                .Select(employee => new StageInterviewerDto
                {
                    Id = employee.Id,
                    EmployeeId = employee.EmployeeId,
                    FullName = employee.Employee.FullName,
                    Email = employee.Employee.Email,
                    JobTitle = employee.Employee.JobTitle,
                    Status = employee.Status,
                    Note = employee.Note
                })
                .ToList(),
            InterviewSessions = applicationStage is null
                ? []
                : applicationStage.InterviewSessions
                    .OrderBy(session => session.ScheduledStart)
                    .Select(ToRecapDto)
                    .ToList(),
            TestAnswers = applicationStage is null
                ? []
                : applicationStage.CandidateAnswers
                    .OrderBy(answer => answer.CreatedAt)
                    .Select(answer => new CandidateAnswerDto
                    {
                        Id = answer.Id,
                        QuestionText = answer.QuestionText,
                        QuestionOptionText = answer.QuestionOptionText,
                        Score = answer.Score,
                        CreatedAt = answer.CreatedAt,
                        UpdatedAt = answer.UpdatedAt
                    })
                    .ToList()
        };
    }

    private static InterviewSessionRecapDto ToRecapDto(this InterviewSession session) => new()
    {
        Id = session.Id,
        SessionName = session.SessionName,
        ScheduledStart = session.ScheduledStart,
        ScheduledEnd = session.ScheduledEnd,
        IsOnline = session.IsOnline,
        Location = session.Location,
        MeetingUrl = session.MeetingUrl,
        Status = session.Status,
        FileSubmission = session.SubmissionFile,
        EndedSubmissionAt = session.EndedSubmissionAt,
        Interviewers = session.SessionInterviews
            .OrderBy(sessionInterview => sessionInterview.StageEmployee.Employee.FullName)
            .Select(sessionInterview => new SessionInterviewerRecapDto
            {
                Id = sessionInterview.Id,
                StageEmployeeId = sessionInterview.StageEmployeeId,
                EmployeeId = sessionInterview.StageEmployee.EmployeeId,
                EmployeeName = sessionInterview.StageEmployee.Employee.FullName,
                Status = sessionInterview.Status,
                IsLead = sessionInterview.IsLead,
                Assessment = sessionInterview.InterviewAssessment is null
                    ? null
                    : new InterviewAssessmentDto
                    {
                        Id = sessionInterview.InterviewAssessment.Id,
                        Notes = sessionInterview.InterviewAssessment.Notes,
                        Recommendation = sessionInterview.InterviewAssessment.Recommendation,
                        Score = sessionInterview.InterviewAssessment.Score,
                        CreatedAt = sessionInterview.InterviewAssessment.CreatedAt,
                        UpdatedAt = sessionInterview.InterviewAssessment.UpdatedAt
                    }
            })
            .ToList()
    };

    private static ApplicationRecapSummaryDto BuildSummary(
        IReadOnlyCollection<ApplicationStageRecapDto> stages,
        BatchApplication application)
    {
        var currentStage = stages.FirstOrDefault(stage => !stage.IsCompleted);

        return new ApplicationRecapSummaryDto
        {
            TotalStages = stages.Count,
            StartedStages = stages.Count(stage => stage.IsStarted),
            CompletedStages = stages.Count(stage => stage.IsCompleted),
            PassedStages = stages.Count(stage => stage.IsPassed),
            RejectedStages = stages.Count(stage =>
                stage.Status is ApplicationStageStatus.Rejected
                    or ApplicationStageStatus.Dropped
                    or ApplicationStageStatus.Expired),
            CurrentBatchStageId = currentStage?.BatchStageId,
            CurrentApplicationStageId = currentStage?.ApplicationStageId,
            CurrentStageName = currentStage?.StageName,
            CurrentStageStatus = currentStage?.Status,
            IsAllStagesCompleted = stages.Count > 0 && stages.All(stage => stage.IsCompleted),
            CanFinalize = stages.Count > 0 &&
                          stages.All(stage => stage.IsCompleted) &&
                          application.Status == ApplicationStatus.InProcess &&
                          application.FinalResultStatus is null
        };
    }
}
