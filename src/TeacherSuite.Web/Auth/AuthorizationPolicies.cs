using TeacherSuite.Domain.Common;

namespace TeacherSuite.Web.Auth;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string TeacherAccess = "TeacherAccess";
    public const string SupervisorAccess = "SupervisorAccess";

    public static readonly string RoleAdmin = AppRoles.Admin;
    public static readonly string RoleTeacher = AppRoles.Teacher;
    public static readonly string RoleSupervisor = AppRoles.Supervisor;
}
