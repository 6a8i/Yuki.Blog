using Npgsql;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Domain.ValueObjects;
using Visma.Yuki.Blog.Infrastructure.Repositories;
using Visma.Yuki.Blog.Tests.Integration.Infrastructure;

namespace Visma.Yuki.Blog.Tests.Integration.Repositories;

public class AddAuthorRepositoryTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly string _connectionString;
    private NpgsqlDataSource _dataSource = null!;
    private UnitOfWork _unitOfWork = null!;
    private AuthorRepository _repository = null!;

    public AddAuthorRepositoryTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _connectionString = factory.ConnectionString;
    }

    [Fact]
    public async Task AddAsync_WithValidAuthor_ShouldInsertAuthorInDatabase()
    {
        var author = new Author(Guid.CreateVersion7(), "New", "Author");

        await _unitOfWork.BeginTransactionAsync();
        await _repository.AddAsync(author, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name, surname, uniquenameidentifier FROM authors WHERE id = @id";
        cmd.Parameters.AddWithValue("id", author.Id);
        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        Assert.Equal("New", reader.GetString(0));
        Assert.Equal("Author", reader.GetString(1));
        Assert.Equal(author.UniqueNameIdentifier.Value, reader.GetString(2));
    }

    [Fact]
    public async Task AddAsync_WithValidAuthor_ShouldGenerateUniqueNameIdentifier()
    {
        var author = new Author(Guid.CreateVersion7(), "Jane", "Smith");

        await _unitOfWork.BeginTransactionAsync();
        await _repository.AddAsync(author, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        Assert.NotEmpty(author.UniqueNameIdentifier.Value);
        Assert.Equal(50, author.UniqueNameIdentifier.Value.Length);
    }

    [Fact]
    public async Task AddAsync_WithDuplicateUniqueNameIdentifier_ShouldThrowException()
    {
        var author1 = new Author(Guid.CreateVersion7(), "John", "Doe");
        var author2 = new Author(Guid.CreateVersion7(), "John", "Doe");

        await _unitOfWork.BeginTransactionAsync();
        await _repository.AddAsync(author1, CancellationToken.None);
        await _unitOfWork.CommitAsync();
        await _unitOfWork.DisposeAsync();

        _unitOfWork = new UnitOfWork(_dataSource);
        _repository = new AuthorRepository(_unitOfWork);

        await _unitOfWork.BeginTransactionAsync();

        await Assert.ThrowsAsync<PostgresException>(() => _repository.AddAsync(author2, CancellationToken.None));

        await _unitOfWork.RollbackAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldInsertAllFieldsCorrectly()
    {
        var author = new Author(Guid.CreateVersion7(), "Full", "Fields");

        await _unitOfWork.BeginTransactionAsync();
        await _repository.AddAsync(author, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, name, surname, uniquenameidentifier FROM authors WHERE name = 'Full'";
        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        Assert.Equal(author.Id, reader.GetGuid(0));
        Assert.Equal("Full", reader.GetString(1));
        Assert.Equal("Fields", reader.GetString(2));
        Assert.Equal(author.UniqueNameIdentifier.Value, reader.GetString(3));
    }

    [Fact]
    public async Task GetByUniqueNameIdentifierAsync_WhenAuthorExists_ShouldReturnAuthor()
    {
        var author = new Author(Guid.CreateVersion7(), "Unique", "Check");
        var identifier = author.UniqueNameIdentifier;

        await _unitOfWork.BeginTransactionAsync();
        await _repository.AddAsync(author, CancellationToken.None);
        await _unitOfWork.CommitAsync();

        var result = await _repository.GetByUniqueNameIdentifierAsync(identifier, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(author.Id, result.Id);
        Assert.Equal("Unique", result.Name);
        Assert.Equal("Check", result.Surname);
    }

    [Fact]
    public async Task GetByUniqueNameIdentifierAsync_WhenAuthorDoesNotExist_ShouldReturnNull()
    {
        var identifier = UniqueNameIdentifier.Create("Nonexistent", "Author");

        var result = await _repository.GetByUniqueNameIdentifierAsync(identifier, CancellationToken.None);

        Assert.Null(result);
    }

    public async Task InitializeAsync()
    {
        _dataSource = new NpgsqlDataSourceBuilder(_connectionString).Build();
        _unitOfWork = new UnitOfWork(_dataSource);
        _repository = new AuthorRepository(_unitOfWork);

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
