using System.Net;
using System.Net.Http.Json;
using Npgsql;
using Visma.Yuki.Blog.Tests.Integration.Infrastructure;

namespace Visma.Yuki.Blog.Tests.Integration.Endpoints;

public class GetAuthorByIdEndpointTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly string _connectionString;
    private Guid _authorId;

    public GetAuthorByIdEndpointTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _connectionString = factory.ConnectionString;
    }

    [Fact]
    public async Task GetAuthorById_WhenAuthorExists_ShouldReturn200WithAuthor()
    {
        var response = await _client.GetAsync($"/api/v1/authors/{_authorId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var author = await response.Content.ReadFromJsonAsync<AuthorDto>();
        Assert.NotNull(author);
        Assert.Equal(_authorId, author.Id);
        Assert.Equal("John Doe", author.FullName);
    }

    [Fact]
    public async Task GetAuthorById_WhenAuthorDoesNotExist_ShouldReturn404NotFound()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1/authors/{nonExistentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAuthorById_ShouldReturnCorrectContentType()
    {
        var response = await _client.GetAsync($"/api/v1/authors/{_authorId}");

        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    public async Task InitializeAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "TRUNCATE TABLE authors;";
        await cmd.ExecuteNonQueryAsync();

        _authorId = Guid.NewGuid();
        cmd.CommandText = """
            INSERT INTO authors (id, uniquenameidentifier, name, surname)
            VALUES (@id, '9bc70138988276f57849e7b4588523b092f6da1c6e1ca87869', 'John', 'Doe')
            """;
        cmd.Parameters.AddWithValue("id", _authorId);
        await cmd.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private record AuthorDto(Guid Id, string FullName);
}
