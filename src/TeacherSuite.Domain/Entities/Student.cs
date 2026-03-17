using System.ComponentModel.DataAnnotations;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Domain.Entities;

public class Student : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTimeOffset DateOfBirth { get; set; }
    [EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? ParentFirstName { get; set; }
    public string? ParentLastName { get; set; }
    public ICollection<StudentGroup> StudentGroups { get; set; } = new List<StudentGroup>();
}