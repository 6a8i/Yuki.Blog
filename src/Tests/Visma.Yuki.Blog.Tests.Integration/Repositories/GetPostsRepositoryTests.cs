using Npgsql;
using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Domain.ValueObjects;
using Visma.Yuki.Blog.Infrastructure.Repositories;
using Visma.Yuki.Blog.Tests.Integration.Infrastructure;

namespace Visma.Yuki.Blog.Tests.Integration.Repositories;

public class GetPostsRepositoryTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly string _connectionString;
    private NpgsqlDataSource _dataSource = null!;
    private UnitOfWork _unitOfWork = null!;
    private PostRepository _repository = null!;

    public GetPostsRepositoryTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _connectionString = factory.ConnectionString;
    }

    [Fact]
    public async Task GetAllAsync_WithoutIncludeAuthor_ShouldReturnPostsWithoutAuthor()
    {
        var author = await InsertAuthorAsync("John", "Doe");
        await InsertPostAsync("Post 1", "Desc 1", "Content 1", author.Id);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetAllAsync(false, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        var posts = result.ToList();
        Assert.Single(posts);
        Assert.Equal("Post 1", posts[0].Title);
        Assert.Null(posts[0].Author);
    }

    [Fact]
    public async Task GetAllAsync_WithIncludeAuthor_ShouldReturnPostsWithAuthor()
    {
        var author = await InsertAuthorAsync("Jane", "Smith");
        await InsertPostAsync("Post with Author", "Desc", "Content", author.Id);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetAllAsync(true, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        var posts = result.ToList();
        Assert.Single(posts);
        Assert.NotNull(posts[0].Author);
        Assert.Equal("Jane", posts[0].Author.Name);
        Assert.Equal("Smith", posts[0].Author.Surname);
    }

    [Fact]
    public async Task GetAllAsync_WhenTableIsEmpty_ShouldReturnEmptyCollection()
    {
        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetAllAsync(false, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WithMultiplePosts_ShouldReturnAllPosts()
    {
        var author = await InsertAuthorAsync("John", "Doe");
        await InsertPostAsync("Post A", null, "Content A", author.Id);
        await InsertPostAsync("Post B", null, "Content B", author.Id);
        await InsertPostAsync("Post C", null, "Content C", author.Id);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetAllAsync(false, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_WithIncludeAuthor_ShouldMapAllFieldsCorrectly()
    {
        var author = await InsertAuthorAsync("Full", "Fields");
        await InsertPostAsync("Full Post", "Full Description", "Full Content", author.Id);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetAllAsync(true, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        var post = result.Single(p => p.Title == "Full Post");
        Assert.NotEqual(Guid.Empty, post.Id);
        Assert.Equal("Full Post", post.Title);
        Assert.Equal("Full Description", post.Description);
        Assert.Equal("Full Content", post.Content);
        Assert.Equal(author.Id, post.AuthorId);
        Assert.NotNull(post.Author);
        Assert.Equal("Full", post.Author.Name);
        Assert.Equal("Fields", post.Author.Surname);
        Assert.NotEmpty(post.Author.UniqueNameIdentifier.Value);
    }

    [Fact]
    public async Task GetAllAsync_WithoutIncludeAuthor_ShouldMapAllPostFields()
    {
        var author = await InsertAuthorAsync("John", "Doe");
        await InsertPostAsync("Mapped Post", "Mapped Desc", "Mapped Content", author.Id);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetAllAsync(false, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        var post = result.Single(p => p.Title == "Mapped Post");
        Assert.NotEqual(Guid.Empty, post.Id);
        Assert.Equal("Mapped Post", post.Title);
        Assert.Equal("Mapped Desc", post.Description);
        Assert.Equal("Mapped Content", post.Content);
        Assert.Equal(author.Id, post.AuthorId);
    }

    [Fact]
    public async Task GetAllAsync_WithIncludeAuthorAndMultipleAuthors_ShouldReturnCorrectAuthorForEachPost()
    {
        var author1 = await InsertAuthorAsync("Author", "One");
        var author2 = await InsertAuthorAsync("Author", "Two");
        await InsertPostAsync("Post 1", null, "Content 1", author1.Id);
        await InsertPostAsync("Post 2", null, "Content 2", author2.Id);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetAllAsync(true, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        var posts = result.ToList();
        Assert.Equal(2, posts.Count);
        var post1 = posts.Single(p => p.Title == "Post 1");
        var post2 = posts.Single(p => p.Title == "Post 2");
        Assert.Equal("One", post1.Author!.Surname);
        Assert.Equal("Two", post2.Author!.Surname);
    }

    private async Task<Author> InsertAuthorAsync(string name, string surname)
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

        return new Author(id, name, surname, identifier.Value);
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
        _dataSource = new NpgsqlDataSourceBuilder(_connectionString).Build();
        _unitOfWork = new UnitOfWork(_dataSource);
        _repository = new PostRepository(_unitOfWork);

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "TRUNCATE TABLE posts, authors CASCADE;";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        await _unitOfWork.DisposeAsync();
        await _dataSource.DisposeAsync();
    }
}
