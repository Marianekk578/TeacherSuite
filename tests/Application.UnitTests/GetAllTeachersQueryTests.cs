using AutoMapper;
using Moq;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Teachers.Dtos;
using TeacherSuite.Application.Teachers.Queries.Get;
using TeacherSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.UnitTests;

public class GetAllTeachersQueryTests
{
    private readonly IMapper _mapper;

    public GetAllTeachersQueryTests()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddMaps(typeof(TeacherDto).Assembly),
            NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_FiltersTeachersByFullName()
    {
        var teachers = new List<Teacher>
        {
            new() { Id = Guid.NewGuid(), FirstName = "Marcin", LastName = "Wójcik", Email = "marcin@example.com", PhoneNumber = "+48 111", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-30), Groups = new List<Group>(), TeacherProgrammingLanguages = new List<TeacherProgrammingLanguage>() },
            new() { Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Kowalska", Email = "anna@example.com", PhoneNumber = "+48 222", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-35), Groups = new List<Group>(), TeacherProgrammingLanguages = new List<TeacherProgrammingLanguage>() },
            new() { Id = Guid.NewGuid(), FirstName = "Jan", LastName = "Nowak", Email = "jan@example.com", PhoneNumber = "+48 333", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-40), Groups = new List<Group>(), TeacherProgrammingLanguages = new List<TeacherProgrammingLanguage>() },
        }.AsQueryable();

        var mockDbSet = CreateMockDbSet(teachers);

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.Teachers).Returns(mockDbSet.Object);

        var handler = new GetAllTeachersQueryHandler(mockDb.Object, _mapper);

        var result = await handler.Handle(new GetAllTeachersQuery { Search = "Marcin Wójcik" }, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Marcin", result.Items[0].FirstName);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mockSet;
    }
}
