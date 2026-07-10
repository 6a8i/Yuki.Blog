using Visma.Yuki.Blog.Application.UseCases;
using Visma.Yuki.Blog.Application.Ports;
using Visma.Yuki.Blog.Domain.Ports.Repositories;
using Visma.Yuki.Blog.Infrastructure.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static TServiceCollection AddSharedServices<TServiceCollection>(this TServiceCollection services)
        where TServiceCollection : IServiceCollection
    {
        // Driving Ports
        AddUseCases(services);
        
        // Driven Ports
        AddDrivenPorts(services);
        
        return services;
    }

    private static void AddUseCases<TServiceCollection>(TServiceCollection services)
        where TServiceCollection : IServiceCollection
    {
        services.AddScoped<IAuthorUseCase, AuthorUseCase>();
    }

    private static void AddDrivenPorts<TServiceCollection>(TServiceCollection services)
        where TServiceCollection : IServiceCollection
    {
        services.AddScoped<IAuthorPorts, AuthorRepository>();
    }
}
