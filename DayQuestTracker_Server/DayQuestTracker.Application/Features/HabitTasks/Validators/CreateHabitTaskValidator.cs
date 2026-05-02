using DayQuestTracker.Application.Features.Tasks.Commands;
using DayQuestTracker.Domain.Enums;
using FluentValidation;

namespace DayQuestTracker.Application.Features.HabitTasks.Validators
{
    public class CreateHabitTaskValidator : AbstractValidator<CreateHabitTaskCommand>
    {
        public CreateHabitTaskValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
                .When(x => x.Description is not null);

            RuleFor(x => x.Difficulty)
                .InclusiveBetween(1, 5).WithMessage("Difficulty must be between 1 and 5.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.");

            RuleFor(x => x.FrequencyType)
                .IsInEnum().WithMessage("Invalid FrequencyType.");

            // Custom requires TargetPerWeek
            RuleFor(x => x.TargetPerWeek)
                .NotNull().WithMessage("TargetPerWeek is required for Custom frequency.")
                .InclusiveBetween(1, 6).WithMessage("TargetPerWeek must be between 1 and 6.")
                .When(x => x.FrequencyType == FrequencyType.Custom);

            // Daily must not have TargetPerWeek
            RuleFor(x => x.TargetPerWeek)
                .Null().WithMessage("TargetPerWeek must not be set for Daily or Weekly tasks.")
                .When(x => x.FrequencyType != FrequencyType.Custom);

            // Daily must not have ScheduledDays
            RuleFor(x => x.ScheduledDays)
                .Null().WithMessage("Daily tasks do not require scheduled days.")
                .When(x => x.FrequencyType == FrequencyType.Daily);

            // Weekly/Custom require ScheduledDays
            RuleFor(x => x.ScheduledDays)
                .NotNull().WithMessage("Scheduled days are required for Weekly and Custom tasks.")
                .Must(days => days!.Any())
                .WithMessage("At least one scheduled day is required.")
                .When(x => x.FrequencyType != FrequencyType.Daily);

            // Each day must be 0-6
            RuleForEach(x => x.ScheduledDays)
                .InclusiveBetween(0, 6).WithMessage("Each scheduled day must be between 0 (Monday) and 6 (Sunday).")
                .When(x => x.ScheduledDays is not null);
        }
    }
}
