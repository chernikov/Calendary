using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Model;

public class EventDate
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = null!;

    public int? UserSettingId { get; set; }
    public UserSetting? UserSetting { get; set; }

    public int? CalendarId { get; set; }
    public Calendar? Calendar { get; set; }
}
