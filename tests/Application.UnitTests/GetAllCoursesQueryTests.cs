using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Linq.Expressions;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Common.Models;
using TeacherSuite.Application.Courses.Dtos;
using TeacherSuite.Application.Courses.Queries;
using TeacherSuite.Domain.Entities;

namespace Application.UnitTests;

public class GetAllCoursesQueryTests
{
    private readonly IMapper _mapper;

    public GetAllCoursesQueryTests()
    {
        var config = new MapperConfiguration(cfg =>
            cfg.AddMaps(typeof(CourseDto).Assembly), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    private static Mock<IApplicationDbContext> CreateMockContext(List<Course> courses)
    {
        IQueryable<Course> asyncEnumerable = TestAsyncEnumerable<Course>.FromEnumerable(courses);

        var mockDbSet = new Mock<DbSet<Course>>();
        mockDbSet.As<IAsyncEnumerable<Course>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new TestAsyncEnumerator<Course>(courses.GetEnumerator()));

        mockDbSet.As<IQueryable<Course>>()
            .Setup(m => m.Provider)
            .Returns(asyncEnumerable.Provider);
        mockDbSet.As<IQueryable<Course>>()
            .Setup(m => m.Expression)
            .Returns(asyncEnumerable.Expression);
        mockDbSet.As<IQueryable<Course>>()
            .Setup(m => m.ElementType)
            .Returns(asyncEnumerable.ElementType);
        mockDbSet.As<IQueryable<Course>>()
            .Setup(m => m.GetEnumerator())
            .Returns(() => courses.GetEnumerator());

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Courses).Returns(mockDbSet.Object);
        return mockContext;
    }

    private static List<Course> GenerateCourses(int count)
    {
        var courses = new List<Course>();
        for (var i = 1; i <= count; i++)
        {
            courses.Add(new Course
            {
                Id = i,
                Name = $"Course {i:D3}",
                Description = $"Description for course {i}",
                AgeGroupID = (i % 3) + 1,
                AgeGroup = new AgeGroup { Id = (i % 3) + 1, Name = $"Group {(i % 3) + 1}", MinAge = 6, MaxAge = 12 }
            });
        }
        return courses;
    }

    [Fact]
    public async Task Handle_ReturnsFirstPage_WhenNoPageSpecified()
    {
        var courses = GenerateCourses(25);
        var mockContext = CreateMockContext(courses);
        var handler = new GetAllCoursesQueryHandler(mockContext.Object, _mapper);

        var result = await handler.Handle(new GetAllCoursesQuery(), CancellationToken.None);

        Assert.Equal(25, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(12, result.PageSize);
        Assert.Equal(12, result.Items.Count);
    }

    [Fact]
    public async Task Handle_ReturnsSecondPage()
    {
        var courses = GenerateCourses(25);
        var mockContext = CreateMockContext(courses);
        var handler = new GetAllCoursesQueryHandler(mockContext.Object, _mapper);

        var result = await handler.Handle(
            new GetAllCoursesQuery { Page = 2, PageSize = 10 },
            CancellationToken.None);

        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(10, result.Items.Count);
    }

    [Fact]
    public async Task Handle_ReturnsLastPage_WithRemainingItems()
    {
        var courses = GenerateCourses(25);
        var mockContext = CreateMockContext(courses);
        var handler = new GetAllCoursesQueryHandler(mockContext.Object, _mapper);

        var result = await handler.Handle(
            new GetAllCoursesQuery { Page = 3, PageSize = 10 },
            CancellationToken.None);

        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(5, result.Items.Count);
    }

    [Fact]
    public async Task Handle_FiltersResults_WhenSearchIsProvided()
    {
        var courses = new List<Course>
        {
            new() { Id = 1, Name = "Python Basics", Description = "Learn Python", AgeGroupID = 1 },
            new() { Id = 2, Name = "Java Advanced", Description = "Advanced Java", AgeGroupID = 1 },
            new() { Id = 3, Name = "Web Development", Description = "Build with Python and Django", AgeGroupID = 2 },
        };
        var mockContext = CreateMockContext(courses);
        var handler = new GetAllCoursesQueryHandler(mockContext.Object, _mapper);

        var result = await handler.Handle(
            new GetAllCoursesQuery { Search = "python" },
            CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task Handle_ClampsPageSize_WhenExceedsMax()
    {
        var courses = GenerateCourses(5);
        var mockContext = CreateMockContext(courses);
        var handler = new GetAllCoursesQueryHandler(mockContext.Object, _mapper);

        var result = await handler.Handle(
            new GetAllCoursesQuery { PageSize = 200 },
            CancellationToken.None);

        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task Handle_DefaultsToPage1_WhenPageIsZeroOrNegative()
    {
        var courses = GenerateCourses(5);
        var mockContext = CreateMockContext(courses);
        var handler = new GetAllCoursesQueryHandler(mockContext.Object, _mapper);

        var result = await handler.Handle(
            new GetAllCoursesQuery { Page = -1 },
            CancellationToken.None);

        Assert.Equal(1, result.Page);
    }

    #region Async Query Helpers

    private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            // Determine the element type from the expression (e.g., IQueryable<CourseDto>)
            var elementType = expression.Type.GetGenericArguments()[0];
            var fromExprMethod = typeof(TestAsyncEnumerable<>)
                .MakeGenericType(elementType)
                .GetMethod(nameof(TestAsyncEnumerable<TEntity>.FromExpression),
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!;
            return (IQueryable)fromExprMethod.Invoke(null, [expression])!;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => TestAsyncEnumerable<TElement>.FromExpression(expression);

        public object? Execute(Expression expression)
            => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
            => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executorMethod = typeof(IQueryProvider)
                .GetMethods()
                .Single(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(expectedResultType);

            var syncResult = executorMethod.Invoke(_inner, [expression]);

            var fromResultMethod = typeof(Task)
                .GetMethods()
                .Single(m => m.Name == nameof(Task.FromResult) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(expectedResultType);

            return (TResult)fromResultMethod.Invoke(null, [syncResult])!;
        }
    }

    private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        private readonly IQueryProvider _asyncProvider;

        private TestAsyncEnumerable(IEnumerable<T> enumerable, IQueryProvider innerProvider) : base(enumerable)
        {
            _asyncProvider = new TestAsyncQueryProvider<T>(innerProvider);
        }

        private TestAsyncEnumerable(Expression expression, IQueryProvider innerProvider) : base(expression)
        {
            _asyncProvider = new TestAsyncQueryProvider<T>(innerProvider);
        }

        public static TestAsyncEnumerable<T> FromEnumerable(IEnumerable<T> enumerable)
        {
            var eq = new EnumerableQuery<T>(enumerable);
            var innerProvider = ((IQueryable<T>)eq).Provider;
            return new TestAsyncEnumerable<T>(enumerable, innerProvider);
        }

        public static TestAsyncEnumerable<T> FromExpression(Expression expression)
        {
            var eq = new EnumerableQuery<T>(expression);
            var innerProvider = ((IQueryable<T>)eq).Provider;
            return new TestAsyncEnumerable<T>(expression, innerProvider);
        }

        IQueryProvider IQueryable.Provider => _asyncProvider;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    private class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;

        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
            => new(inner.MoveNext());
    }

    #endregion
}
