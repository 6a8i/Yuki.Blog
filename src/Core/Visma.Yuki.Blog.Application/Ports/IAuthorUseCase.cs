using FluentResults;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.Ports;

public interface IAuthorUseCase
{
    Task<Result<Author>> CreateAuthorAsync(string Name, string Surname, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Author>>> GetAuthorsAsync(CancellationToken cancellationToken = default);
}