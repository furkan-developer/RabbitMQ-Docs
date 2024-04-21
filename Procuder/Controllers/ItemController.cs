using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Shared.Messages;

namespace Procuder.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly IPublishEndpoint _publish;
        private readonly ISendEndpointProvider _sendProvider;

        public ItemController(IPublishEndpoint publish, ISendEndpointProvider sendEndpointProvider)
        {
            _publish = publish;
            _sendProvider = sendEndpointProvider;
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem()
        {
            // Add code for creating a new item

            // Publish message for item created to rabbitMQ server
            // _publish.Publish(message: new ItemCreatedMessage(Name: "Item-1"));


            // Set up endpoint
            ISendEndpoint sendEndpoint = await _sendProvider.GetSendEndpoint(new Uri(string.Concat("amqps://welaavja:x6hkeE8trOMn48bGFhmy_q1hgpCYlZA-@crow.rmq.cloudamqp.com/welaavja", "/","specify-queue")));
            
            // Publish message
            await sendEndpoint.Send(message: new ItemCreatedMessage(Name: "Item-1"));

            System.Console.WriteLine("Published Item-1 to specify-queue");
            
            return Created();
        }
    }
}