using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using MassTransit;
using Microsoft.Extensions.Logging;

using Sample.Domain.Contracts;

namespace Sample.Application.Consumers
{
    public class ThingCreatedConsumer : IConsumer<IThingCreated>
    {
        private readonly ILogger<ThingCreatedConsumer> _logger;

        public ThingCreatedConsumer(ILogger<ThingCreatedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IThingCreated> context)
        {
            _logger.LogDebug("Thing Created Event Received - Id={thingId}, Name={thingName}", context.Message.Id, context.Message.Name);
        }
    }
}
