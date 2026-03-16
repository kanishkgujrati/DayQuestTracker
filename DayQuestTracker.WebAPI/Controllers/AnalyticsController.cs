using DayQuestTracker.Application.Features.Analytics.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DayQuestTracker.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnalyticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException());

        [HttpGet("consistency")]
        public async Task<IActionResult> GetConsistency([FromQuery] DateOnly startDate,[FromQuery] DateOnly endDate,[FromQuery] Guid? categoryId = null)
        {
            var result = await _mediator.Send(
                new GetTaskConsistencyQuery(GetUserId(), startDate, endDate, categoryId));

            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }

        [HttpGet("daily-trend")]
        public async Task<IActionResult> GetDailyTrend([FromQuery] DateOnly startDate,[FromQuery] DateOnly endDate)
        {
            var result = await _mediator.Send(
                new GetDailyScoreTrendQuery(GetUserId(), startDate, endDate));

            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }

        [HttpGet("streaks")]
        public async Task<IActionResult> GetStreaks()
        {
            var result = await _mediator.Send(new GetStreakSummaryQuery(GetUserId()));
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }

        [HttpGet("weakest-habits")]
        public async Task<IActionResult> GetWeakestHabits([FromQuery] DateOnly startDate,[FromQuery] DateOnly endDate,[FromQuery] int topN = 5)
        {
            var result = await _mediator.Send(
                new GetWeakestHabitsQuery(GetUserId(), startDate, endDate, topN));

            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }

        [HttpGet("category-performance")]
        public async Task<IActionResult> GetCategoryPerformance([FromQuery] DateOnly startDate,[FromQuery] DateOnly endDate)
        {
            var result = await _mediator.Send(
                new GetCategoryPerformanceQuery(GetUserId(), startDate, endDate));

            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }
    }
}
