using DayQuestTracker.Application.Features.Categories.Commands;
using DayQuestTracker.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DayQuestTracker.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Extract UserId from JWT token — not from request body
        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException());

        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetCategoriesQuery(GetUserId()));
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpGet("GetCategoryById/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery(id, GetUserId()));
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        [HttpPost("CreateCategory")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            var result = await _mediator.Send(
                new CreateCategoryCommand(GetUserId(), request.Name, request.Color, request.Icon));

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
                : Conflict(result.Error);
        }

        [HttpPatch("UpdateCategoryById/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            var result = await _mediator.Send(
                new UpdateCategoryCommand(id, GetUserId(), request.Name, request.Color, request.Icon));

            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        [HttpDelete("DeleteCategoryById/{id}")]
        public async Task<IActionResult> Delete(Guid id, bool force = false)
        {
            var result = await _mediator.Send(new DeleteCategoryCommand(id, GetUserId(), force));

            return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
        }
    }
    public record CreateCategoryRequest(string Name, string Color, string? Icon);
    public record UpdateCategoryRequest(string? Name, string? Color, string? Icon);
}
