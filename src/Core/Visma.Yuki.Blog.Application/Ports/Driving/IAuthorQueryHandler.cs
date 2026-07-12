using FluentResults;
using Visma.Yuki.Blog.Application.Queries.Author;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.Ports.Driving;

public interface IAuthorQueryHandler
{
    Task<Result<IEnumerable<Author>>> HandleAsync(GetAllAuthorsQuery query, CancellationToken cancellationToken = default);
    Task<Result<Author?>> HandleAsync(GetAuthorByIdQuery query, CancellationToken cancellationToken = default);
}
