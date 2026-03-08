using DayQuestTracker.Application.Common.Interfaces;
using DayQuestTracker.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayQuestTracker.Application.Features.Categories.Queries
{
    public record GetCategoriesQuery(Guid UserId) : IRequest<Result<List<CategoryDto>>>;

    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
    {
        private readonly ITrackerDbContext _context;

        public GetCategoriesQueryHandler(ITrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CategoryDto>>> Handle(
            GetCategoriesQuery request,
            CancellationToken cancellationToken)
        {
            var categories = await _context.Categories
                .Where(c => c.UserId == request.UserId)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Color = c.Color,
                    Icon = c.Icon,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return Result<List<CategoryDto>>.Success(categories);
        }
    }
}
