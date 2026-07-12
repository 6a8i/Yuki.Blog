using FluentResults;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Application.Ports.Driving;
using Visma.Yuki.Blog.Application.Queries.Post;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.UseCases;

public class PostQueryHandler(
    IUnitOfWork uow,
    IPostPorts postPorts) : IPostQueryHandler
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPostPorts _postPorts = postPorts;

    public async Task<Result<IEnumerable<Post>>> HandleAsync(GetAllPostsQuery query, CancellationToken cancellationToken = default)
    {
        await _uow.BeginTransactionAsync(cancellationToken);

        try
        {
            IEnumerable<Post> posts = await _postPorts.GetAllAsync(query.IncludeAuthor, cancellationToken);

            await _uow.CommitAsync(cancellationToken);

            return Result.Ok(posts);
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync(cancellationToken);
            return Result.Fail<IEnumerable<Post>>($"Failed to retrieve posts. => {ex}");
        }
    }

    public async Task<Result<Post?>> HandleAsync(GetPostByIdQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            Post? post = await _postPorts.GetPostByIdAsync(query.Id, query.IncludeAuthor, cancellationToken);

            return Result.Ok(post);
        }
        catch
        {
            return Result.Fail<Post?>($"Failed to retrieve post.");
        }
    }
}
