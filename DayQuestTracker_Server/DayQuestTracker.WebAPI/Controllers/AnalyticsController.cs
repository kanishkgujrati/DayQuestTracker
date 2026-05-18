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
        public async Task<IActionResult> GetConsistency([FromQuery] string startDate,[FromQuery] string endDate,[FromQuery] Guid? categoryId = null)
        {
            if (!DateOnly.TryParse(startDate, out var parsedstartDate))
                return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });
            if (!DateOnly.TryParse(endDate, out var parsedEndDate))
                return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });

            var result = await _mediator.Send(
                new GetTaskConsistencyQuery(GetUserId(), parsedstartDate, parsedEndDate, categoryId));

            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }

        [HttpGet("daily-trend")]
        public async Task<IActionResult> GetDailyTrend([FromQuery] string startDate,[FromQuery] string endDate)
        {
            if (!DateOnly.TryParse(startDate, out var parsedstartDate))
                return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });
            if (!DateOnly.TryParse(endDate, out var parsedEndDate))
                return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });

            var result = await _mediator.Send(
                new GetDailyScoreTrendQuery(GetUserId(), parsedstartDate, parsedEndDate));

            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }

        [HttpGet("streaks")]
        public async Task<IActionResult> GetStreaks()
        {
            var result = await _mediator.Send(new GetStreakSummaryQuery(GetUserId()));
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }

        [HttpGet("weakest-habits")]
        public async Task<IActionResult> GetWeakestHabits([FromQuery] string startDate,[FromQuery] string endDate,[FromQuery] int topN = 5)
        {
            if (!DateOnly.TryParse(startDate, out var parsedstartDate))
                return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });
            if (!DateOnly.TryParse(endDate, out var parsedEndDate))
                return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });

            var result = await _mediator.Send(
                new GetWeakestHabitsQuery(GetUserId(), parsedstartDate, parsedEndDate, topN));

            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }

        [HttpGet("category-performance")]
        public async Task<IActionResult> GetCategoryPerformance([FromQuery] string startDate,[FromQuery] string endDate)
        {
            if (!DateOnly.TryParse(startDate, out var parsedstartDate))
                return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });
            if (!DateOnly.TryParse(endDate, out var parsedEndDate))
                return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd." });

            var result = await _mediator.Send(
                new GetCategoryPerformanceQuery(GetUserId(), parsedstartDate, parsedEndDate));

            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }
    }
}
