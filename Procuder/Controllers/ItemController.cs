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

        public ItemController(IPublishEndpoint publish)
        {
            _publish = publish;
        }

        [HttpPost]
        public IActionResult CreateItem()
        {
            // Add code for creating a new item

            // Publish message for item created to rabbitMQ server
            _publish.Publish(message: new ItemCreatedMessage(Name: "Item-1"));

            return Created();
        }
    }
}