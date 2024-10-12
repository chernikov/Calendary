using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Model;

public class Image
{
    public int Id { get; set; }
    public string ImageUrl { get; set; }

    public int CalendarId { get; set; }
    public Calendar Calendar { get; set; }
}
