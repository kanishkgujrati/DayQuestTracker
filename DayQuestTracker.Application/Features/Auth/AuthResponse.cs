namespace DayQuestTracker.Application.Features.Auth
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiry { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Level { get; set; }
        public int TotalXP { get; set; }
    }
}
