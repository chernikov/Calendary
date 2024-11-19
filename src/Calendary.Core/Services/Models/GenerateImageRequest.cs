using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Core.Services.Models
{
    public record GenerateImageRequest
    {
        public string Version { get; init; }
        public GenerateImageRequestInput Input { get; init; }
    }
}
