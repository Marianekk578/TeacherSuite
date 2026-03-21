using TeacherSuite.Application.Students.Commands.Common;

namespace Application.UnitTests;

public class AgeCalculatorTests
{
    [Fact]
    public void CalculateAge_BirthdayToday_ReturnsCorrectAge()
    {
        // A person born exactly N years ago today
        var today = DateTimeOffset.UtcNow;
        var dob = today.AddYears(-10);

        var age = AgeCalculator.CalculateAge(dob);

        Assert.Equal(10, age);
    }

    [Fact]
    public void CalculateAge_BirthdayTomorrow_ReturnsOneYearLess()
    {
        // Birthday is tomorrow — hasn't turned the next age yet
        var today = DateTimeOffset.UtcNow;
        var dob = today.AddYears(-10).AddDays(1);

        var age = AgeCalculator.CalculateAge(dob);

        Assert.Equal(9, age);
    }

    [Fact]
    public void CalculateAge_BirthdayYesterday_ReturnsFullAge()
    {
        // Birthday was yesterday — has already turned the next age
        var today = DateTimeOffset.UtcNow;
        var dob = today.AddYears(-10).AddDays(-1);

        var age = AgeCalculator.CalculateAge(dob);

        Assert.Equal(10, age);
    }

    [Fact]
    public void CalculateAge_BornToday_ReturnsZero()
    {
        var today = DateTimeOffset.UtcNow;

        var age = AgeCalculator.CalculateAge(today);

        Assert.Equal(0, age);
    }

    [Fact]
    public void CalculateAge_BornYesterday_ReturnsZero()
    {
        var yesterday = DateTimeOffset.UtcNow.AddDays(-1);

        var age = AgeCalculator.CalculateAge(yesterday);

        Assert.Equal(0, age);
    }

    [Fact]
    public void CalculateAge_NeverReturnsNegative()
    {
        // Even if someone enters a date in the future, age should be 0
        var futureDate = DateTimeOffset.UtcNow.AddDays(30);

        var age = AgeCalculator.CalculateAge(futureDate);

        Assert.True(age >= 0, "Age should never be negative");
    }

    [Fact]
    public void CalculateAge_ExactlyEighteen_ReturnsEighteen()
    {
        var today = DateTimeOffset.UtcNow;
        var dob = today.AddYears(-18);

        var age = AgeCalculator.CalculateAge(dob);

        Assert.Equal(18, age);
    }

    [Fact]
    public void CalculateAge_OneDayBeforeEighteen_ReturnsSeventeen()
    {
        var today = DateTimeOffset.UtcNow;
        var dob = today.AddYears(-18).AddDays(1);

        var age = AgeCalculator.CalculateAge(dob);

        Assert.Equal(17, age);
    }

    [Fact]
    public void GetMaxBirthYear_ReturnsCurrentYearMinusMinAge()
    {
        var expected = DateTimeOffset.UtcNow.Year - AgeCalculator.MinimumStudentAge;

        var result = AgeCalculator.GetMaxBirthYear();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void MinimumStudentAge_IsSeven()
    {
        Assert.Equal(7, AgeCalculator.MinimumStudentAge);
    }
}
