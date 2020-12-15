using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using MassTransit;
using Microsoft.Extensions.Logging;

using Sample.Domain.Contracts;
using Sample.Domain.Entities;
using Sample.Domain.Services;

namespace Sample.Application.Consumers
{
    public class CreateThingConsumer : IConsumer<ICreateThing>
    {
        private readonly ILogger<CreateThingConsumer> _logger;
        private readonly IThingService _thingService;

        public CreateThingConsumer(ILogger<CreateThingConsumer> logger, IThingService thingService)
        {
            _logger = logger;
            _thingService = thingService;
        }

        public async Task Consume(ConsumeContext<ICreateThing> context)
        {
            var thing = new Thing
            {
                Name = context.Message.Name
            };

            await _thingService.CreateAsync(thing);

            _logger.LogInformation("Successfully handled CreateThing Command for thing.Id={thingId}, thing.Name={thingNmae}", thing.Id, thing.Name);
        }
    }
}
