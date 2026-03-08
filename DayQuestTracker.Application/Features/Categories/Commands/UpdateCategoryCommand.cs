using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Categories.Commands
{
    public record UpdateCategoryCommand(Guid Id,Guid UserId,string? Name,string? Color,string? Icon) : IRequest<Result<CategoryDto>>;

    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
    {
        private readonly ITrackerDbContext _context;

        public UpdateCategoryCommandHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<CategoryDto>> Handle(
            UpdateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == request.Id &&
                                          c.UserId == request.UserId,
                                     cancellationToken);

            if (category is null)
                return Result<CategoryDto>.Failure("Category not found.");

            if (request.Name is not null)
                category.Name = request.Name.Trim();

            if (request.Color is not null)
                category.Color = request.Color;

            if (request.Icon is not null)
                category.Icon = request.Icon;
            
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<CategoryDto>.Success(new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Color = category.Color,
                Icon = category.Icon,
                CreatedAt = category.CreatedAt
            });
        }
    }
}
