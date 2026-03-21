namespace TeacherSuite.Application.Students.Commands.Common;

public static class AgeCalculator
{
    public const int MinimumStudentAge = 7;

    public static int CalculateAge(DateTimeOffset dateOfBirth)
    {
        var today = DateTimeOffset.UtcNow;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return Math.Max(0, age);
    }

    /// <summary>
    /// Returns the latest year a student can be born in to meet the minimum age requirement.
    /// </summary>
    public static int GetMaxBirthYear()
    {
        return DateTimeOffset.UtcNow.Year - MinimumStudentAge;
    }
}
