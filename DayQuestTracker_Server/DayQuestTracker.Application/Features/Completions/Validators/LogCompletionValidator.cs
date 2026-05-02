using DayQuestTracker.Application.Features.Completions.Commands;
using FluentValidation;

namespace DayQuestTracker.Application.Features.Completions.Validators
{
    public class LogCompletionValidator : AbstractValidator<LogCompletionCommand>
    {
        public LogCompletionValidator()
        {
            RuleFor(x => x.TaskId)
                .NotEmpty().WithMessage("TaskId is required.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status. Must be Completed or Skipped.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.")
                .When(x => x.Notes is not null);
        }
    }
}
