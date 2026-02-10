using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;

namespace Application.UnitTests.Architecture;

public class CleanArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(TeacherSuite.Domain.Entities.Teacher).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(TeacherSuite.Application.Common.Interfaces.IApplicationDbContext).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(TeacherSuite.Infrastructure.Data.ApplicationDbContext).Assembly;
    private static readonly Assembly WebAssembly = typeof(TeacherSuite.Web.Endpoints.Teachers).Assembly;

    private const string DomainNamespace = "TeacherSuite.Domain";
    private const string ApplicationNamespace = "TeacherSuite.Application";
    private const string InfrastructureNamespace = "TeacherSuite.Infrastructure";
    private const string WebNamespace = "TeacherSuite.Web";

    [Fact]
    public void Domain_ShouldNotDependOn_Application()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Domain layer must not depend on Application layer");
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Domain layer must not depend on Infrastructure layer");
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Web()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(WebNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Domain layer must not depend on Web layer");
    }

    [Fact]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Application layer must not depend on Infrastructure layer");
    }

    [Fact]
    public void Application_ShouldNotDependOn_Web()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(WebNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Application layer must not depend on Web layer");
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOn_Web()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(WebNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Infrastructure layer must not depend on Web layer");
    }
}
