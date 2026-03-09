using DayQuestTracker.Application.Features.Tasks.Commands;
using DayQuestTracker.Domain.Enums;
using FluentValidation;

namespace DayQuestTracker.Application.Features.HabitTasks.Validators
{
    public class UpdateHabitTaskValidator : AbstractValidator<UpdateHabitTaskCommand>
    {
        public UpdateHabitTaskValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.")
                .When(x => x.Title is not null);

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
                .When(x => x.Description is not null);

            RuleFor(x => x.Difficulty)
                .InclusiveBetween(1, 5).WithMessage("Difficulty must be between 1 and 5.")
                .When(x => x.Difficulty.HasValue);

            RuleFor(x => x.FrequencyType)
                .IsInEnum().WithMessage("Invalid FrequencyType.")
                .When(x => x.FrequencyType.HasValue);

            RuleFor(x => x.TargetPerWeek)
                .InclusiveBetween(1, 6).WithMessage("TargetPerWeek must be between 1 and 6.")
                .When(x => x.TargetPerWeek.HasValue);

            // If changing to Custom, TargetPerWeek must be provided
            RuleFor(x => x.TargetPerWeek)
                .NotNull().WithMessage("TargetPerWeek is required when setting Custom frequency.")
                .InclusiveBetween(1, 6).WithMessage("TargetPerWeek must be between 1 and 6.")
                .When(x => x.FrequencyType == FrequencyType.Custom);

            // If changing away from Custom, TargetPerWeek must not be sent
            RuleFor(x => x.TargetPerWeek)
                .Null().WithMessage("TargetPerWeek must not be set for Daily or Weekly tasks.")
                .When(x => x.FrequencyType.HasValue &&
                           x.FrequencyType != FrequencyType.Custom);

            // ScheduledDays range check
            RuleForEach(x => x.ScheduledDays)
                .InclusiveBetween(0, 6)
                .WithMessage("Each scheduled day must be between 0 (Monday) and 6 (Sunday).")
                .When(x => x.ScheduledDays is not null);

            // Daily must not have ScheduledDays
            RuleFor(x => x.ScheduledDays)
                .Null().WithMessage("Daily tasks do not require scheduled days.")
                .When(x => x.FrequencyType == FrequencyType.Daily);

            // Weekly/Custom require at least one ScheduledDay if provided
            RuleFor(x => x.ScheduledDays)
                .Must(days => days != null && days.Any())
                .WithMessage("At least one scheduled day is required for Weekly and Custom tasks.")
                .When(x => x.FrequencyType.HasValue &&
                           x.FrequencyType != FrequencyType.Daily &&
                           x.ScheduledDays is not null);
        }
    }
}
