using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Model;

public class Holiday
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; } // Наприклад, "Різдво", "День незалежності"

    public int CountryId { get; set; }
    public Country Country { get; set; }
}
