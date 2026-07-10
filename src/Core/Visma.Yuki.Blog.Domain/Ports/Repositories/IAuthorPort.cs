using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Domain.Ports.Repositories;

public interface IAuthorPorts
{
    Task SaveAsync(Author Author, CancellationToken cancellationToken = default);
    Task<IEnumerable<Author>> GetAllAsync(CancellationToken cancellationToken = default);
}