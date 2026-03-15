using AutoMapper;
using Moq;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Courses.Dtos;
using TeacherSuite.Application.Courses.Queries;
using TeacherSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.UnitTests;

public class GetAllCoursesQueryTests
{
    private readonly IMapper _mapper;

    public GetAllCoursesQueryTests()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddMaps(typeof(CourseDto).Assembly),
            NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_ReturnsPagedResult_WithDefaults()
    {
        // Arrange
        var courses = Enumerable.Range(1, 15).Select(i => new Course
        {
            Id = i,
            Name = $"Course {i:D2}",
            Description = $"Description {i}",
            AgeGroupID = 1,
            CourseProgrammingLanguages = new List<CourseProgrammingLanguage>()
        }).ToList();

        var queryable = courses.AsQueryable();
        var mockDbSet = CreateMockDbSet(queryable);

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.Courses).Returns(mockDbSet.Object);

        var handler = new GetAllCoursesQueryHandler(mockDb.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllCoursesQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(12, result.PageSize);
        Assert.Equal(12, result.Items.Count);
    }

    [Fact]
    public async Task Handle_ReturnsSecondPage()
    {
        // Arrange
        var courses = Enumerable.Range(1, 15).Select(i => new Course
        {
            Id = i,
            Name = $"Course {i:D2}",
            Description = $"Description {i}",
            AgeGroupID = 1,
            CourseProgrammingLanguages = new List<CourseProgrammingLanguage>()
        }).ToList();

        var queryable = courses.AsQueryable();
        var mockDbSet = CreateMockDbSet(queryable);

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.Courses).Returns(mockDbSet.Object);

        var handler = new GetAllCoursesQueryHandler(mockDb.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllCoursesQuery { Page = 2, PageSize = 10 }, CancellationToken.None);

        // Assert
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(5, result.Items.Count);
    }

    [Fact]
    public async Task Handle_ClampsPageSizeToMax100()
    {
        // Arrange
        var courses = new List<Course>
        {
            new() { Id = 1, Name = "Course 1", Description = "Desc", AgeGroupID = 1, CourseProgrammingLanguages = new List<CourseProgrammingLanguage>() }
        }.AsQueryable();

        var mockDbSet = CreateMockDbSet(courses);

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.Courses).Returns(mockDbSet.Object);

        var handler = new GetAllCoursesQueryHandler(mockDb.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllCoursesQuery { PageSize = 200 }, CancellationToken.None);

        // Assert
        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task Handle_ClampsPageToMinimum1()
    {
        // Arrange
        var courses = new List<Course>
        {
            new() { Id = 1, Name = "Course 1", Description = "Desc", AgeGroupID = 1, CourseProgrammingLanguages = new List<CourseProgrammingLanguage>() }
        }.AsQueryable();

        var mockDbSet = CreateMockDbSet(courses);

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.Courses).Returns(mockDbSet.Object);

        var handler = new GetAllCoursesQueryHandler(mockDb.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllCoursesQuery { Page = -5 }, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Page);
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
