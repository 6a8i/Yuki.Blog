using FluentResults;
using Visma.Yuki.Blog.Application.Commands.Post;

namespace Visma.Yuki.Blog.Application.Ports.Driving;

public interface IPostUseCase
{
    Task<Result<Guid>> CreatePostAsync(CreatePostCommand command, CancellationToken cancellationToken = default);
}