using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Features.Portal;

namespace WEBAPI.Models;

public static class InterviewerDashboardMapping
{
    public static DashboardInterviewSessionDto ToDashboardInterviewSessionDto(this SessionInterview si)
    {
        var session = si.InterviewSession;
        var applicationStage = session.ApplicationStage;
        var candidate = applicationStage?.BatchApplication?.Candidate;
        var stage = session.ApplicationStage?.BatchStage;
        var batch = stage?.RecruitmentBatch;
        var assessment = si.InterviewAssessment;

        return new DashboardInterviewSessionDto
        {
            SessionInterviewId = si.Id,
            InterviewSessionId = session.Id,
            SessionName = session.SessionName,
            ScheduledStart = session.ScheduledStart,
            ScheduledEnd = session.ScheduledEnd,
            IsOnline = session.IsOnline,
            MeetingUrl = session.MeetingUrl,
            Location = session.Location,
            SessionStatus = session.Status,
            InterviewerStatus = si.Status,
            CandidateId = candidate?.Id ?? Guid.Empty,
            CandidateName = candidate?.FullName ?? string.Empty,
            CandidateEmail = candidate?.Email ?? string.Empty,
            BatchName = batch?.Name ?? string.Empty,
            StageName = stage?.StageName ?? string.Empty,
            CanAssignTakeHome = si.Status != SessionInterviewStatus.Cancelled
                && session.Status == InterviewSessionStatus.AcceptedByCandidate
                && applicationStage is not null
                && CandidateActionPolicy.IsActive(applicationStage),
            HasTakeHome = session.EndedSubmissionAt.HasValue || !string.IsNullOrWhiteSpace(session.SubmissionFile),
            EndedSubmissionAt = session.EndedSubmissionAt,
            FileTakeHome = session.FileTakeHome,
            FileSubmission = session.SubmissionFile,
            IsLead = si.IsLead,
            IsAssessmentSubmitted = assessment is not null,
            AssessmentScore = assessment?.Score,
            AssessmentRecommendation = assessment?.Recommendation
        };
    }
}

