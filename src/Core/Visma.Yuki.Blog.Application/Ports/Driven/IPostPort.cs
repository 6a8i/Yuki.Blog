using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.Ports.Driven;

public interface IPostPorts
{
    Task AddAsync(Post post, CancellationToken cancellationToken = default);
}