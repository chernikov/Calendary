using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Core.Services.Models;

public record CreateModelRequest
{
    public string Owner { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string Visibility { get; init; }
    public string Hardware { get; init; }
}
