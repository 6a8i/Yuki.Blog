using System.Net;
using System.Net.Http.Json;
using Npgsql;
using Visma.Yuki.Blog.Domain.ValueObjects;
using Visma.Yuki.Blog.Tests.Integration.Infrastructure;

namespace Visma.Yuki.Blog.Tests.Integration.Endpoints;

public class GetPostByIdEndpointTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly string _connectionString;

    public GetPostByIdEndpointTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _connectionString = factory.ConnectionString;
    }

    [Fact]
    public async Task GetPostById_WhenPostExists_ShouldReturn200WithPost()
    {
        var (authorId, _) = await InsertAuthorAsync("John", "Doe");
        var postId = await InsertPostAsync("My Post", "Desc", "Content", authorId);

        var response = await _client.GetAsync($"/api/v1/posts/{postId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var post = await response.Content.ReadFromJsonAsync<PostDto>();
        Assert.NotNull(post);
        Assert.Equal(postId, post.Id);
        Assert.Equal("My Post", post.Title);
    }

    [Fact]
    public async Task GetPostById_WhenPostDoesNotExist_ShouldReturn404NotFound()
    {
        var randomId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1/posts/{randomId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPostById_WithoutIncludeAuthor_ShouldNotReturnAuthorInfo()
    {
        var (authorId, _) = await InsertAuthorAsync("John", "Doe");
        var postId = await InsertPostAsync("Post", "Desc", "Content", authorId);

        var response = await _client.GetAsync($"/api/v1/posts/{postId}");
        var post = await response.Content.ReadFromJsonAsync<PostDto>();

        Assert.NotNull(post);
        Assert.Null(post.AuthorInfo);
    }

    [Fact]
    public async Task GetPostById_WithIncludeAuthorTrue_ShouldReturnAuthorInfo()
    {
        var (authorId, _) = await InsertAuthorAsync("Jane", "Smith");
        var postId = await InsertPostAsync("Post", "Desc", "Content", authorId);

        var response = await _client.GetAsync($"/api/v1/posts/{postId}?includeAuthor=true");
        var post = await response.Content.ReadFromJsonAsync<PostDto>();

        Assert.NotNull(post);
        Assert.NotNull(post.AuthorInfo);
        Assert.Equal("Jane Smith", post.AuthorInfo.FullName);
    }

    [Fact]
    public async Task GetPostById_WithIncludeAuthorFalse_ShouldNotReturnAuthorInfo()
    {
        var (authorId, _) = await InsertAuthorAsync("John", "Doe");
        var postId = await InsertPostAsync("Post", "Desc", "Content", authorId);

        var response = await _client.GetAsync($"/api/v1/posts/{postId}?includeAuthor=false");
        var post = await response.Content.ReadFromJsonAsync<PostDto>();

        Assert.NotNull(post);
        Assert.Null(post.AuthorInfo);
    }

    [Fact]
    public async Task GetPostById_ShouldReturnCorrectPostFields()
    {
        var (authorId, _) = await InsertAuthorAsync("John", "Doe");
        var postId = await InsertPostAsync("Full Title", "Full Description", "Full Content", authorId);

        var response = await _client.GetAsync($"/api/v1/posts/{postId}");
        var post = await response.Content.ReadFromJsonAsync<PostDto>();

        Assert.NotNull(post);
        Assert.Equal(postId, post.Id);
        Assert.Equal("Full Title", post.Title);
        Assert.Equal("Full Description", post.Description);
        Assert.Equal("Full Content", post.Content);
        Assert.Equal(authorId, post.AuthorId);
    }

    [Fact]
    public async Task GetPostById_WithInvalidGuid_ShouldReturn400BadRequest()
    {
        var response = await _client.GetAsync("/api/v1/posts/not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<(Guid Id, string Identifier)> InsertAuthorAsync(string name, string surname)
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

        return (id, identifier.Value);
    }

    private async Task<Guid> InsertPostAsync(string title, string? description, string content, Guid authorId)
    {
        var id = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO posts (id, title, description, content, authorId) VALUES (@id, @title, @description, @content, @authorId)";
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("title", title);
        cmd.Parameters.AddWithValue("description", (object?)description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("content", content);
        cmd.Parameters.AddWithValue("authorId", authorId);
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

    private record PostDto(Guid Id, string Title, string? Description, string Content, Guid AuthorId, AuthorDto? AuthorInfo);
    private record AuthorDto(Guid Id, string FullName);
}
