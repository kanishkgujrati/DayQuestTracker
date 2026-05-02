using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using DayQuestTracker.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Categories.Commands
{
    public record CreateCategoryCommand(Guid UserId, string Name, string Color, string? Icon) : IRequest<Result<CategoryDto>>;

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
    {
        private readonly ITrackerDbContext _context;

        public CreateCategoryCommandHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            // Check duplicate name for this user
            var trimmedName = request.Name.Trim();

            var exists = await _context.Categories
                .AnyAsync(c => c.UserId == request.UserId &&
                               c.Name == trimmedName,
                          cancellationToken);

            if (exists)
                return Result<CategoryDto>.Failure("A category with this name already exists.");

            var category = new Category
            {   
                UserId = request.UserId,
                Name = trimmedName,
                Color = request.Color,
                Icon = request.Icon
            };

            _context.Categories.Add(category);
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
