namespace TeacherSuite.Application.Common.Interfaces;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AuthorizeAttribute : Attribute
{
    public string? Roles { get; set; }

    public AuthorizeAttribute() { }

    public AuthorizeAttribute(string roles)
    {
        Roles = roles;
    }
}
