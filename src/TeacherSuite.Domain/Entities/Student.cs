using System.ComponentModel.DataAnnotations;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Domain.Entities;

public class Student : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int Age { get; set; }
    [EmailAddress]
    public string ParentsEmail { get; set; } = string.Empty;
    public Guid GroupId { get; set; }
    public Group? Group { get; set; }
}