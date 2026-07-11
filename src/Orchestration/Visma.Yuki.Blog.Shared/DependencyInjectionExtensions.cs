using Visma.Yuki.Blog.Application.UseCases;
using Visma.Yuki.Blog.Application.Ports;
using Visma.Yuki.Blog.Infrastructure.Repositories;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System.Data;
using Visma.Yuki.Blog.Application.Ports.Driving;
using Visma.Yuki.Blog.Application.Ports.Driven;

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

    public static TBuilder AddDatabaseDependencies<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.AddNpgsqlDataSource("yuki-blog-database");

        builder.Services.AddTransient<IDbConnection>(sp =>
        {
            var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
            return dataSource.CreateConnection();
        });

        builder.Services.AddHealthChecks();

        return builder;
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
