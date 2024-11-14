using System.ComponentModel.DataAnnotations;

namespace Calendary.Model;

public class UserSetting
{

    public int Id { get; set; }
    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;
    public int LanguageId { get; set; } = Language.Ukrainian.Id;
    public Language Language { get; set; } = null!;

    public int CountryId { get; set; } = Country.Ukraine.Id;
    public Country Country { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<EventDate> EventDates { get; set; } = [];

    //latest delivery address that saved
    [MaxLength(1000)]
    public string? DeliveryAddress { get; set; } = null!;   // saved in text city, postOffice

    [MaxLength(1000)]
    public string? DeliveryRaw { get; set; } // saved in json city and postOffice  
}
