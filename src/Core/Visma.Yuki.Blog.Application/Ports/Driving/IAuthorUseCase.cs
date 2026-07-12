using FluentResults;
using Visma.Yuki.Blog.Application.Commands.Author;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.Ports.Driving;

public interface IAuthorUseCase
{
    Task<Result<Author>> CreateAuthorAsync(CreateAuthorCommand command, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Author>>> GetAuthorsAsync(CancellationToken cancellationToken = default);
    Task<Result<Author?>> GetAuthorAsync(Guid id, CancellationToken cancellationToken);
}