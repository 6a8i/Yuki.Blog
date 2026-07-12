using FluentValidation;

namespace Visma.Yuki.Blog.Application.Commands.Author;

public record CreateAuthorCommand(string Name, string Surname);

public class CreateAuthorCommandValidator : AbstractValidator<CreateAuthorCommand>
{
    public CreateAuthorCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Author's name is required.")
            .MaximumLength(150).WithMessage("Name cannot exceed 150 characters.");

        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("Author's surname is required.")
            .MaximumLength(150).WithMessage("Surname cannot exceed 150 characters.");
    }
}