using DayQuestTracker.Application.Features.UserProfile.Commands;
using DayQuestTracker.Application.Features.UserProfile.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DayQuestTracker.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException());

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _mediator.Send(new GetProfileQuery(GetUserId()));
            return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var result = await _mediator.Send(
                new UpdateProfileCommand(GetUserId(), request.Username, request.Timezone));

            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _mediator.Send(new ChangePasswordCommand(
                GetUserId(),
                request.CurrentPassword,
                request.NewPassword,
                request.ConfirmNewPassword));

            return result.IsSuccess
                ? Ok(new { message = "Password changed successfully. Please log in again." })
                : BadRequest(new { error = result.Error });
        }
    }

    public record UpdateProfileRequest(string? Username, string? Timezone);
    public record ChangePasswordRequest(string CurrentPassword,string NewPassword,string ConfirmNewPassword);

}
