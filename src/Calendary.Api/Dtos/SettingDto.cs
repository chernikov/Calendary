namespace Calendary.Api.Dtos
{
    public class SettingDto
    {
        public DayOfWeek FirstDayOfWeek { get; set; }
        public LanguageDto? Language { get; set; }
        public CountryDto? Country { get; set; }
        public ICollection<EventDateDto> EventDates { get; set; } = Array.Empty<EventDateDto>();
    }
}
