using System.Data;
using System.Data.Common;
using Npgsql;
using Visma.Yuki.Blog.Application.Ports.Driven;

namespace Visma.Yuki.Blog.Infrastructure.Repositories;

public class UnitOfWork(NpgsqlDataSource dataSource) : IUnitOfWork
{
    private readonly NpgsqlDataSource _dataSource = dataSource;
    private DbConnection? _connection;
    private DbTransaction? _transaction;
    private bool _disposed;
    
    // Propriedade para os repositórios pegarem a conexão e transação atuais
    public IDbConnection Connection => _connection ??= _dataSource.CreateConnection();
    public IDbTransaction? Transaction => _transaction;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // Se a conexão ainda não foi instanciada, cria uma
        if (_connection == null)
            _connection = _dataSource.CreateConnection();

        if (_connection.State == ConnectionState.Closed)
            await _connection.OpenAsync(cancellationToken);

        _transaction = await _connection.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction != null)
                await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _transaction?.Dispose();
        _connection?.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        
        if (_transaction != null)
            await _transaction.DisposeAsync();

        if (_connection != null)
            await _connection.DisposeAsync();

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
            await _transaction.RollbackAsync(cancellationToken);
            
        await DisposeTransactionAsync();
    }
}