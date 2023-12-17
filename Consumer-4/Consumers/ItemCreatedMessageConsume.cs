using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Shared.Messages;

namespace Consumer.Consumers
{
    public class ExampleConsume : IConsumer<ItemCreatedMessage>
    {
        public Task Consume(ConsumeContext<ItemCreatedMessage> context)
        {
            ItemCreatedMessage message = context.Message;
            
            System.Console.WriteLine($"Recieved message details: Item Name: {message.Name}");

            return Task.CompletedTask;
        }
    }
}