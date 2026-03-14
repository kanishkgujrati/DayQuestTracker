using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Categories.Commands
{
    public record DeleteCategoryCommand(Guid Id, Guid UserId) : IRequest<Result<bool>>;

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

            // Soft delete — never hard delete
            category.DeletedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }

}
