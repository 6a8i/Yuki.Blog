using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.Ports.Driven;

public interface IAuthorPorts
{
    Task AddAsync(Author Author, CancellationToken cancellationToken = default);
    Task<IEnumerable<Author>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}