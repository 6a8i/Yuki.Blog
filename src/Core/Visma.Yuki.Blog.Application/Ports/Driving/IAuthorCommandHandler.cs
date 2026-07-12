using FluentResults;
using Visma.Yuki.Blog.Application.Commands.Author;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.Ports.Driving;

public interface IAuthorCommandHandler
{
    Task<Result<Author>> HandleAsync(CreateAuthorCommand command, CancellationToken cancellationToken = default);
}
