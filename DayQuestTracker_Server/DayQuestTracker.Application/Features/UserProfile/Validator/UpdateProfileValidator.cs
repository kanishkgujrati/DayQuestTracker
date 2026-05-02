using DayQuestTracker.Application.Features.UserProfile.Commands;
using FluentValidation;

namespace DayQuestTracker.Application.Features.UserProfile.Validator
{
    public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username cannot be empty.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
                .MaximumLength(100).WithMessage("Username cannot exceed 100 characters.")
                .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("Username can only contain letters, numbers and underscores.")
                .When(x => x.Username is not null);

            RuleFor(x => x.Timezone)
                .NotEmpty().WithMessage("Timezone cannot be empty.")
                .Must(BeAValidTimezone).WithMessage("Invalid timezone. Must be a valid IANA timezone e.g. Asia/Kolkata.")
                .When(x => x.Timezone is not null);
        }

        private bool BeAValidTimezone(string? timezone)
        {
            if (timezone is null) return true;
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(timezone);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
