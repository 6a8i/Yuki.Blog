using FluentResults;
using Visma.Yuki.Blog.Application.Queries.Post;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.Ports.Driving;

public interface IPostQueryHandler
{
    Task<Result<IEnumerable<Post>>> HandleAsync(GetAllPostsQuery query, CancellationToken cancellationToken = default);
    Task<Result<Post?>> HandleAsync(GetPostByIdQuery query, CancellationToken cancellationToken = default);
}
