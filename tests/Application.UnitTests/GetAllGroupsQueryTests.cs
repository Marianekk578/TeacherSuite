using AutoMapper;
using Moq;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Groups.Dtos;
using TeacherSuite.Application.Groups.Queries;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.UnitTests;

public class GetAllGroupsQueryTests
{
    private readonly IMapper _mapper;

    public GetAllGroupsQueryTests()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddMaps(typeof(GroupDto).Assembly),
            NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_WithNoFilter_ReturnsAllGroups()
    {
        // Arrange
        var groupAId = Guid.NewGuid();
        var groupBId = Guid.NewGuid();

        var groupA = new Group
        {
            Id = groupAId,
            Name = "Group A",
            TeacherId = Guid.NewGuid(),
            AgeGroupID = 1,
            GroupCourses = new List<GroupCourse>()
        };

        var groupB = new Group
        {
            Id = groupBId,
            Name = "Group B",
            TeacherId = Guid.NewGuid(),
            AgeGroupID = 1,
            GroupCourses = new List<GroupCourse>()
        };

        var groups = new List<Group> { groupA, groupB }.AsQueryable();
        var mockDbSet = CreateMockDbSet(groups);

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.Groups).Returns(mockDbSet.Object);

        var handler = new GetAllGroupsQueryHandler(mockDb.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllGroupsQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_WithMatchingCourseName_ReturnsFilteredGroups()
    {
        // Arrange
        var course = new Course { Id = 1, Name = "Intro to C#", Description = "Basics", AgeGroupID = 1 };
        var otherCourse = new Course { Id = 2, Name = "Advanced Python", Description = "Advanced", AgeGroupID = 1 };

        var matchingGroupId = Guid.NewGuid();
        var nonMatchingGroupId = Guid.NewGuid();

        var matchingGroup = new Group
        {
            Id = matchingGroupId,
            Name = "Group A",
            TeacherId = Guid.NewGuid(),
            AgeGroupID = 1,
            GroupCourses = new List<GroupCourse>
            {
                new() { Id = Guid.NewGuid(), GroupId = matchingGroupId, CourseId = 1, Course = course, Status = CourseAssignmentStatus.Active, StartDate = DateTimeOffset.UtcNow }
            }
        };

        var nonMatchingGroup = new Group
        {
            Id = nonMatchingGroupId,
            Name = "Group B",
            TeacherId = Guid.NewGuid(),
            AgeGroupID = 1,
            GroupCourses = new List<GroupCourse>
            {
                new() { Id = Guid.NewGuid(), GroupId = nonMatchingGroupId, CourseId = 2, Course = otherCourse, Status = CourseAssignmentStatus.Planned, StartDate = DateTimeOffset.UtcNow }
            }
        };

        var groups = new List<Group> { matchingGroup, nonMatchingGroup }.AsQueryable();
        var mockDbSet = CreateMockDbSet(groups);

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.Groups).Returns(mockDbSet.Object);

        var handler = new GetAllGroupsQueryHandler(mockDb.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllGroupsQuery(CourseName: "Intro to C#"), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("Group A", result[0].Name);
    }

    [Fact]
    public async Task Handle_WithNoMatchingCourseName_ReturnsEmptyList()
    {
        // Arrange
        var course = new Course { Id = 1, Name = "Intro to C#", Description = "Basics", AgeGroupID = 1 };

        var groupId = Guid.NewGuid();
        var group = new Group
        {
            Id = groupId,
            Name = "Group A",
            TeacherId = Guid.NewGuid(),
            AgeGroupID = 1,
            GroupCourses = new List<GroupCourse>
            {
                new() { Id = Guid.NewGuid(), GroupId = groupId, CourseId = 1, Course = course, Status = CourseAssignmentStatus.Active, StartDate = DateTimeOffset.UtcNow }
            }
        };

        var groups = new List<Group> { group }.AsQueryable();
        var mockDbSet = CreateMockDbSet(groups);

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.Groups).Returns(mockDbSet.Object);

        var handler = new GetAllGroupsQueryHandler(mockDb.Object, _mapper);

        // Act
        var result = await handler.Handle(new GetAllGroupsQuery(CourseName: "Nonexistent Course"), CancellationToken.None);

        // Assert
        Assert.Empty(result);
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

// Async query support for in-memory testing
internal class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
        var elementType = expression.Type.GetGenericArguments().FirstOrDefault() ?? typeof(TEntity);
        var queryableType = typeof(TestAsyncEnumerable<>).MakeGenericType(elementType);
        return (IQueryable)Activator.CreateInstance(queryableType, expression, inner)!;
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression) =>
        new TestAsyncEnumerable<TElement>(expression, inner);

    public object? Execute(System.Linq.Expressions.Expression expression) =>
        inner.Execute(expression);

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression) =>
        inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: [typeof(System.Linq.Expressions.Expression)])!
            .MakeGenericMethod(resultType)
            .Invoke(inner, [expression]);

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, [executionResult])!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IQueryProvider _innerProvider;

    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression, IQueryProvider innerProvider)
        : base(expression)
    {
        _innerProvider = innerProvider;
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(_innerProvider);

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
}

internal class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;
    public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
    public ValueTask DisposeAsync()
    {
        inner.Dispose();
        return ValueTask.CompletedTask;
    }
}

// Required interface for EF Core async query operations
internal interface IAsyncQueryProvider : IQueryProvider
{
    TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default);
}
