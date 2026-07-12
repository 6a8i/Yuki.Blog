using FluentResults;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Application.Ports.Driving;
using Visma.Yuki.Blog.Application.Queries.Author;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.UseCases;

public class AuthorQueryHandler(IAuthorPorts authorPorts) : IAuthorQueryHandler
{
    private readonly IAuthorPorts _authorPorts = authorPorts;

    public async Task<Result<IEnumerable<Author>>> HandleAsync(GetAllAuthorsQuery query, CancellationToken cancellationToken = default)
    {
        IEnumerable<Author> authors = await _authorPorts.GetAllAsync(cancellationToken);

        return Result.Ok(authors);
    }

    public async Task<Result<Author?>> HandleAsync(GetAuthorByIdQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            Author? author = await _authorPorts.GetByIdAsync(query.Id, cancellationToken);
            
            return Result.Ok(author);
        }
        catch(Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
