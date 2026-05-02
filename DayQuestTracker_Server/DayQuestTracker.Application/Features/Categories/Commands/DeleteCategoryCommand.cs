using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Categories.Commands
{

    public record DeleteCategoryCommand(Guid Id,Guid UserId,bool Force = false) : IRequest<Result<bool>>;

    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<bool>>
    {
        private readonly ITrackerDbContext _context;

        public DeleteCategoryCommandHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(DeleteCategoryCommand request,  CancellationToken cancellationToken)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == request.Id &&
                                          c.UserId == request.UserId,
                                     cancellationToken);

            if (category is null)
                return Result<bool>.Failure("Category not found.");

            // Check for active tasks in this category
            var activeTaskCount = await _context.Tasks
                .CountAsync(t => t.CategoryId == request.Id &&
                                 t.DeletedAt == null,
                            cancellationToken);

            if (activeTaskCount > 0 && !request.Force)
                return Result<bool>.Failure(
                    $"This category has {activeTaskCount} active task(s). " +
                    "Use force=true to delete the category and all its tasks.");

            // Force flag — cascade soft delete to all tasks
            if (activeTaskCount > 0 && request.Force)
            {
                var activeTasks = await _context.Tasks
                    .Where(t => t.CategoryId == request.Id &&
                                t.DeletedAt == null)
                    .ToListAsync(cancellationToken);

                foreach (var task in activeTasks)
                {
                    task.DeletedAt = DateTime.UtcNow;
                    task.UpdatedAt = DateTime.UtcNow;
                }
            }

            category.DeletedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
