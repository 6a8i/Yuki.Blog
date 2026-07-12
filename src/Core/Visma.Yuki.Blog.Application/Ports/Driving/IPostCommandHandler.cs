using FluentResults;
using Visma.Yuki.Blog.Application.Commands.Post;

namespace Visma.Yuki.Blog.Application.Ports.Driving;

public interface IPostCommandHandler
{
    Task<Result<Guid>> HandleAsync(CreatePostCommand command, CancellationToken cancellationToken = default);
}
