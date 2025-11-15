using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Core.Settings;
public class ReplicateSettings
{
    public string? ApiKey { get; set; }
    public string Owner { get; set; } = string.Empty;
    public string TrainerModel { get; set; } = string.Empty;
    public string TrainerVersion { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30000;
    public int MaxRetries { get; set; } = 3;
}
