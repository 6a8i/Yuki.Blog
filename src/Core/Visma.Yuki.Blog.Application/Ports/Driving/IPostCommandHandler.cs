using FluentResults;
using Visma.Yuki.Blog.Application.Commands.Post;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.Ports.Driving;

public interface IPostCommandHandler
{
    Task<Result<Post>> HandleAsync(CreatePostCommand command, CancellationToken cancellationToken = default);
}
