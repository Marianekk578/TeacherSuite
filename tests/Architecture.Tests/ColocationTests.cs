using System.Reflection;

namespace Architecture.Tests;

public class ColocationTests
{
    private static readonly Assembly ApplicationAssembly = Assembly.Load("TeacherSuite.Application");

    private static IEnumerable<object[]> GetHandlerRequestPairs(string namespaceSegment)
    {
        var handlerInterfaceType = typeof(MediatR.IRequestHandler<,>);

        var handlers = ApplicationAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType))
            .Where(t => t.Namespace?.Contains(namespaceSegment) == true);

        foreach (var handler in handlers)
        {
            var requestInterface = handler.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType);
            var requestType = requestInterface.GetGenericArguments()[0];

            yield return [requestType, handler];
        }
    }

    public static IEnumerable<object[]> CommandHandlerData() => GetHandlerRequestPairs("Commands");

    public static IEnumerable<object[]> QueryHandlerData() => GetHandlerRequestPairs("Queries");

    [Theory]
    [MemberData(nameof(CommandHandlerData))]
    public void CommandHandler_ShouldBeInSameNamespace_AsCommand(Type commandType, Type handlerType)
    {
        Assert.True(
            commandType.Namespace == handlerType.Namespace,
            $"Handler '{handlerType.Name}' should be in the same namespace as its command '{commandType.Name}'. " +
            $"Expected: {commandType.Namespace}, Actual: {handlerType.Namespace}");
    }

    [Theory]
    [MemberData(nameof(QueryHandlerData))]
    public void QueryHandler_ShouldBeInSameNamespace_AsQuery(Type queryType, Type handlerType)
    {
        Assert.True(
            queryType.Namespace == handlerType.Namespace,
            $"Handler '{handlerType.Name}' should be in the same namespace as its query '{queryType.Name}'. " +
            $"Expected: {queryType.Namespace}, Actual: {handlerType.Namespace}");
    }
}
