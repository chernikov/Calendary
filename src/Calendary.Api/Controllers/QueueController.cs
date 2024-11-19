using Calendary.Core.Senders;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers
{
    [Route("api/queue")]
    public class QueueController : ControllerBase
    {
        private readonly IRabbitMqSender _rabbitMqSender;

        public QueueController(IRabbitMqSender rabbitMqSender)
        {
            _rabbitMqSender = rabbitMqSender;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            await _rabbitMqSender.SendMessageAsync("create-prediction", "Hello from QueueController");
            return Ok("Hello from QueueController");
        }
    }
}
