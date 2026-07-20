namespace IELTSLms.Domain.Audit;

public static class ActivityTypes
{
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string PasswordChanged = "PasswordChanged";
    public const string PasswordReset = "PasswordReset";
    public const string EmailVerified = "EmailVerified";
    public const string TenantCreated = "TenantCreated";
    public const string TenantSuspended = "TenantSuspended";
    public const string TenantActivated = "TenantActivated";
    public const string SessionRevoked = "SessionRevoked";
    public const string SecurityEvent = "SecurityEvent";
    public const string AdminAction = "AdminAction";
}
