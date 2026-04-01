using TeacherSuite.Application.Lessons.Commands.Common;

namespace TeacherSuite.Application.Lessons.Commands.Create;

public class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        LessonValidationRules.ApplyCommonRules(this,
            x => x.Title,
            x => x.CourseId,
            x => x.DurationMinutes);
    }
}
