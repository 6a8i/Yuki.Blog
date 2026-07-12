using Npgsql;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Domain.ValueObjects;
using Visma.Yuki.Blog.Infrastructure.Repositories;
using Visma.Yuki.Blog.Tests.Integration.Infrastructure;

namespace Visma.Yuki.Blog.Tests.Integration.Repositories;

public class AddPostRepositoryTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly string _connectionString;
    private NpgsqlDataSource _dataSource = null!;
    private UnitOfWork _unitOfWork = null!;
    private PostRepository _repository = null!;

    public AddPostRepositoryTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _connectionString = factory.ConnectionString;
    }

    [Fact]
    public async Task AddAsync_WithValidPost_ShouldInsertPostInDatabase()
    {
        var author = await InsertAuthorAsync("Post", "Author");
        var post = new Post(Guid.CreateVersion7(), "Test Post", "Test Description", "Test Content", author);

        await _unitOfWork.BeginTransactionAsync();
        await _repository.AddAsync(post, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT title, description, content, authorId FROM posts WHERE id = @id";
        cmd.Parameters.AddWithValue("id", post.Id);
        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        Assert.Equal("Test Post", reader.GetString(0));
        Assert.Equal("Test Description", reader.GetString(1));
        Assert.Equal("Test Content", reader.GetString(2));
        Assert.Equal(author.Id, reader.GetGuid(3));
    }

    [Fact]
    public async Task AddAsync_WithNullDescription_ShouldInsertNullDescription()
    {
        var author = await InsertAuthorAsync("Null", "Desc");
        var post = new Post(Guid.CreateVersion7(), "No Description Post", null, "Content", author);

        await _unitOfWork.BeginTransactionAsync();
        await _repository.AddAsync(post, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT description FROM posts WHERE id = @id";
        cmd.Parameters.AddWithValue("id", post.Id);
        var result = await cmd.ExecuteScalarAsync();

        Assert.True(result is DBNull || result is null);
    }

    [Fact]
    public async Task AddAsync_ShouldInsertAllFieldsCorrectly()
    {
        var author = await InsertAuthorAsync("Full", "Fields");
        var post = new Post(Guid.CreateVersion7(), "Full Post", "Full Description", "Full Content", author);

        await _unitOfWork.BeginTransactionAsync();
        await _repository.AddAsync(post, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, title, description, content, authorId FROM posts WHERE title = 'Full Post'";
        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        Assert.Equal(post.Id, reader.GetGuid(0));
        Assert.Equal("Full Post", reader.GetString(1));
        Assert.Equal("Full Description", reader.GetString(2));
        Assert.Equal("Full Content", reader.GetString(3));
        Assert.Equal(author.Id, reader.GetGuid(4));
    }

    [Fact]
    public async Task AddAsync_WithNonExistentAuthorId_ShouldThrowException()
    {
        var fakeAuthor = new Author(Guid.NewGuid(), "Fake", "Author");
        var post = new Post(Guid.CreateVersion7(), "Orphan Post", null, "Content", fakeAuthor);

        await _unitOfWork.BeginTransactionAsync();

        await Assert.ThrowsAsync<PostgresException>(() => _repository.AddAsync(post, CancellationToken.None));

        await _unitOfWork.RollbackAsync();
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
