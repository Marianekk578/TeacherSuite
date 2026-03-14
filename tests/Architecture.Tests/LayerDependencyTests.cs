using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Assembly = System.Reflection.Assembly;

namespace Architecture.Tests;

public class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = Assembly.Load("TeacherSuite.Domain");
    private static readonly Assembly ApplicationAssembly = Assembly.Load("TeacherSuite.Application");
    private static readonly Assembly InfrastructureAssembly = Assembly.Load("TeacherSuite.Infrastructure");
    private static readonly Assembly WebAssembly = Assembly.Load("TeacherSuite.Web");

    private static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader()
            .LoadAssemblies(DomainAssembly, ApplicationAssembly, InfrastructureAssembly, WebAssembly)
            .Build();

    private static readonly IObjectProvider<IType> DomainLayer =
        Types().That().ResideInAssembly(DomainAssembly).As("Domain Layer");

    private static readonly IObjectProvider<IType> ApplicationLayer =
        Types().That().ResideInAssembly(ApplicationAssembly).As("Application Layer");

    private static readonly IObjectProvider<IType> InfrastructureLayer =
        Types().That().ResideInAssembly(InfrastructureAssembly).As("Infrastructure Layer");

    private static readonly IObjectProvider<IType> WebLayer =
        Types().That().ResideInAssembly(WebAssembly).As("Web Layer");

    [Fact]
    public void Domain_ShouldNotDependOn_Application()
    {
        Types().That().Are(DomainLayer)
            .Should().NotDependOnAny(ApplicationLayer)
            .Check(Architecture);
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        Types().That().Are(DomainLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .Check(Architecture);
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Web()
    {
        Types().That().Are(DomainLayer)
            .Should().NotDependOnAny(WebLayer)
            .Check(Architecture);
    }

    [Fact]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        Types().That().Are(ApplicationLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .Check(Architecture);
    }

    [Fact]
    public void Application_ShouldNotDependOn_Web()
    {
        Types().That().Are(ApplicationLayer)
            .Should().NotDependOnAny(WebLayer)
            .Check(Architecture);
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOn_Web()
    {
        Types().That().Are(InfrastructureLayer)
            .Should().NotDependOnAny(WebLayer)
            .Check(Architecture);
    }
}
