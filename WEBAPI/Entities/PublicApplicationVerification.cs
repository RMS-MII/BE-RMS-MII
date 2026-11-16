namespace WEBAPI.Entities;

public enum CandidateCodePurpose
{
    Application = 0,
    Login = 1
}

public sealed class PublicApplicationVerification : BaseEntity
{
    public Guid? BatchId { get; set; }
    public CandidateCodePurpose Purpose { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public int FailedAttempts { get; set; }
}
