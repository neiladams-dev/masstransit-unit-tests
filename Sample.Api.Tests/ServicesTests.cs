using System;
using Xunit;

using MassTransit;
using MassTransit.Testing;
using Sample.Domain.Contracts;
using Sample.Application.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;
using System.Threading;
using Sample.Application.Services;

namespace Sample.Tests
{
    public class ServicesTests
    {
        [Fact]
        public async void TestThingService_SendCreateThingCommandAsync_Success()
        {
            var provider = new ServiceCollection()
            .AddMassTransitInMemoryTestHarness(cfg =>
            {
                cfg.AddConsumer<CreateThingConsumer>();
                cfg.AddConsumer<ThingCreatedConsumer>();

                cfg.AddConsumerTestHarness<CreateThingConsumer>();
                cfg.AddConsumerTestHarness<ThingCreatedConsumer>();
            })
            .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<InMemoryTestHarness>();

            var mockLoggerThingService = new Mock<ILogger<ThingService>>();
            var mockLoggerCreateThingConsumer = new Mock<ILogger<CreateThingConsumer>>();
            var mockLoggerThingCreatedConsumer = new Mock<ILogger<ThingCreatedConsumer>>();

            await harness.Start();

            var publishEndpoint = (IPublishEndpoint)harness.BusControl;
            var sendEndpointProvider = (ISendEndpointProvider)harness.BusControl;

            var thingService = new ThingService(mockLoggerThingService.Object, sendEndpointProvider, publishEndpoint);

            var sendCommandConsumerHarness = harness.Consumer<CreateThingConsumer>(() => new CreateThingConsumer(mockLoggerCreateThingConsumer.Object, thingService));
            var publishEventConsumerHarness = harness.Consumer<ThingCreatedConsumer>(() => new ThingCreatedConsumer(mockLoggerThingCreatedConsumer.Object));

            try
            {
                await thingService.SendCreateThingCommandAsync(new Domain.Entities.Thing 
                { 
                    Name = "New thing to create"
                });

                Thread.Sleep(3000);
            }
            finally
            {
                await harness.Stop();

                await provider.DisposeAsync();
            }

            Assert.True(await harness.Consumed.Any<ICreateThing>());

            Assert.False(await harness.Published.Any<Fault<ICreateThing>>());

            var sentCommand = harness.Sent.Select<ICreateThing>().ToList().FirstOrDefault();
            Assert.NotNull(sentCommand);
            Assert.Equal("New thing to create", sentCommand.Context.Message.Name);

            var publishedEvent = harness.Published.Select<IThingCreated>().ToList().FirstOrDefault();
            Assert.NotNull(publishedEvent);
            Assert.Equal("New thing to create", publishedEvent.Context.Message.Name);

        }
    }
}
