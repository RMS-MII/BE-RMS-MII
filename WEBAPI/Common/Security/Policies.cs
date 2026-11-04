namespace WEBAPI.Common.Security;

public static class Policies
{
    public const string HrOnly = "HrOnly";                          // Admin or HR (role-based rule, written as a named policy)
    public const string InterviewerOnly = "InterviewerOnly";        // Interviewer (role-based rule)
    public const string HasEmployeeProfile = "HasEmployeeProfile";  // claim-based rule: the account is linked to an employee record
}

public static class RmsRoles
{
    public const string Hr = "HR";
    public const string Interviewer = "Interviewer";
}

public static class HrisClaimTypes
{
    public const string EmployeeId = "employeeId";
    public const string DepartmentId = "departmentId";
    public const string Role = "role";
}
