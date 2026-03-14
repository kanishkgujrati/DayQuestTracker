using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Categories.Queries
{
    public record GetCategoryByIdQuery(Guid Id,Guid UserId) : IRequest<Result<CategoryDto>>;

    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
    {
        private readonly ITrackerDbContext _context;

        public GetCategoryByIdQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _context.Categories
                .Where(c => c.Id == request.Id && c.UserId == request.UserId)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Color = c.Color,
                    Icon = c.Icon,
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (category is null)
                return Result<CategoryDto>.Failure("Category not found.");

            return Result<CategoryDto>.Success(category);
        }
    }
}
