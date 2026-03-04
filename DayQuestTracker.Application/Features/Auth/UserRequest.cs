namespace DayQuestTracker.Application.Features.Auth
{
    public class UserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Timezone { get; set; } = "UTC";
    }
}
