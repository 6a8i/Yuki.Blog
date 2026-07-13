using System.Net;
using System.Net.Http.Json;
using Npgsql;
using Visma.Yuki.Blog.Domain.ValueObjects;
using Visma.Yuki.Blog.Tests.Integration.Infrastructure;

namespace Visma.Yuki.Blog.Tests.Integration.Endpoints;

public class CreatePostEndpointTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly string _connectionString;

    public CreatePostEndpointTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _connectionString = factory.ConnectionString;
    }

    [Fact]
    public async Task CreatePost_WithValidDataAndAuthorId_ShouldReturn201Created()
    {
        var authorId = await InsertAuthorAsync("John", "Doe");

        var request = new { Title = "My Post", Description = "A description", Content = "Content body", AuthorId = authorId };

        var response = await _client.PostAsJsonAsync("/api/v1/posts/", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var post = await response.Content.ReadFromJsonAsync<PostDto>();
        Assert.NotNull(post);
        Assert.NotEqual(Guid.Empty, post.Id);
        Assert.NotEmpty(post.Links);
    }

    [Fact]
    public async Task CreatePost_WithValidDataAndAuthorName_ShouldReturn201Created()
    {
        await InsertAuthorAsync("Jane", "Smith");

        var request = new { Title = "New Post", Description = (string?)null, Content = "Content body", AuthorName = "Jane", AuthorSurname = "Smith" };

        var response = await _client.PostAsJsonAsync("/api/v1/posts/", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_WithValidDataAndNewAuthorName_ShouldReturn201Created()
    {
        var request = new { Title = "Post With New Author", Description = (string?)null, Content = "Content body", AuthorName = "New", AuthorSurname = "Author" };

        var response = await _client.PostAsJsonAsync("/api/v1/posts/", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnLocationHeader()
    {
        var authorId = await InsertAuthorAsync("Located", "Author");

        var request = new { Title = "Located Post", Description = (string?)null, Content = "Content", AuthorId = authorId };

        var response = await _client.PostAsJsonAsync("/api/v1/posts/", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/v1/posts/", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task CreatePost_WithEmptyTitle_ShouldReturn400BadRequest()
    {
        var authorId = await InsertAuthorAsync("John", "Doe");

        var request = new { Title = "", Description = (string?)null, Content = "Content", AuthorId = authorId };

        var response = await _client.PostAsJsonAsync("/api/v1/posts/", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_WithEmptyContent_ShouldReturn400BadRequest()
    {
        var authorId = await InsertAuthorAsync("John", "Doe");

        var request = new { Title = "My Post", Description = (string?)null, Content = "", AuthorId = authorId };

        var response = await _client.PostAsJsonAsync("/api/v1/posts/", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_WithNoAuthorIdentification_ShouldReturn400BadRequest()
    {
        var request = new { Title = "My Post", Description = (string?)null, Content = "Content", AuthorId = (Guid?)null, AuthorName = (string?)null, AuthorSurname = (string?)null };

        var response = await _client.PostAsJsonAsync("/api/v1/posts/", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_WithNonExistentAuthorId_ShouldReturn400BadRequest()
    {
        var request = new { Title = "My Post", Description = (string?)null, Content = "Content", AuthorId = Guid.NewGuid() };

        var response = await _client.PostAsJsonAsync("/api/v1/posts/", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_WithValidData_ShouldPersistPostInDatabase()
    {
        var authorId = await InsertAuthorAsync("Persisted", "Author");

        var request = new { Title = "Persisted Post", Description = "Persisted description", Content = "Persisted content", AuthorId = authorId };

        var response = await _client.PostAsJsonAsync("/api/v1/posts/", request);
        var post = await response.Content.ReadFromJsonAsync<PostDto>();
        var postId = post!.Id;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT title, description, content, authorId FROM posts WHERE id = @id";
        cmd.Parameters.AddWithValue("id", postId);
        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        Assert.Equal("Persisted Post", reader.GetString(0));
        Assert.Equal("Persisted description", reader.GetString(1));
        Assert.Equal("Persisted content", reader.GetString(2));
        Assert.Equal(authorId, reader.GetGuid(3));
    }

    [Fact]
    public async Task CreatePost_WithNewAuthorName_ShouldCreateAuthorInDatabase()
    {
        var request = new { Title = "Post With Brand New Author", Description = (string?)null, Content = "Content", AuthorName = "Brand", AuthorSurname = "New" };

        await _client.PostAsJsonAsync("/api/v1/posts/", request);

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM authors WHERE name = 'Brand' AND surname = 'New'";
        var count = (long)(await cmd.ExecuteScalarAsync())!;

        Assert.Equal(1, count);
    }

    private async Task<Guid> InsertAuthorAsync(string name, string surname)
    {
        var identifier = UniqueNameIdentifier.Create(name, surname);
        var id = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO authors (id, uniquenameidentifier, name, surname) VALUES (@id, @identifier, @name, @surname)";
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("identifier", identifier.Value);
        cmd.Parameters.AddWithValue("name", name);
        cmd.Parameters.AddWithValue("surname", surname);
        await cmd.ExecuteNonQueryAsync();

        return id;
    }

    public async Task InitializeAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "TRUNCATE TABLE posts, authors CASCADE;";
        await cmd.ExecuteNonQueryAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private record PostDto(Guid Id, string Title, string? Description, string Content, Guid AuthorId, AuthorDto? AuthorInfo, List<LinkDto> Links);
    private record AuthorDto(Guid Id, string FullName);
    private record LinkDto(string Rel, string Method, string Href);
}
