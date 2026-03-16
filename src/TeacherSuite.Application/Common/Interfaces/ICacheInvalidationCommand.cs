namespace TeacherSuite.Application.Common.Interfaces;

public interface ICacheInvalidationCommand
{
    IReadOnlyCollection<string> TagsToInvalidate { get; }
}
