using DayQuestTracker.Application.Features.Categories.Commands;
using FluentValidation;

namespace DayQuestTracker.Application.Features.Categories.Validators
{
    public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name cannot be empty.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.")
                .When(x => x.Name is not null);

            RuleFor(x => x.Color)
                .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Color must be a valid hex code e.g. #FF5733.")
                .When(x => x.Color is not null);

            RuleFor(x => x.Icon)
                .MaximumLength(50).WithMessage("Icon cannot exceed 50 characters.")
                .When(x => x.Icon is not null);
        }
    }
}
