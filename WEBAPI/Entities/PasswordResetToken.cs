namespace WEBAPI.Entities;

public class PasswordResetToken : BaseEntity
{
    public Guid AccountId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}