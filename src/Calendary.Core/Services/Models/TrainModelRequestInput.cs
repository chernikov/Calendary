using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Core.Services.Models;

public record TrainModelRequestInput
{
    public int Steps { get; init; }
    public int LoraRank { get; init; }
    public string Optimizer { get; init; }
    public int BatchSize { get; init; }
    public string Resolution { get; init; }
    public bool Autocaption { get; init; }
    public string AutocaptionPrefix { get; init; }
    public string InputImages { get; init; }
    public string TriggerWord { get; init; }
    public double LearningRate { get; init; }
    public string WandbProject { get; init; }
    public int WandbSaveInterval { get; init; }
    public double CaptionDropoutRate { get; init; }
    public bool CacheLatentsToDisk { get; init; }
    public int WandbSampleInterval { get; init; }
}
