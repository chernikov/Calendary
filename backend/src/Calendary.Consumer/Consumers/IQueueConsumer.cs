using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Consumer.Consumers
{
    public interface IQueueConsumer
    {
        Task ProcessMessageAsync(string message);
    }
}
