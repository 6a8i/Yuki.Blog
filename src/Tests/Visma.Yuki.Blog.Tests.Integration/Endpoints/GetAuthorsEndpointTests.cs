using System.Net;
using System.Net.Http.Json;
using Npgsql;
using Visma.Yuki.Blog.Tests.Integration.Infrastructure;

namespace Visma.Yuki.Blog.Tests.Integration.Endpoints;

public class GetAuthorsEndpointTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly string _connectionString;

    public GetAuthorsEndpointTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _connectionString = factory.ConnectionString;
    }

    [Fact]
    public async Task GetAuthors_WhenTableHasData_ShouldReturn200WithAuthors()
    {
        var response = await _client.GetAsync("/api/v1/authors/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var authors = await response.Content.ReadFromJsonAsync<List<AuthorDto>>();
        Assert.NotNull(authors);
        Assert.Equal(2, authors.Count);
        Assert.Contains(authors, a => a.FullName == "John Doe");
        Assert.Contains(authors, a => a.FullName == "Jane Smith");
    }

    [Fact]
    public async Task GetAuthors_WhenTableIsEmpty_ShouldReturn204NoContent()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "TRUNCATE TABLE posts, authors CASCADE;";
        await cmd.ExecuteNonQueryAsync();

        var response = await _client.GetAsync("/api/v1/authors/");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetAuthors_ShouldReturnCorrectContentType()
    {
        var response = await _client.GetAsync("/api/v1/authors/");

        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetAuthors_WithMultipleAuthors_ShouldReturnAllAuthors()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO authors (id, uniquenameidentifier, name, surname)
            VALUES
            (gen_random_uuid(), 'aaa111222333444555666777888999aaabbbcccdddeeefff11', 'Test', 'User1'),
            (gen_random_uuid(), 'bbb111222333444555666777888999aaabbbcccdddeeefff22', 'Test', 'User2'),
            (gen_random_uuid(), 'ccc111222333444555666777888999aaabbbcccdddeeefff33', 'Test', 'User3');
            """;
        await cmd.ExecuteNonQueryAsync();

        var response = await _client.GetAsync("/api/v1/authors/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var authors = await response.Content.ReadFromJsonAsync<List<AuthorDto>>();
        Assert.NotNull(authors);
        Assert.True(authors.Count >= 5);
    }

    public async Task InitializeAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "TRUNCATE TABLE posts, authors CASCADE;";
        await cmd.ExecuteNonQueryAsync();

        cmd.CommandText = """
            INSERT INTO authors (id, uniquenameidentifier, name, surname)
            VALUES
            (gen_random_uuid(), '9bc70138988276f57849e7b4588523b092f6da1c6e1ca87869', 'John', 'Doe'),
            (gen_random_uuid(), '8fb1b1516f1a8cc3f5e5b3f2ec20fa52b4742718fae471fa28', 'Jane', 'Smith');
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private record AuthorDto(Guid Id, string FullName);
}
