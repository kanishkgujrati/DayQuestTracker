namespace DayQuestTracker.Application.Features.UserProfile
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Timezone { get; set; } = string.Empty;
        public int TotalXP { get; set; }
        public int Level { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
