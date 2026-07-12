using System.Net;
using System.Net.Http.Json;
using Npgsql;
using Visma.Yuki.Blog.Tests.Integration.Infrastructure;

namespace Visma.Yuki.Blog.Tests.Integration.Endpoints;

public class CreateAuthorEndpointTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly string _connectionString;

    public CreateAuthorEndpointTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _connectionString = factory.ConnectionString;
    }

    [Fact]
    public async Task CreateAuthor_WithValidData_ShouldReturn201Created()
    {
        var request = new { Name = "New", Surname = "Author" };

        var response = await _client.PostAsJsonAsync("/api/v1/authors/", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var author = await response.Content.ReadFromJsonAsync<AuthorDto>();
        Assert.NotNull(author);
        Assert.Equal("New Author", author.FullName);
        Assert.NotEqual(Guid.Empty, author.Id);
    }

    [Fact]
    public async Task CreateAuthor_WithValidData_ShouldReturnLocationHeader()
    {
        var request = new { Name = "Located", Surname = "Author" };

        var response = await _client.PostAsJsonAsync("/api/v1/authors/", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/v1/authors/", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task CreateAuthor_WithEmptyName_ShouldReturn400BadRequest()
    {
        var request = new { Name = "", Surname = "Author" };

        var response = await _client.PostAsJsonAsync("/api/v1/authors/", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuthor_WithEmptySurname_ShouldReturn400BadRequest()
    {
        var request = new { Name = "John", Surname = "" };

        var response = await _client.PostAsJsonAsync("/api/v1/authors/", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuthor_WithDuplicateNameAndSurname_ShouldReturn400BadRequest()
    {
        var identifier = Visma.Yuki.Blog.Domain.ValueObjects.UniqueNameIdentifier.Create("John", "Doe");

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO authors (id, uniquenameidentifier, name, surname)
            VALUES (gen_random_uuid(), @identifier, 'John', 'Doe')
            """;
        cmd.Parameters.AddWithValue("identifier", identifier.Value);
        await cmd.ExecuteNonQueryAsync();

        var request = new { Name = "John", Surname = "Doe" };

        var response = await _client.PostAsJsonAsync("/api/v1/authors/", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuthor_WithValidData_ShouldPersistAuthorInDatabase()
    {
        var request = new { Name = "Persisted", Surname = "Author" };

        var response = await _client.PostAsJsonAsync("/api/v1/authors/", request);
        var author = await response.Content.ReadFromJsonAsync<AuthorDto>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name, surname FROM authors WHERE id = @id";
        cmd.Parameters.AddWithValue("id", author!.Id);
        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        Assert.Equal("Persisted", reader.GetString(0));
        Assert.Equal("Author", reader.GetString(1));
    }

    public async Task InitializeAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "TRUNCATE TABLE authors;";
        await cmd.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private record AuthorDto(Guid Id, string FullName);
}
