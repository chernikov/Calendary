using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Model;

public class Language
{

    public static Language Ukrainian = new Language { Id = 1, Name = "Українська", Code = "uk-UA" };
    public static Language English = new Language { Id = 2, Name = "English", Code = "en-EN" };
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;
}
