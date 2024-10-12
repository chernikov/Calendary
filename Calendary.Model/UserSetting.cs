using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Model;

public class UserSetting
{
    public int Id { get; set; }
    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;
    public int LanguageId { get; set; }
    public Language Language { get; set; } = null!;

    public int CountryId { get; set; }
    public Country Country { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<EventDate> EventDates { get; set; } = [];
}
