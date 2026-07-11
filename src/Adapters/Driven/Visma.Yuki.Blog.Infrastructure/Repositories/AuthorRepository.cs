using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Application.Ports.Driven;

namespace Visma.Yuki.Blog.Infrastructure.Repositories;

public class AuthorRepository : IAuthorPorts
{
    public Task AddAsync(Author Author, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task<IEnumerable<Author>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
