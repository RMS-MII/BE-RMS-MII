namespace WEBAPI.Common.Security;

public sealed class RefreshSessionOptions
{
    public const string SectionName = "RefreshSession";
    public int IdleDays { get; set; } = 7;
    public int AbsoluteDays { get; set; } = 30;
    public string CookieName { get; set; } = "rms_refresh";
    public string SameSite { get; set; } = "Lax";
}
