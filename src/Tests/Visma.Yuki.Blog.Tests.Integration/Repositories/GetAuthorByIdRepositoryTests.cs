using Npgsql;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Infrastructure.Repositories;
using Visma.Yuki.Blog.Tests.Integration.Infrastructure;

namespace Visma.Yuki.Blog.Tests.Integration.Repositories;

public class GetAuthorByIdRepositoryTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly string _connectionString;
    private NpgsqlDataSource _dataSource = null!;
    private UnitOfWork _unitOfWork = null!;
    private AuthorRepository _repository = null!;
    private Guid _authorId;

    public GetAuthorByIdRepositoryTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _connectionString = factory.ConnectionString;
    }

    [Fact]
    public async Task GetByIdAsync_WhenAuthorExists_ShouldReturnAuthor()
    {
        var result = await _repository.GetByIdAsync(_authorId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(_authorId, result.Id);
        Assert.Equal("John", result.Name);
        Assert.Equal("Doe", result.Surname);
        Assert.NotEmpty(result.UniqueNameIdentifier.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAuthorDoesNotExist_ShouldReturnNull()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _repository.GetByIdAsync(nonExistentId, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldMapAllFieldsCorrectly()
    {
        var result = await _repository.GetByIdAsync(_authorId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.False(string.IsNullOrWhiteSpace(result.Name));
        Assert.False(string.IsNullOrWhiteSpace(result.Surname));
        Assert.False(string.IsNullOrWhiteSpace(result.UniqueNameIdentifier.Value));
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

        _authorId = Guid.NewGuid();
        cmd.CommandText = """
            INSERT INTO authors (id, uniquenameidentifier, name, surname)
            VALUES (@id, '9bc70138988276f57849e7b4588523b092f6da1c6e1ca87869', 'John', 'Doe')
            """;
        cmd.Parameters.AddWithValue("id", _authorId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        await _unitOfWork.DisposeAsync();
        await _dataSource.DisposeAsync();
    }
}
