using NetArchTest.Rules;
using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Application.UseCases;
using Visma.Yuki.Blog.Infrastructure.Repositories;
using Visma.Yuki.Blog.Api.Endpoints.V1;

namespace Visma.Yuki.Blog.Tests.Architecture.Design;

public class NamespaceTests
{
    [Fact]
    public void Domain_Types_ShouldBeInDomainNamespace()
    {
        var result = Types.InAssembly(typeof(Author).Assembly)
            .Should()
            .ResideInNamespace("Visma.Yuki.Blog.Domain")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Application_Types_ShouldBeInApplicationNamespace()
    {
        var result = Types.InAssembly(typeof(AuthorUseCase).Assembly)
            .Should()
            .ResideInNamespace("Visma.Yuki.Blog.Application")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Infrastructure_Types_ShouldBeInInfrastructureNamespace()
    {
        var typesOutsideInfrastructureNamespace = typeof(AuthorRepository).Assembly
            .GetTypes()
            .Where(t => !string.IsNullOrEmpty(t.Namespace)
                     && !t.Namespace.StartsWith("Visma.Yuki.Blog.Infrastructure")
                     && !t.Name.StartsWith("<"))
            .Select(t => t.FullName)
            .ToList();

        Assert.Empty(typesOutsideInfrastructureNamespace);
    }

    [Fact]
    public void Api_Types_ShouldBeInApiNamespace()
    {
        var typesOutsideApiNamespace = typeof(AuthorEndpoints).Assembly
            .GetTypes()
            .Where(t => !string.IsNullOrEmpty(t.Namespace)
                     && t.Namespace.StartsWith("Visma.Yuki.Blog")
                     && !t.Namespace.StartsWith("Visma.Yuki.Blog.Api"))
            .Select(t => t.FullName)
            .ToList();

        Assert.Empty(typesOutsideApiNamespace);
    }

    [Fact]
    public void Domain_ShouldNotDependOnApplicationOrInfrastructure()
    {
        var result = Types.InAssembly(typeof(Author).Assembly)
            .Should()
            .NotHaveDependencyOnAny("Visma.Yuki.Blog.Application", "Visma.Yuki.Blog.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(typeof(AuthorUseCase).Assembly)
            .Should()
            .NotHaveDependencyOnAny("Visma.Yuki.Blog.Infrastructure", "Visma.Yuki.Blog.Api")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(typeof(AuthorRepository).Assembly)
            .Should()
            .NotHaveDependencyOnAny("Visma.Yuki.Blog.Api")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Api_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(typeof(AuthorEndpoints).Assembly)
            .Should()
            .NotHaveDependencyOnAny("Visma.Yuki.Blog.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}
