using DayQuestTracker.Application.Features.Tasks.Commands;
using DayQuestTracker.Application.Features.Tasks.Queries;
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
    public class HabitTasksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HabitTasksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException());

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId = null)
        {
            var result = await _mediator.Send(new GetHabitTasksQuery(GetUserId(), categoryId));
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetHabitTaskByIdQuery(id, GetUserId()));
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateHabitTaskRequest request)
        {
            var result = await _mediator.Send(new CreateHabitTaskCommand(
                GetUserId(),
                request.CategoryId,
                request.Title,
                request.Description,
                request.Difficulty,
                request.FrequencyType,
                request.TargetPerWeek,
                request.ScheduledDays));

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
                : BadRequest(result.Error);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHabitTaskRequest request)
        {
            var result = await _mediator.Send(new UpdateHabitTaskCommand(
                id,
                GetUserId(),
                request.CategoryId,
                request.Title,
                request.Description,
                request.Difficulty,
                request.FrequencyType,
                request.TargetPerWeek,
                request.ScheduledDays));

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteHabitTaskCommand(id, GetUserId()));
            return result.IsSuccess ? NoContent() : NotFound(result.Error);
        }
    }

    public record CreateHabitTaskRequest(Guid CategoryId,string Title,string? Description,int Difficulty,FrequencyType FrequencyType,int? TargetPerWeek,List<int>? ScheduledDays);

    public record UpdateHabitTaskRequest(Guid? CategoryId,string? Title,string? Description,int? Difficulty,FrequencyType? FrequencyType,int? TargetPerWeek,List<int>? ScheduledDays);
}
