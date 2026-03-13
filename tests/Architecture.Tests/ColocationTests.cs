using System.Reflection;

namespace Architecture.Tests;

public class ColocationTests
{
    private static readonly Assembly ApplicationAssembly =
        typeof(TeacherSuite.Application.Teachers.Commands.Create.CreateTeacherCommand).Assembly;

    public static IEnumerable<object[]> CommandHandlerPairs()
    {
        var handlerInterfaceDefinition = typeof(MediatR.IRequestHandler<,>);

        var handlers = ApplicationAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceDefinition))
            .Where(t => t.Namespace?.Contains("Commands") == true);

        foreach (var handler in handlers)
        {
            var handlerInterface = handler.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceDefinition);

            var commandType = handlerInterface.GetGenericArguments()[0];

            yield return [commandType.Name, handler.Name, commandType.Namespace!, handler.Namespace!];
        }
    }

    public static IEnumerable<object[]> QueryHandlerPairs()
    {
        var handlerInterfaceDefinition = typeof(MediatR.IRequestHandler<,>);

        var handlers = ApplicationAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceDefinition))
            .Where(t => t.Namespace?.Contains("Queries") == true);

        foreach (var handler in handlers)
        {
            var handlerInterface = handler.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceDefinition);

            var queryType = handlerInterface.GetGenericArguments()[0];

            yield return [queryType.Name, handler.Name, queryType.Namespace!, handler.Namespace!];
        }
    }

    [Theory]
    [MemberData(nameof(CommandHandlerPairs))]
    public void Command_And_Handler_ShouldBeInSameNamespace(
        string commandName, string handlerName, string commandNamespace, string handlerNamespace)
    {
        Assert.True(
            commandNamespace == handlerNamespace,
            $"{commandName} (namespace: {commandNamespace}) and {handlerName} (namespace: {handlerNamespace}) should be in the same namespace.");
    }

    [Theory]
    [MemberData(nameof(QueryHandlerPairs))]
    public void Query_And_Handler_ShouldBeInSameNamespace(
        string queryName, string handlerName, string queryNamespace, string handlerNamespace)
    {
        Assert.True(
            queryNamespace == handlerNamespace,
            $"{queryName} (namespace: {queryNamespace}) and {handlerName} (namespace: {handlerNamespace}) should be in the same namespace.");
    }
}
