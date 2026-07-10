using FluentResults;
using Visma.Yuki.Blog.Application.Ports;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.UseCases;

public class AuthorUseCase : IAuthorUseCase
{
    public Task<Result<Author>> CreateAuthorAsync(string Name, string Surname, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<Author>>> GetAuthorsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
