using System.Reflection;
using NetArchTest.Rules;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Tests.Architecture.Layers;

public class DomainPurityTests
{
    private static readonly Assembly DomainAssembly = typeof(Author).Assembly;

    private static readonly string[] FrameworkAssemblyPrefixes =
    [
        "System",
        "Microsoft",
        "netstandard",
        "mscorlib"
    ];

    [Fact]
    public void Domain_ShouldNotReferenceExternalPackages()
    {
        var externalReferences = DomainAssembly
            .GetReferencedAssemblies()
            .Where(a => !FrameworkAssemblyPrefixes.Any(prefix => a.Name!.StartsWith(prefix)))
            .ToList();

        Assert.Empty(externalReferences);
    }

    [Fact]
    public void Domain_Entities_ShouldBeInEntitiesNamespace()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreClasses()
            .And()
            .ResideInNamespace("Visma.Yuki.Blog.Domain")
            .And()
            .DoNotResideInNamespace("Visma.Yuki.Blog.Domain.ValueObjects")
            .And()
            .DoNotResideInNamespace("Visma.Yuki.Blog.Domain.Enums")
            .And()
            .DoNotResideInNamespace("Visma.Yuki.Blog.Domain.Events")
            .And()
            .DoNotResideInNamespace("Visma.Yuki.Blog.Domain.Exceptions")
            .Should()
            .ResideInNamespace("Visma.Yuki.Blog.Domain.Entities")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnAnyAdapter()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Visma.Yuki.Blog.Infrastructure",
                "Visma.Yuki.Blog.Api",
                "Visma.Yuki.Blog.Shared")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Domain_ShouldNotReferenceEntityFrameworkOrDapper()
    {
        var ormReferences = DomainAssembly
            .GetReferencedAssemblies()
            .Where(a => a.Name!.Contains("EntityFramework") || a.Name!.Contains("Dapper"))
            .ToList();

        Assert.Empty(ormReferences);
    }
}
