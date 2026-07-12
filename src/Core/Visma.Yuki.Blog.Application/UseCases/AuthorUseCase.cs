using FluentResults;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Application.Ports.Driving;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.UseCases;

public class AuthorUseCase(IAuthorPorts authorPorts) : IAuthorUseCase
{
    private readonly IAuthorPorts _authorPorts = authorPorts;

    public Task<Result<Author>> CreateAuthorAsync(string Name, string Surname, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<IEnumerable<Author>>> GetAuthorsAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<Author> authors = await _authorPorts.GetAllAsync(cancellationToken);

        return Result.Ok(authors);
    }
}
