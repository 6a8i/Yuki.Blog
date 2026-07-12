using FluentValidation;

namespace Visma.Yuki.Blog.Application.Commands.Post;

public record CreatePostCommand(string Title, string? Description, string Content, Guid? AuthorId, string? AuthorName, string? AuthorSurname);

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");
        
        When(x => !string.IsNullOrEmpty(x.Description), () =>
        {
            RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description can be null but not empty or whitespace.")
            .MaximumLength(300).WithMessage("Description cannot exceed 300 characters.");
        });
        
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");

        RuleFor(x => x)
        .Must(x => x.AuthorId.HasValue || (!string.IsNullOrEmpty(x.AuthorName) && !string.IsNullOrEmpty(x.AuthorSurname)))
        .WithName("AuthorIdentification")
        .WithMessage("You must provide either the 'AuthorId' OR both 'AuthorName' and 'AuthorSurname'.");

        When(x => !string.IsNullOrWhiteSpace(x.AuthorName), () =>
        {
            RuleFor(x => x.AuthorSurname)
                .NotEmpty().WithMessage("AuthorSurname is required when AuthorName is provided.")
                .MaximumLength(150).WithMessage("AuthorSurname cannot exceed 150 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.AuthorSurname), () =>
        {
            RuleFor(x => x.AuthorName)
                .NotEmpty().WithMessage("AuthorName is required when AuthorSurname is provided.")
                .MaximumLength(150).WithMessage("AuthorName cannot exceed 150 characters.");
        });
    }
}