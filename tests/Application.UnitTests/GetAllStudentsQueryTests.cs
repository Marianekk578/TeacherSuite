using AutoMapper;
using Moq;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Students.Dtos;
using TeacherSuite.Application.Students.Queries;
using TeacherSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.UnitTests;

public class GetAllStudentsQueryTests
{
    private readonly IMapper _mapper;

    public GetAllStudentsQueryTests()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddMaps(typeof(StudentDto).Assembly),
            NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_ReturnsPagedResult_WithDefaults()
    {
        // Arrange
        var students = Enumerable.Range(1, 15).Select(i => new Student
        {
            Id = Guid.NewGuid(),
            FirstName = $"First{i:D2}",
            LastName = $"Last{i:D2}",
            DateOfBirth = DateTimeOffset.UtcNow.AddYears(-10),
            ContactEmail = $"student{i}@example.com",
            ContactPhone = $"+48 123 456 {i:D3}",
            StudentGroups = new List<StudentGroup>()
        }).ToList();

        var queryable = students.AsQueryable();
        var mockDbSet = CreateMockDbSet(queryable);

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.Students).Returns(mockDbSet.Object);

        var handler = new GetAllStudentsQueryHandler(mockDb.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllStudentsQuery(), CancellationToken.None);

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
        var students = Enumerable.Range(1, 15).Select(i => new Student
        {
            Id = Guid.NewGuid(),
            FirstName = $"First{i:D2}",
            LastName = $"Last{i:D2}",
            DateOfBirth = DateTimeOffset.UtcNow.AddYears(-10),
            ContactEmail = $"student{i}@example.com",
            ContactPhone = $"+48 123 456 {i:D3}",
            StudentGroups = new List<StudentGroup>()
        }).ToList();

        var queryable = students.AsQueryable();
        var mockDbSet = CreateMockDbSet(queryable);

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.Students).Returns(mockDbSet.Object);

        var handler = new GetAllStudentsQueryHandler(mockDb.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllStudentsQuery { Page = 2, PageSize = 10 }, CancellationToken.None);

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
        var students = new List<Student>
        {
            new() { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-10), ContactEmail = "john@example.com", ContactPhone = "+48 123", StudentGroups = new List<StudentGroup>() }
        }.AsQueryable();

        var mockDbSet = CreateMockDbSet(students);

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.Students).Returns(mockDbSet.Object);

        var handler = new GetAllStudentsQueryHandler(mockDb.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllStudentsQuery { PageSize = 200 }, CancellationToken.None);

        // Assert
        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task Handle_ClampsPageToMinimum1()
    {
        // Arrange
        var students = new List<Student>
        {
            new() { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-10), ContactEmail = "john@example.com", ContactPhone = "+48 123", StudentGroups = new List<StudentGroup>() }
        }.AsQueryable();

        var mockDbSet = CreateMockDbSet(students);

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.Students).Returns(mockDbSet.Object);

        var handler = new GetAllStudentsQueryHandler(mockDb.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllStudentsQuery { Page = -5 }, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task Handle_FiltersStudentsBySearch()
    {
        // Arrange
        var students = new List<Student>
        {
            new() { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Smith", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-12), ContactEmail = "alice@example.com", ContactPhone = "+48 111", StudentGroups = new List<StudentGroup>() },
            new() { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Jones", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-14), ContactEmail = "bob@example.com", ContactPhone = "+48 222", StudentGroups = new List<StudentGroup>() },
            new() { Id = Guid.NewGuid(), FirstName = "Charlie", LastName = "Brown", DateOfBirth = DateTimeOffset.UtcNow.AddYears(-10), ContactEmail = "charlie@example.com", ContactPhone = "+48 333", StudentGroups = new List<StudentGroup>() },
        }.AsQueryable();

        var mockDbSet = CreateMockDbSet(students);

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.Students).Returns(mockDbSet.Object);

        var handler = new GetAllStudentsQueryHandler(mockDb.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllStudentsQuery { Search = "alice" }, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Alice", result.Items[0].FirstName);
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
