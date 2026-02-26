namespace TeacherSuite.Web.Auth;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string TeacherAccess = "TeacherAccess";
    public const string SupervisorAccess = "SupervisorAccess";

    public const string RoleAdmin = "admin";
    public const string RoleTeacher = "teacher";
    public const string RoleSupervisor = "supervisor";
}
