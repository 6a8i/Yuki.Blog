using Visma.Yuki.Blog.Domain.Ports.Repositories;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Infrastructure.Repositories;

public class AuthorRepository : IAuthorPorts
{
    public Task SaveAsync(Author Author, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task<IEnumerable<Author>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
