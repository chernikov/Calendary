using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Calendary.Model;

public class Calendar
{
    public int Id { get; set; }
    public int Year { get; set; }
    public string FirstDayOfWeek { get; set; } // "Monday", "Sunday"

    public int LanguageId { get; set; }
    public Language Language { get; set; } 

    public int OrderId { get; set; }
    public Order Order { get; set; }

    public ICollection<Image> Images { get; set; }
    public ICollection<EventDate> EventDates { get; set; }

    public ICollection<CalendarHoliday> CalendarHolidays { get; set; } = [];
}
