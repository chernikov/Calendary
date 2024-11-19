using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Core.Services.Models;

public record CreateModelResponse
{
    public string Name { get; init; }
    public string Owner { get; init; }
    public string Url { get; init; }
    public string Visibility { get; init; }
    public string CreatedAt { get; init; }
    public string Description { get; init; }
}
