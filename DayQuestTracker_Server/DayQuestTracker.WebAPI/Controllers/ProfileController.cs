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

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? throw new UnauthorizedAccessException());

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

        [HttpPost("upload-photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided." });

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(new { error = "Only JPEG, PNG and WebP images are allowed." });

            // Validate file size — max 2MB
            if (file.Length > 2 * 1024 * 1024)
                return BadRequest(new { error = "File size cannot exceed 2MB." });

            var userId = GetUserId();
            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");

            Directory.CreateDirectory(uploadsFolder);

            // Use userId as filename — overwrites previous photo automatically
            var extension = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"{userId}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save URL to DB
            var photoUrl = $"/uploads/profiles/{fileName}";
            var result = await _mediator.Send(new UpdateProfilePhotoCommand(userId, photoUrl));

            return result.IsSuccess
                ? Ok(new { photoUrl })
                : BadRequest(new { error = result.Error });
        }

        public record UpdateProfileRequest(string? Username, string? Timezone);
        public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmNewPassword);
    }
}
