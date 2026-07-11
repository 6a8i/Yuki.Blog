
namespace Visma.Yuki.Blog.Application.Ports.Driven;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}