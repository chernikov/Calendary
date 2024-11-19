using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Core.Services.Models;

public record TrainModelRequest
{
    public string Destination { get; init; }
    public TrainModelRequestInput Input { get; init; }
    public string Webhook { get; init; }
}
