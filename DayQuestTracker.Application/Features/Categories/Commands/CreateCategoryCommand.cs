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

        public async Task<Result<CategoryDto>> Handle(
            CreateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<CategoryDto>.Failure("Name cannot be empty.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(request.Color, @"^#[0-9A-Fa-f]{6}$"))
                return Result<CategoryDto>.Failure("Color must be a valid hex code e.g. #FF5733.");

            if (request.Icon is not null && request.Icon.Length > 50)
                return Result<CategoryDto>.Failure("Icon key cannot exceed 50 characters.");

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
                Name = request.Name.Trim(),
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
