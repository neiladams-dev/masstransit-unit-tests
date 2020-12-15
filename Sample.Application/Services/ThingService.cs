using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using MassTransit;
using Microsoft.Extensions.Logging;

using Sample.Domain.Contracts;
using Sample.Domain.Entities;
using Sample.Domain.Services;

namespace Sample.Application.Services
{
    public class ThingService : IThingService
    {
        private readonly ILogger<ThingService> _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IPublishEndpoint _publisher;

        public ThingService(ILogger<ThingService> logger, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publisher)
        {
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
            _publisher = publisher;
        }

        public async Task SendCreateThingCommandAsync(Thing thing)
        {
            _logger.LogDebug("Start SendCreateThingCommandAsync");

            var sendCommandEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:create-thing"));

            await sendCommandEndpoint.Send<Domain.Contracts.ICreateThing>(new
            {
                Name = thing.Name
            });

            _logger.LogInformation("Sent command to create thing - Name={thingName}", thing.Name);
        }


        public async Task CreateAsync(Thing thing)
        {
            _logger.LogDebug("Start CreateAsync");

            // simulate thing created
            thing.Id = 1;

            _logger.LogInformation("Thing Created - Id={thingId}, Name={thingName}", thing.Id, thing.Name);

            await _publisher.Publish<IThingCreated>(new
            {
                Id = thing.Id,
                Name = thing.Name
            });

            _logger.LogInformation("Published thing created event - Id={thingId}, Name={thingName}", thing.Id, thing.Name);
        }
    }
}
