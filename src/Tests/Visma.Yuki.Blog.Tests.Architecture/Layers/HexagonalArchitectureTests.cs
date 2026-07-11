using NetArchTest.Rules;
using Visma.Yuki.Blog.Application.Ports.Driving;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Application.UseCases;
using Visma.Yuki.Blog.Infrastructure.Repositories;
using Visma.Yuki.Blog.Api.Endpoints.V1;

namespace Visma.Yuki.Blog.Tests.Architecture.Layers;

public class HexagonalArchitectureTests
{
    [Fact]
    public void DrivingPorts_ShouldBeInterfaces_InApplicationLayer()
    {
        var result = Types.InAssembly(typeof(IAuthorUseCase).Assembly)
            .That()
            .ResideInNamespace("Visma.Yuki.Blog.Application.Ports.Driving")
            .Should()
            .BeInterfaces()
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void DrivenPorts_ShouldBeInterfaces_InApplicationLayer()
    {
        var result = Types.InAssembly(typeof(IAuthorPorts).Assembly)
            .That()
            .ResideInNamespace("Visma.Yuki.Blog.Application.Ports.Driven")
            .Should()
            .BeInterfaces()
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void UseCases_ShouldImplementDrivingPorts()
    {
        var result = Types.InAssembly(typeof(AuthorUseCase).Assembly)
            .That()
            .ResideInNamespace("Visma.Yuki.Blog.Application.UseCases")
            .Should()
            .ImplementInterface(typeof(IAuthorUseCase))
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Infrastructure_ShouldImplementDrivenPorts()
    {
        var infrastructureTypes = Types.InAssembly(typeof(AuthorRepository).Assembly)
            .That()
            .ResideInNamespace("Visma.Yuki.Blog.Infrastructure")
            .GetTypes();

        var drivenPortTypes = new[]
        {
            typeof(IAuthorPorts),
            typeof(IUnitOfWork)
        };

        foreach (var portType in drivenPortTypes)
        {
            var hasImplementation = infrastructureTypes
                .Any(t => portType.IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false });

            Assert.True(hasImplementation,
                $"No concrete implementation found in Infrastructure for driven port {portType.Name}");
        }
    }

    [Fact]
    public void Api_ShouldDependOnDrivingPorts_NotConcreteUseCases()
    {
        var apiTypes = Types.InAssembly(typeof(AuthorEndpoints).Assembly)
            .That()
            .ResideInNamespace("Visma.Yuki.Blog.Api")
            .GetTypes();

        foreach (var type in apiTypes)
        {
            var usedTypes = type.GetMethods()
                .SelectMany(m => m.GetParameters())
                .Select(p => p.ParameterType)
                .Concat(type.GetFields().Select(f => f.FieldType))
                .Distinct();

            var dependsOnConcreteUseCase = usedTypes
                .Any(t => t == typeof(AuthorUseCase));

            Assert.False(dependsOnConcreteUseCase,
                $"{type.FullName} depends on concrete AuthorUseCase instead of IAuthorUseCase");
        }
    }

    [Fact]
    public void Domain_ShouldNotDependOnApplicationPorts()
    {
        var result = Types.InAssembly(typeof(Visma.Yuki.Blog.Domain.Entities.Author).Assembly)
            .Should()
            .NotHaveDependencyOnAny("Visma.Yuki.Blog.Application")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}
