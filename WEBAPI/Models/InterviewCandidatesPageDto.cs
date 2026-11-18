namespace WEBAPI.Models;

public sealed class InterviewCandidatesPageDto
{
    public IReadOnlyCollection<InterviewCandidateOptionDto> Items { get; set; } = [];
    public bool HasMore { get; set; }
}
