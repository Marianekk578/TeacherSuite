namespace TeacherSuite.Domain.Common;

public abstract class BaseAuditableEntity
{
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; set; } = "System";

    public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;

    public string LastModifiedBy { get; set; } = "System";
}