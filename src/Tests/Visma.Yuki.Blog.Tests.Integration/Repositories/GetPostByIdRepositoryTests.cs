using Npgsql;
using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Domain.ValueObjects;
using Visma.Yuki.Blog.Infrastructure.Repositories;
using Visma.Yuki.Blog.Tests.Integration.Infrastructure;

namespace Visma.Yuki.Blog.Tests.Integration.Repositories;

public class GetPostByIdRepositoryTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly string _connectionString;
    private NpgsqlDataSource _dataSource = null!;
    private UnitOfWork _unitOfWork = null!;
    private PostRepository _repository = null!;

    public GetPostByIdRepositoryTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _connectionString = factory.ConnectionString;
    }

    [Fact]
    public async Task GetPostByIdAsync_WhenPostExists_ShouldReturnPost()
    {
        var author = await InsertAuthorAsync("John", "Doe");
        var postId = await InsertPostAsync("My Post", "Desc", "Content", author.Id);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetPostByIdAsync(postId, false, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        Assert.NotNull(result);
        Assert.Equal(postId, result!.Id);
        Assert.Equal("My Post", result.Title);
    }

    [Fact]
    public async Task GetPostByIdAsync_WhenPostDoesNotExist_ShouldReturnNull()
    {
        var randomId = Guid.NewGuid();

        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetPostByIdAsync(randomId, false, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPostByIdAsync_WithoutIncludeAuthor_ShouldReturnPostWithoutAuthor()
    {
        var author = await InsertAuthorAsync("John", "Doe");
        var postId = await InsertPostAsync("Post", "Desc", "Content", author.Id);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetPostByIdAsync(postId, false, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        Assert.NotNull(result);
        Assert.Null(result!.Author);
    }

    [Fact]
    public async Task GetPostByIdAsync_WithIncludeAuthor_ShouldReturnPostWithAuthor()
    {
        var author = await InsertAuthorAsync("Jane", "Smith");
        var postId = await InsertPostAsync("Post", "Desc", "Content", author.Id);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetPostByIdAsync(postId, true, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        Assert.NotNull(result);
        Assert.NotNull(result!.Author);
        Assert.Equal("Jane", result.Author.Name);
        Assert.Equal("Smith", result.Author.Surname);
    }

    [Fact]
    public async Task GetPostByIdAsync_ShouldMapAllFieldsCorrectly()
    {
        var author = await InsertAuthorAsync("Full", "Fields");
        var postId = await InsertPostAsync("Full Title", "Full Description", "Full Content", author.Id);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetPostByIdAsync(postId, false, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        Assert.NotNull(result);
        Assert.Equal(postId, result!.Id);
        Assert.Equal("Full Title", result.Title);
        Assert.Equal("Full Description", result.Description);
        Assert.Equal("Full Content", result.Content);
        Assert.Equal(author.Id, result.AuthorId);
    }

    [Fact]
    public async Task GetPostByIdAsync_WithIncludeAuthor_ShouldMapAllFieldsIncludingAuthor()
    {
        var author = await InsertAuthorAsync("Mapped", "Author");
        var postId = await InsertPostAsync("Mapped Post", "Mapped Desc", "Mapped Content", author.Id);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetPostByIdAsync(postId, true, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        Assert.NotNull(result);
        Assert.Equal(postId, result!.Id);
        Assert.Equal("Mapped Post", result.Title);
        Assert.Equal("Mapped Desc", result.Description);
        Assert.Equal("Mapped Content", result.Content);
        Assert.Equal(author.Id, result.AuthorId);
        Assert.NotNull(result.Author);
        Assert.Equal("Mapped", result.Author.Name);
        Assert.Equal("Author", result.Author.Surname);
        Assert.NotEmpty(result.Author.UniqueNameIdentifier.Value);
    }

    [Fact]
    public async Task GetPostByIdAsync_WithNonExistentIdAndIncludeAuthor_ShouldReturnNull()
    {
        var randomId = Guid.NewGuid();

        await _unitOfWork.BeginTransactionAsync();
        var result = await _repository.GetPostByIdAsync(randomId, true, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        Assert.Null(result);
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
