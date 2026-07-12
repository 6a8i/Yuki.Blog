using FluentResults;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.Ports.Driving;

public interface IAuthorUseCase
{
    Task<Result<Author>> CreateAuthorAsync(string Name, string Surname, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Author>>> GetAuthorsAsync(CancellationToken cancellationToken = default);
    Task<Result<Author?>> GetAuthorAsync(Guid id, CancellationToken cancellationToken);
}