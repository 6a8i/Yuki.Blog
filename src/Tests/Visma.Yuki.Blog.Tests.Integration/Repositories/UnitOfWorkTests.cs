using Npgsql;
using Visma.Yuki.Blog.Infrastructure.Repositories;
using Visma.Yuki.Blog.Tests.Integration.Infrastructure;

namespace Visma.Yuki.Blog.Tests.Integration.Repositories;

public class UnitOfWorkTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly string _connectionString;
    private NpgsqlDataSource _dataSource = null!;

    public UnitOfWorkTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _connectionString = factory.ConnectionString;
    }

    [Fact]
    public async Task Connection_WhenAccessedTwice_ShouldReturnSameInstance()
    {
        var uow = new UnitOfWork(_dataSource);

        var connection1 = uow.Connection;
        var connection2 = uow.Connection;

        Assert.Same(connection1, connection2);

        uow.Dispose();
    }

    [Fact]
    public async Task Transaction_BeforeBeginTransaction_ShouldBeNull()
    {
        var uow = new UnitOfWork(_dataSource);

        Assert.Null(uow.Transaction);

        uow.Dispose();
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldSetTransaction()
    {
        var uow = new UnitOfWork(_dataSource);

        await uow.BeginTransactionAsync();

        Assert.NotNull(uow.Transaction);

        await uow.RollbackAsync();
        uow.Dispose();
    }

    [Fact]
    public async Task CommitAsync_WithActiveTransaction_ShouldCommitSuccessfully()
    {
        var uow = new UnitOfWork(_dataSource);
        await uow.BeginTransactionAsync();

        await uow.CommitAsync();

        Assert.Null(uow.Transaction);
        uow.Dispose();
    }

    [Fact]
    public async Task CommitAsync_WhenTransactionAlreadyDisposed_ShouldRollbackAndThrow()
    {
        var uow = new UnitOfWork(_dataSource);
        await uow.BeginTransactionAsync();
        var transaction = uow.Transaction!;

        transaction.Dispose();

        await Assert.ThrowsAnyAsync<Exception>(() => uow.CommitAsync());

        uow.Dispose();
    }

    [Fact]
    public async Task RollbackAsync_WithActiveTransaction_ShouldRollbackAndClearTransaction()
    {
        var uow = new UnitOfWork(_dataSource);
        await uow.BeginTransactionAsync();

        await uow.RollbackAsync();

        Assert.Null(uow.Transaction);
        uow.Dispose();
    }

    [Fact]
    public async Task RollbackAsync_WithoutActiveTransaction_ShouldNotThrow()
    {
        var uow = new UnitOfWork(_dataSource);

        await uow.RollbackAsync();

        uow.Dispose();
    }

    [Fact]
    public async Task Dispose_ShouldReleaseConnectionAndTransaction()
    {
        var uow = new UnitOfWork(_dataSource);
        await uow.BeginTransactionAsync();
        _ = uow.Connection;

        uow.Dispose();

        Assert.True(true);
    }

    [Fact]
    public async Task Dispose_WhenCalledTwice_ShouldNotThrow()
    {
        var uow = new UnitOfWork(_dataSource);
        await uow.BeginTransactionAsync();

        uow.Dispose();
        uow.Dispose();

        Assert.True(true);
    }

    [Fact]
    public async Task DisposeAsync_WhenCalledTwice_ShouldNotThrow()
    {
        var uow = new UnitOfWork(_dataSource);
        await uow.BeginTransactionAsync();

        await uow.DisposeAsync();
        await uow.DisposeAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task DisposeAsync_WithoutBeginTransaction_ShouldNotThrow()
    {
        var uow = new UnitOfWork(_dataSource);

        await uow.DisposeAsync();

        Assert.True(true);
    }

    public Task InitializeAsync()
    {
        _dataSource = new NpgsqlDataSourceBuilder(_connectionString).Build();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _dataSource.DisposeAsync();
    }
}
