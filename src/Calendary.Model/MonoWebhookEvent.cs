using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Model;

public class MonoWebhookEvent
{
    public int Id { get; set; }
    public string EventType { get; set; } = null!;
    public string Data { get; set; } = null!;
    public DateTime ReceivedAt { get; set; }
}
