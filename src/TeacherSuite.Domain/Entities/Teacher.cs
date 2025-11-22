using System.ComponentModel.DataAnnotations;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Domain.Entities;

public class Teacher : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }

    public ICollection<Group> Groups { get; set; } = new List<Group>();
    public ICollection<TeacherProgrammingLanguage> TeacherProgrammingLanguages { get; set; } = new List<TeacherProgrammingLanguage>();
}