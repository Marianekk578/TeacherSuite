using AutoMapper;
using Moq;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.LessonPlan.Dtos;
using TeacherSuite.Application.LessonPlan.Queries;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore.Query;

namespace Application.UnitTests;

public class GetLessonPlanQueryTests
{
    private readonly IMapper _mapper;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;

    public GetLessonPlanQueryTests()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddMaps(typeof(ScheduledLessonDto).Assembly),
            NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _mockCurrentUser = new Mock<ICurrentUserService>();
        _mockCurrentUser.Setup(x => x.IsInRole(AppRoles.Admin)).Returns(true);
        _mockCurrentUser.Setup(x => x.IsInRole(AppRoles.Teacher)).Returns(false);
        _mockCurrentUser.Setup(x => x.IsInRole(AppRoles.Supervisor)).Returns(false);
        _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
    }

    [Fact]
    public async Task Handle_ReturnsAllScheduledLessons_ForAdmin()
    {
        // Arrange
        var course = new Course { Id = 1, Name = "Python Basics" };
        var group = new Group { Id = Guid.NewGuid(), Name = "Group A", TeacherId = Guid.NewGuid() };
        var lesson = new Lesson { Id = 1, CourseId = 1, Course = course, Title = "Lesson 1", Order = 1, DurationMinutes = 90 };

        var now = DateTimeOffset.UtcNow;
        var scheduledLessons = new List<ScheduledLesson>
        {
            new()
            {
                Id = Guid.NewGuid(),
                LessonId = 1,
                Lesson = lesson,
                GroupId = group.Id,
                Group = group,
                ScheduledStart = now.AddHours(1),
                ScheduledEnd = now.AddHours(2.5),
            },
            new()
            {
                Id = Guid.NewGuid(),
                LessonId = 1,
                Lesson = lesson,
                GroupId = group.Id,
                Group = group,
                ScheduledStart = now.AddDays(1),
                ScheduledEnd = now.AddDays(1).AddMinutes(90),
            }
        };

        var mockDbSet = CreateMockDbSet(scheduledLessons.AsQueryable());
        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.ScheduledLessons).Returns(mockDbSet.Object);

        var handler = new GetLessonPlanQueryHandler(mockDb.Object, _mapper, _mockCurrentUser.Object);

        // Act
        var result = await handler.Handle(new GetLessonPlanQuery(null, null), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Lesson 1", result[0].LessonTitle);
        Assert.Equal("Group A", result[0].GroupName);
        Assert.Equal("Python Basics", result[0].CourseName);
    }

    [Fact]
    public async Task Handle_FiltersScheduledLessons_ByDateRange()
    {
        // Arrange
        var course = new Course { Id = 1, Name = "Python Basics" };
        var group = new Group { Id = Guid.NewGuid(), Name = "Group A", TeacherId = Guid.NewGuid() };
        var lesson = new Lesson { Id = 1, CourseId = 1, Course = course, Title = "Lesson 1", Order = 1, DurationMinutes = 90 };

        var now = DateTimeOffset.UtcNow;
        var scheduledLessons = new List<ScheduledLesson>
        {
            new()
            {
                Id = Guid.NewGuid(),
                LessonId = 1,
                Lesson = lesson,
                GroupId = group.Id,
                Group = group,
                ScheduledStart = now.AddDays(-2),
                ScheduledEnd = now.AddDays(-2).AddMinutes(90),
            },
            new()
            {
                Id = Guid.NewGuid(),
                LessonId = 1,
                Lesson = lesson,
                GroupId = group.Id,
                Group = group,
                ScheduledStart = now.AddHours(1),
                ScheduledEnd = now.AddHours(2.5),
            }
        };

        var mockDbSet = CreateMockDbSet(scheduledLessons.AsQueryable());
        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.ScheduledLessons).Returns(mockDbSet.Object);

        var handler = new GetLessonPlanQueryHandler(mockDb.Object, _mapper, _mockCurrentUser.Object);

        // Act - filter from now onwards
        var result = await handler.Handle(new GetLessonPlanQuery(now, null), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.True(new DateTimeOffset(result[0].ScheduledEnd.DateTime, TimeSpan.Zero) >= now);
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenNoScheduledLessons()
    {
        // Arrange
        var scheduledLessons = new List<ScheduledLesson>();
        var mockDbSet = CreateMockDbSet(scheduledLessons.AsQueryable());
        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.ScheduledLessons).Returns(mockDbSet.Object);

        var handler = new GetLessonPlanQueryHandler(mockDb.Object, _mapper, _mockCurrentUser.Object);

        // Act
        var result = await handler.Handle(new GetLessonPlanQuery(null, null), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ReturnsOrderedByScheduledStart()
    {
        // Arrange
        var course = new Course { Id = 1, Name = "Python Basics" };
        var group = new Group { Id = Guid.NewGuid(), Name = "Group A", TeacherId = Guid.NewGuid() };

        var now = DateTimeOffset.UtcNow;
        var scheduledLessons = new List<ScheduledLesson>
        {
            new()
            {
                Id = Guid.NewGuid(),
                LessonId = 1,
                Lesson = new Lesson { Id = 1, CourseId = 1, Course = course, Title = "Later Lesson", Order = 2, DurationMinutes = 90 },
                GroupId = group.Id,
                Group = group,
                ScheduledStart = now.AddDays(2),
                ScheduledEnd = now.AddDays(2).AddMinutes(90),
            },
            new()
            {
                Id = Guid.NewGuid(),
                LessonId = 2,
                Lesson = new Lesson { Id = 2, CourseId = 1, Course = course, Title = "Earlier Lesson", Order = 1, DurationMinutes = 90 },
                GroupId = group.Id,
                Group = group,
                ScheduledStart = now.AddDays(1),
                ScheduledEnd = now.AddDays(1).AddMinutes(90),
            }
        };

        var mockDbSet = CreateMockDbSet(scheduledLessons.AsQueryable());
        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.ScheduledLessons).Returns(mockDbSet.Object);

        var handler = new GetLessonPlanQueryHandler(mockDb.Object, _mapper, _mockCurrentUser.Object);

        // Act
        var result = await handler.Handle(new GetLessonPlanQuery(null, null), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Earlier Lesson", result[0].LessonTitle);
        Assert.Equal("Later Lesson", result[1].LessonTitle);
    }

    [Fact]
    public async Task Handle_TeacherOnlyUser_ReturnsOwnGroupsOnly()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var otherTeacherId = Guid.NewGuid();
        var teacherEmail = "teacher@example.com";

        var mockTeacherUser = new Mock<ICurrentUserService>();
        mockTeacherUser.Setup(x => x.IsInRole(AppRoles.Admin)).Returns(false);
        mockTeacherUser.Setup(x => x.IsInRole(AppRoles.Supervisor)).Returns(false);
        mockTeacherUser.Setup(x => x.IsInRole(AppRoles.Teacher)).Returns(true);
        mockTeacherUser.Setup(x => x.Email).Returns(teacherEmail);

        var teacher = new Teacher
        {
            Id = teacherId,
            Email = teacherEmail,
            FirstName = "John",
            LastName = "Doe"
        };

        var course = new Course { Id = 1, Name = "Python Basics" };
        var myGroup = new Group { Id = Guid.NewGuid(), Name = "My Group", TeacherId = teacherId };
        var otherGroup = new Group { Id = Guid.NewGuid(), Name = "Other Group", TeacherId = otherTeacherId };

        var now = DateTimeOffset.UtcNow;
        var scheduledLessons = new List<ScheduledLesson>
        {
            new()
            {
                Id = Guid.NewGuid(),
                LessonId = 1,
                Lesson = new Lesson { Id = 1, CourseId = 1, Course = course, Title = "My Lesson", Order = 1, DurationMinutes = 90 },
                GroupId = myGroup.Id,
                Group = myGroup,
                ScheduledStart = now.AddHours(1),
                ScheduledEnd = now.AddHours(2.5),
            },
            new()
            {
                Id = Guid.NewGuid(),
                LessonId = 2,
                Lesson = new Lesson { Id = 2, CourseId = 1, Course = course, Title = "Other Lesson", Order = 2, DurationMinutes = 90 },
                GroupId = otherGroup.Id,
                Group = otherGroup,
                ScheduledStart = now.AddHours(1),
                ScheduledEnd = now.AddHours(2.5),
            }
        };

        var teachers = new List<Teacher> { teacher };
        var mockScheduledDbSet = CreateMockDbSet(scheduledLessons.AsQueryable());
        var mockTeacherDbSet = CreateMockDbSet(teachers.AsQueryable());

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.ScheduledLessons).Returns(mockScheduledDbSet.Object);
        mockDb.Setup(x => x.Teachers).Returns(mockTeacherDbSet.Object);

        var handler = new GetLessonPlanQueryHandler(mockDb.Object, _mapper, mockTeacherUser.Object);

        // Act
        var result = await handler.Handle(new GetLessonPlanQuery(null, null), CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("My Lesson", result[0].LessonTitle);
        Assert.Equal("My Group", result[0].GroupName);
    }

    [Fact]
    public async Task Handle_TeacherNotFound_ReturnsEmpty()
    {
        // Arrange
        var mockTeacherUser = new Mock<ICurrentUserService>();
        mockTeacherUser.Setup(x => x.IsInRole(AppRoles.Admin)).Returns(false);
        mockTeacherUser.Setup(x => x.IsInRole(AppRoles.Supervisor)).Returns(false);
        mockTeacherUser.Setup(x => x.IsInRole(AppRoles.Teacher)).Returns(true);
        mockTeacherUser.Setup(x => x.Email).Returns("nonexistent@example.com");

        var teachers = new List<Teacher>();
        var scheduledLessons = new List<ScheduledLesson>
        {
            new()
            {
                Id = Guid.NewGuid(),
                LessonId = 1,
                Lesson = new Lesson { Id = 1, CourseId = 1, Title = "Lesson 1", Order = 1, DurationMinutes = 90 },
                GroupId = Guid.NewGuid(),
                Group = new Group { Id = Guid.NewGuid(), Name = "Group A", TeacherId = Guid.NewGuid() },
                ScheduledStart = DateTimeOffset.UtcNow.AddHours(1),
                ScheduledEnd = DateTimeOffset.UtcNow.AddHours(2.5),
            }
        };

        var mockScheduledDbSet = CreateMockDbSet(scheduledLessons.AsQueryable());
        var mockTeacherDbSet = CreateMockDbSet(teachers.AsQueryable());

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(x => x.ScheduledLessons).Returns(mockScheduledDbSet.Object);
        mockDb.Setup(x => x.Teachers).Returns(mockTeacherDbSet.Object);

        var handler = new GetLessonPlanQueryHandler(mockDb.Object, _mapper, mockTeacherUser.Object);

        // Act
        var result = await handler.Handle(new GetLessonPlanQuery(null, null), CancellationToken.None);

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
