using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Domain.ValueObjects;

namespace Visma.Yuki.Blog.Application.Ports.Driven;

public interface IAuthorPorts
{
    Task AddAsync(Author Author, CancellationToken cancellationToken = default);
    Task<IEnumerable<Author>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Author?> GetByUniqueNameIdentifierAsync(UniqueNameIdentifier uniqueNameIdentifier, CancellationToken cancellationToken);
}