using FluentResults;
using Visma.Yuki.Blog.Application.Commands.Post;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.Ports.Driving;

public interface IPostUseCase
{
    Task<Result<Guid>> CreatePostAsync(CreatePostCommand command, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Post>>> GetAllAsync(bool includeAuthor, CancellationToken cancellationToken);
    Task<Result<Post?>> GetPostAsync(Guid id, bool includeAuthor, CancellationToken cancellationToken);
}