using System.Net;
using System.Net.Http.Json;
using Npgsql;
using Visma.Yuki.Blog.Domain.ValueObjects;
using Visma.Yuki.Blog.Tests.Integration.Infrastructure;

namespace Visma.Yuki.Blog.Tests.Integration.Endpoints;

public class GetPostsEndpointTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly string _connectionString;

    public GetPostsEndpointTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _connectionString = factory.ConnectionString;
    }

    [Fact]
    public async Task GetPosts_WhenTableHasData_ShouldReturn200WithPosts()
    {
        var (authorId, _) = await InsertAuthorAsync("John", "Doe");
        await InsertPostAsync("Post 1", "Desc 1", "Content 1", authorId);
        await InsertPostAsync("Post 2", "Desc 2", "Content 2", authorId);

        var response = await _client.GetAsync("/api/v1/posts/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var collection = await response.Content.ReadFromJsonAsync<PostCollectionDto>();
        Assert.NotNull(collection);
        Assert.Equal(2, collection.Items.Count);
        Assert.NotEmpty(collection.Links);
    }

    [Fact]
    public async Task GetPosts_WhenTableIsEmpty_ShouldReturn200OKWithEmptyCollection()
    {
        var response = await _client.GetAsync("/api/v1/posts/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var collection = await response.Content.ReadFromJsonAsync<PostCollectionDto>();
        Assert.NotNull(collection);
        Assert.Empty(collection.Items);
    }

    [Fact]
    public async Task GetPosts_ShouldReturnCorrectContentType()
    {
        var (authorId, _) = await InsertAuthorAsync("John", "Doe");
        await InsertPostAsync("Post 1", "Desc 1", "Content 1", authorId);

        var response = await _client.GetAsync("/api/v1/posts/");

        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetPosts_ShouldReturnLinksOnEachItem()
    {
        var (authorId, _) = await InsertAuthorAsync("John", "Doe");
        await InsertPostAsync("Post 1", "Desc 1", "Content 1", authorId);

        var response = await _client.GetAsync("/api/v1/posts/");
        var collection = await response.Content.ReadFromJsonAsync<PostCollectionDto>();

        Assert.NotNull(collection);
        Assert.All(collection.Items, p => Assert.NotEmpty(p.Links));
    }

    [Fact]
    public async Task GetPosts_WithoutIncludeAuthor_ShouldNotReturnAuthorInfo()
    {
        var (authorId, _) = await InsertAuthorAsync("John", "Doe");
        await InsertPostAsync("Post 1", "Desc 1", "Content 1", authorId);

        var response = await _client.GetAsync("/api/v1/posts/");
        var collection = await response.Content.ReadFromJsonAsync<PostCollectionDto>();

        Assert.NotNull(collection);
        Assert.All(collection.Items, p => Assert.Null(p.AuthorInfo));
    }

    [Fact]
    public async Task GetPosts_WithIncludeAuthorTrue_ShouldReturnAuthorInfo()
    {
        var (authorId, _) = await InsertAuthorAsync("John", "Doe");
        await InsertPostAsync("Post 1", "Desc 1", "Content 1", authorId);

        var response = await _client.GetAsync("/api/v1/posts/?includeAuthor=true");
        var collection = await response.Content.ReadFromJsonAsync<PostCollectionDto>();

        Assert.NotNull(collection);
        Assert.NotEmpty(collection.Items);
        Assert.NotNull(collection.Items[0].AuthorInfo);
        Assert.Equal("John Doe", collection.Items[0].AuthorInfo.FullName);
    }

    [Fact]
    public async Task GetPosts_WithIncludeAuthorFalse_ShouldNotReturnAuthorInfo()
    {
        var (authorId, _) = await InsertAuthorAsync("John", "Doe");
        await InsertPostAsync("Post 1", "Desc 1", "Content 1", authorId);

        var response = await _client.GetAsync("/api/v1/posts/?includeAuthor=false");
        var collection = await response.Content.ReadFromJsonAsync<PostCollectionDto>();

        Assert.NotNull(collection);
        Assert.All(collection.Items, p => Assert.Null(p.AuthorInfo));
    }

    [Fact]
    public async Task GetPosts_ShouldReturnCorrectPostFields()
    {
        var (authorId, _) = await InsertAuthorAsync("John", "Doe");
        await InsertPostAsync("My Title", "My Description", "My Content", authorId);

        var response = await _client.GetAsync("/api/v1/posts/");
        var collection = await response.Content.ReadFromJsonAsync<PostCollectionDto>();

        Assert.NotNull(collection);
        var post = collection.Items.Single(p => p.Title == "My Title");
        Assert.Equal("My Description", post.Description);
        Assert.Equal("My Content", post.Content);
        Assert.Equal(authorId, post.AuthorId);
    }

    [Fact]
    public async Task GetPosts_WithMultiplePosts_ShouldReturnAllPosts()
    {
        var (authorId, _) = await InsertAuthorAsync("John", "Doe");
        await InsertPostAsync("Post A", null, "Content A", authorId);
        await InsertPostAsync("Post B", null, "Content B", authorId);
        await InsertPostAsync("Post C", null, "Content C", authorId);

        var response = await _client.GetAsync("/api/v1/posts/");
        var collection = await response.Content.ReadFromJsonAsync<PostCollectionDto>();

        Assert.NotNull(collection);
        Assert.True(collection.Items.Count >= 3);
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

    private async Task InsertPostAsync(string title, string? description, string content, Guid authorId)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO posts (id, title, description, content, authorId) VALUES (@id, @title, @description, @content, @authorId)";
        cmd.Parameters.AddWithValue("id", Guid.NewGuid());
        cmd.Parameters.AddWithValue("title", title);
        cmd.Parameters.AddWithValue("description", (object?)description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("content", content);
        cmd.Parameters.AddWithValue("authorId", authorId);
        await cmd.ExecuteNonQueryAsync();
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
    private record PostCollectionDto(List<PostDto> Items, List<LinkDto> Links);
}
