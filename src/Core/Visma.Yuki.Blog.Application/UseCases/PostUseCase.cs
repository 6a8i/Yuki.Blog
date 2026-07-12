using FluentResults;
using FluentValidation;
using Visma.Yuki.Blog.Application.Commands.Post;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Application.Ports.Driving;
using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Domain.ValueObjects;

namespace Visma.Yuki.Blog.Application.UseCases;

public class PostUseCase(
    IUnitOfWork uow, 
    IValidator<CreatePostCommand> createPostValidator,
    IAuthorPorts authorPorts,
    IPostPorts postPorts) : IPostUseCase
{

    private readonly IUnitOfWork _uow = uow;
    private readonly IValidator<CreatePostCommand> _createPostValidator = createPostValidator;
    private readonly IAuthorPorts _authorPorts = authorPorts;
    private readonly IPostPorts _postPorts = postPorts;

    public async Task<Result<Guid>> CreatePostAsync(CreatePostCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createPostValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(err => new Error(err.ErrorMessage).WithMetadata("Property", err.PropertyName));
            
            return Result.Fail<Guid>(errors);
        }

        Author? author = null;

        await _uow.BeginTransactionAsync(cancellationToken);
        try
        {
            if(command.AuthorId.HasValue)
            {
                author = await _authorPorts.GetByIdAsync(command.AuthorId.Value, cancellationToken);
                
                if(author is null)
                    return Result.Fail<Guid>("The Author Identification informed doesn't exists.");
            } 
            else if(!string.IsNullOrEmpty(command.AuthorName) && !string.IsNullOrEmpty(command.AuthorSurname))
            {
                UniqueNameIdentifier uni = UniqueNameIdentifier.Create(command.AuthorName, command.AuthorSurname);

                author = await _authorPorts.GetByUniqueNameIdentifierAsync(uni, cancellationToken);
            }
            else
            {
                return Result.Fail<Guid>("You must provide either the 'AuthorId' OR both 'AuthorName' and 'AuthorSurname'.");
            }
            
            if (author is null)
            {
                author = new(Guid.CreateVersion7(), command.AuthorName!, command.AuthorSurname!);
                
                await _authorPorts.AddAsync(author, cancellationToken);
            }

            Post post = new(Guid.CreateVersion7(), command.Title, command.Description, command.Content, author);

            await _postPorts.AddAsync(post, cancellationToken);
            
            await _uow.CommitAsync(cancellationToken);

            return Result.Ok(post.Id);
        }
        catch
        {
            await _uow.RollbackAsync(cancellationToken);
            return Result.Fail<Guid>($"An error occurred while saving the author to the database.");
        }
    }

    public async Task<Result<IEnumerable<Post>>> GetAllAsync(bool includeAuthor, CancellationToken cancellationToken)
    {
        await _uow.BeginTransactionAsync(cancellationToken);

        try
        {
            IEnumerable<Post> posts = await _postPorts.GetAllAsync(includeAuthor, cancellationToken);

            await _uow.CommitAsync(cancellationToken);
            
            return Result.Ok(posts);
        }
        catch(Exception ex)
        {
            await _uow.RollbackAsync(cancellationToken);
            return Result.Fail<IEnumerable<Post>>($"Failed to retrieve posts. => {ex}");
        }
    }

    public async Task<Result<Post?>> GetPostAsync(Guid id, bool includeAuthor, CancellationToken cancellationToken)
    {
        try
        {
            Post? posts = await _postPorts.GetPostByIdAsync(id, includeAuthor, cancellationToken);
            
            return Result.Ok(posts);
        }
        catch
        {
            return Result.Fail<Post?>($"Failed to retrieve post.");
        }
    }
}