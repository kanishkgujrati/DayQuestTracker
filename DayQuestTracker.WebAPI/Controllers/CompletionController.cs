using DayQuestTracker.Application.Features.Completions.Commands;
using DayQuestTracker.Application.Features.Completions.Queries;
using DayQuestTracker.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DayQuestTracker.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompletionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CompletionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException());

        [HttpPost]
        public async Task<IActionResult> LogCompletion([FromBody] LogCompletionRequest request)
        {
            var result = await _mediator.Send(new LogCompletionCommand(
                request.TaskId,
                GetUserId(),
                request.CompletionDate,
                request.Status,
                request.Notes));

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetCompletions), null, result.Value)
                : BadRequest(new { error = result.Error });
        }

        [HttpDelete("{completionId}")]
        public async Task<IActionResult> UndoCompletion(Guid completionId)
        {
            var result = await _mediator.Send(
                new UndoCompletionCommand(completionId, GetUserId()));

            return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
        }

        [HttpGet]
        public async Task<IActionResult> GetCompletions(
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate,
            [FromQuery] Guid? taskId = null)
        {
            var result = await _mediator.Send(
                new GetCompletionsQuery(GetUserId(), startDate, endDate, taskId));

            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }
    }

    public record LogCompletionRequest(Guid TaskId,DateOnly CompletionDate,CompletionStatus Status,string? Notes);
}
