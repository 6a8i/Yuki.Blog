using Npgsql;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Infrastructure.Repositories;
using Visma.Yuki.Blog.Tests.Integration.Infrastructure;

namespace Visma.Yuki.Blog.Tests.Integration.Repositories;

public class AuthorRepositoryTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly string _connectionString;
    private NpgsqlDataSource _dataSource = null!;
    private UnitOfWork _unitOfWork = null!;
    private AuthorRepository _repository = null!;

    public AuthorRepositoryTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _connectionString = factory.ConnectionString;
    }

    [Fact]
    public async Task GetAllAsync_WhenTableHasData_ShouldReturnAllAuthors()
    {
        var result = await _repository.GetAllAsync();

        var authors = result.ToList();
        Assert.Equal(2, authors.Count);
        Assert.Contains(authors, a => a.Name == "John" && a.Surname == "Doe");
        Assert.Contains(authors, a => a.Name == "Jane" && a.Surname == "Smith");
    }

    [Fact]
    public async Task GetAllAsync_WhenTableIsEmpty_ShouldReturnEmptyCollection()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "TRUNCATE TABLE posts, authors CASCADE;";
        await cmd.ExecuteNonQueryAsync();

        var result = await _repository.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldMapAllFieldsCorrectly()
    {
        var result = await _repository.GetAllAsync();
        var author = result.First(a => a.Name == "John");

        Assert.NotEqual(Guid.Empty, author.Id);
        Assert.Equal("John", author.Name);
        Assert.Equal("Doe", author.Surname);
        Assert.NotEmpty(author.UniqueNameIdentifier.Value);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAuthorsWithUniqueNameIdentifiers()
    {
        var result = await _repository.GetAllAsync();

        foreach (var author in result)
        {
            Assert.False(string.IsNullOrWhiteSpace(author.UniqueNameIdentifier.Value));
        }
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

        cmd.CommandText = """
            INSERT INTO authors (id, uniquenameidentifier, name, surname)
            VALUES
            (gen_random_uuid(), '9bc70138988276f57849e7b4588523b092f6da1c6e1ca87869', 'John', 'Doe'),
            (gen_random_uuid(), '8fb1b1516f1a8cc3f5e5b3f2ec20fa52b4742718fae471fa28', 'Jane', 'Smith');
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        await _unitOfWork.DisposeAsync();
        await _dataSource.DisposeAsync();
    }
}
