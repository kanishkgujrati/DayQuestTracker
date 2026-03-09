using DayQuestTracker.Application.Features.Categories.Commands;
using FluentValidation;

namespace DayQuestTracker.Application.Features.Categories.Validators
{
    public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("Color is required.")
                .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Color must be a valid hex code e.g. #FF5733.");

            RuleFor(x => x.Icon)
                .MaximumLength(50).WithMessage("Icon cannot exceed 50 characters.")
                .When(x => x.Icon is not null);
        }
    }
}
