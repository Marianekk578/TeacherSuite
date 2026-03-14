using System.Reflection;

namespace Architecture.Tests;

public class VisibilityTests
{
    private static readonly Assembly ApplicationAssembly = Assembly.Load("TeacherSuite.Application");

    public static IEnumerable<object[]> CommandHandlerTypes()
    {
        var handlerInterfaceType = typeof(MediatR.IRequestHandler<,>);

        return ApplicationAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType))
            .Where(t => t.Namespace?.Contains("Commands") == true)
            .Select(t => new object[] { t });
    }

    public static IEnumerable<object[]> QueryHandlerTypes()
    {
        var handlerInterfaceType = typeof(MediatR.IRequestHandler<,>);

        return ApplicationAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType))
            .Where(t => t.Namespace?.Contains("Queries") == true)
            .Select(t => new object[] { t });
    }

    [Theory]
    [MemberData(nameof(CommandHandlerTypes))]
    public void CommandHandler_ShouldBeInternal(Type handlerType)
    {
        Assert.False(
            handlerType.IsPublic,
            $"Command handler '{handlerType.Name}' should be internal, not public.");
    }

    [Theory]
    [MemberData(nameof(CommandHandlerTypes))]
    public void CommandHandler_ShouldBeSealed(Type handlerType)
    {
        Assert.True(
            handlerType.IsSealed,
            $"Command handler '{handlerType.Name}' should be sealed.");
    }

    [Theory]
    [MemberData(nameof(QueryHandlerTypes))]
    public void QueryHandler_ShouldBeInternal(Type handlerType)
    {
        Assert.False(
            handlerType.IsPublic,
            $"Query handler '{handlerType.Name}' should be internal, not public.");
    }

    [Theory]
    [MemberData(nameof(QueryHandlerTypes))]
    public void QueryHandler_ShouldBeSealed(Type handlerType)
    {
        Assert.True(
            handlerType.IsSealed,
            $"Query handler '{handlerType.Name}' should be sealed.");
    }
}
