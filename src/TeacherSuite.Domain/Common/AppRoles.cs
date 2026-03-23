namespace TeacherSuite.Domain.Common;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Supervisor = "Supervisor";

    public static class Policies
    {
        public const string AdminOrSupervisor = $"{Admin},{Supervisor}";
        public const string AdminSupervisorOrTeacher = $"{Admin},{Supervisor},{Teacher}";
    }
}
