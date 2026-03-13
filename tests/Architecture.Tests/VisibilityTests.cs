using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Architecture.Tests;

public class VisibilityTests
{
    private static readonly System.Reflection.Assembly ApplicationAssembly =
        typeof(TeacherSuite.Application.Teachers.Commands.Create.CreateTeacherCommand).Assembly;

    private static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader()
            .LoadAssemblies(ApplicationAssembly)
            .Build();

    [Fact]
    public void CommandHandlers_ShouldBeInternal()
    {
        Classes().That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .And().ResideInNamespaceMatching(@".*\.Commands(\..*)?")
            .Should().NotBePublic()
            .Check(Architecture);
    }

    [Fact]
    public void CommandHandlers_ShouldBeSealed()
    {
        Classes().That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .And().ResideInNamespaceMatching(@".*\.Commands(\..*)?")
            .Should().BeSealed()
            .Check(Architecture);
    }

    [Fact]
    public void QueryHandlers_ShouldBeInternal()
    {
        Classes().That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .And().ResideInNamespaceMatching(@".*\.Queries(\..*)?")
            .Should().NotBePublic()
            .Check(Architecture);
    }

    [Fact]
    public void QueryHandlers_ShouldBeSealed()
    {
        Classes().That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .And().ResideInNamespaceMatching(@".*\.Queries(\..*)?")
            .Should().BeSealed()
            .Check(Architecture);
    }
}
