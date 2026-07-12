using FluentResults;
using FluentValidation;
using Visma.Yuki.Blog.Application.Commands.Author;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Application.Ports.Driving;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Application.UseCases;

public class AuthorUseCase(IAuthorPorts authorPorts, 
                           IValidator<CreateAuthorCommand> createAuthorValidator,
                           IUnitOfWork uow) : IAuthorUseCase
{
    private readonly IAuthorPorts _authorPorts = authorPorts;
    private readonly IValidator<CreateAuthorCommand> _createAuthorValidator = createAuthorValidator;
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<Author>> CreateAuthorAsync(CreateAuthorCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createAuthorValidator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(err => new Error(err.ErrorMessage).WithMetadata("Property", err.PropertyName));
            
            return Result.Fail<Author>(errors);
        }
        
        await _uow.BeginTransactionAsync(cancellationToken);

        try
        {
            var author = new Author(Guid.CreateVersion7(), command.Name, command.Surname);

            Author? authorExists = await _authorPorts.GetByUniqueNameIdentifierAsync(author.UniqueNameIdentifier, cancellationToken);
            
            if (authorExists is not null)
                return Result.Fail<Author>($"The author {command.Name} {command.Surname} already exists.");

            await _authorPorts.AddAsync(author, cancellationToken);
            await _uow.CommitAsync(cancellationToken);

            return Result.Ok(author);
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync(cancellationToken);
            return Result.Fail<Author>($"An error occurred while saving the author to the database. => {ex}");
        }
        finally
        {
            await _uow.DisposeAsync();
        }
    }

    public async Task<Result<Author?>> GetAuthorAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            Author? author = await _authorPorts.GetByIdAsync(id, cancellationToken);
            
            return Result.Ok(author);
        }
        catch(Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<IEnumerable<Author>>> GetAuthorsAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<Author> authors = await _authorPorts.GetAllAsync(cancellationToken);

        return Result.Ok(authors);
    }
}
