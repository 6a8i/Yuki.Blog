using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Testcontainers.PostgreSql;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Infrastructure.Repositories;

namespace Visma.Yuki.Blog.Tests.Integration.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("yuki-blog-test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<NpgsqlDataSource>();

            var dataSource = new NpgsqlDataSourceBuilder(ConnectionString).Build();
            services.AddSingleton<NpgsqlDataSource>(dataSource);

            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(sp => new UnitOfWork(sp.GetRequiredService<NpgsqlDataSource>()));
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS authors (
                id UUID PRIMARY KEY,
                uniquenameidentifier VARCHAR(50) NOT NULL UNIQUE,
                name VARCHAR(150) NOT NULL,
                surname VARCHAR(150) NOT NULL
            );
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
