using WEBAPI.Entities;

namespace WEBAPI.Models;

public static class InterviewAssessmentMapping
{
    public static AssessmentDto ToAssessmentDto(
        this InterviewAssessment assessment,
        SessionInterview si)
    {
        var session = si.InterviewSession;
        var candidate = session.ApplicationStage?.BatchApplication?.Candidate;

        return new AssessmentDto
        {
            AssessmentId = assessment.Id,
            SessionInterviewId = si.Id,
            InterviewSessionId = session.Id,
            SessionName = session.SessionName,
            CandidateId = candidate?.Id ?? Guid.Empty,
            CandidateName = candidate?.FullName ?? string.Empty,
            Score = assessment.Score,
            Recommendation = assessment.Recommendation,
            Notes = assessment.Notes,
            InterviewerStatus = si.Status,
            CreatedAt = assessment.CreatedAt,
            UpdatedAt = assessment.UpdatedAt
        };
    }
}

