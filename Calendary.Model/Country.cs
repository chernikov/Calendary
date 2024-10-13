using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Model;

public class Country
{
    public static Country Ukraine = new Country { Id = 1, Name = "Україна", Code = "UA" };

    public int Id { get; set; }
    public string Name { get; set; } // Наприклад, "Україна", "США"
    public string Code { get; set; } // Наприклад, "UA", "US"

    public ICollection<Holiday> Holidays { get; set; }
}
