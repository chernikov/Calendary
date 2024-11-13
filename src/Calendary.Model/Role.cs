using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Model;

public class Role
{

    public static Role AdminRole = new Role { Id = 1, Name = "Admin" };
    public static Role UserRole = new Role { Id = 2, Name = "User" };

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}