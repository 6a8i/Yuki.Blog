using System.Reflection;
using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Application.UseCases;
using Visma.Yuki.Blog.Infrastructure.Repositories;
using Visma.Yuki.Blog.Api.Endpoints.V1;

namespace Visma.Yuki.Blog.Tests.Architecture.Layers;

public class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(Author).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(AuthorCommandHandler).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(AuthorRepository).Assembly;
    private static readonly Assembly ApiAssembly = typeof(AuthorEndpoints).Assembly;

    private static bool AssemblyReferences(Assembly assembly, string targetAssemblyName)
    {
        return assembly.GetReferencedAssemblies()
            .Any(a => a.Name == targetAssemblyName);
    }

    [Fact]
    public void Domain_ShouldNotReferenceAnyOtherLayer()
    {
        var forbiddenReferences = new[]
        {
            "Visma.Yuki.Blog.Application",
            "Visma.Yuki.Blog.Infrastructure",
            "Visma.Yuki.Blog.Api",
            "Visma.Yuki.Blog.Shared"
        };

        var violatingReferences = forbiddenReferences
            .Where(name => AssemblyReferences(DomainAssembly, name))
            .ToList();

        Assert.Empty(violatingReferences);
    }

    [Fact]
    public void Application_ShouldOnlyReferenceDomain()
    {
        var forbiddenReferences = new[]
        {
            "Visma.Yuki.Blog.Infrastructure",
            "Visma.Yuki.Blog.Api",
            "Visma.Yuki.Blog.Shared"
        };

        var violatingReferences = forbiddenReferences
            .Where(name => AssemblyReferences(ApplicationAssembly, name))
            .ToList();

        Assert.Empty(violatingReferences);
    }

    [Fact]
    public void Application_ShouldReferenceDomain()
    {
        Assert.True(AssemblyReferences(ApplicationAssembly, "Visma.Yuki.Blog.Domain"));
    }

    [Fact]
    public void Infrastructure_ShouldNotReferenceApiOrShared()
    {
        var forbiddenReferences = new[]
        {
            "Visma.Yuki.Blog.Api",
            "Visma.Yuki.Blog.Shared"
        };

        var violatingReferences = forbiddenReferences
            .Where(name => AssemblyReferences(InfrastructureAssembly, name))
            .ToList();

        Assert.Empty(violatingReferences);
    }

    [Fact]
    public void Infrastructure_ShouldReferenceDomainAndApplication()
    {
        Assert.True(AssemblyReferences(InfrastructureAssembly, "Visma.Yuki.Blog.Domain"));
        Assert.True(AssemblyReferences(InfrastructureAssembly, "Visma.Yuki.Blog.Application"));
    }

    [Fact]
    public void Api_ShouldNotReferenceInfrastructureDirectly()
    {
        Assert.False(AssemblyReferences(ApiAssembly, "Visma.Yuki.Blog.Infrastructure"));
    }

    [Fact]
    public void Api_ShouldReferenceApplication()
    {
        Assert.True(AssemblyReferences(ApiAssembly, "Visma.Yuki.Blog.Application"));
    }
}
