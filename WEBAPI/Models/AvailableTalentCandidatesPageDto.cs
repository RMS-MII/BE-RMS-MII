namespace WEBAPI.Models;

public sealed class AvailableTalentCandidatesPageDto
{
    public IReadOnlyCollection<AvailableTalentCandidateDto> Items { get; set; } = [];
    public bool HasMore { get; set; }
}
