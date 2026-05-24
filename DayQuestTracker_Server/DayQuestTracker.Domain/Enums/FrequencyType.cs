namespace DayQuestTracker.Domain.Enums
{
    public enum FrequencyType
    {
        Daily = 1,
        Weekly = 2,
        Custom = 3,
        OnceAWeek = 4, // any day Mon-Sun, must complete at least once
        OnceAMonth = 5 // any day in calendar month, must complete at least once
    }
}
