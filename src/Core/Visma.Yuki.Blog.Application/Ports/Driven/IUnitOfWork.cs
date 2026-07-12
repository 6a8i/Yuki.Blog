
using System.Data;

namespace Visma.Yuki.Blog.Application.Ports.Driven;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{

    public IDbConnection Connection {get;}
    public IDbTransaction? Transaction {get;}

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}