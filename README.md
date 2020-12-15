# masstransit-unit-tests
Simple Sample solution for evaluating unit testing with  of Send, Publish, Consumers & Filters with MassTransit

Contains
- Api resource to create thing 
  - [Post] to /things sends a command to create something & logs Correlation-ID
- Service class for performing work on sending command, creating thing and publishing thing created event
- Consumer of a command to create a thing
- Consumer of an event that a thing has been created
- Publish/Send Filters to automate/ensure Correlation-ID is sent in message header.
- Consumer Filter to ensure Correltation-ID exists in message header and is utilized in logging framework.
- Unit tests using .AddMassTransitInMemoryTestHarness to configure harness
- Unit tests using direct harness configurations

Currently:
API resource runs as expected.
   1. Post to /things sends Create Thing command
      Body: { "Name" : "This is a test" }
   2. Create thing command consumer consumes create thing command, simulates creating thing then publishing event that thing was created
   3. Thing created event consumer consumes event and logs that the thing was created.
   4. Correllation Id is logged in all application log messages accross command/event workflows

Unit tests
1) ConsumerTests: 
   - Send/Pub/Sub Unit tests are setup but not working intended to directly test consumers
   - Use .AddMassTransitInMemoryTestHarness to configure harness
2) ServicesTests: 
   - Intended to indirectly test consumers with the harness throught the service class.
   - Use .AddMassTransitInMemoryTestHarness to configure harness

Code utilizes
Steve Gordon - ASP.NET CORE CORRELATION IDS
https://www.stevejgordon.co.uk/asp-net-core-correlation-ids
https://www.stevejgordon.co.uk/updates-asp-net-core-correlation-id-library
https://github.com/stevejgordon/CorrelationId
https://www.nuget.org/packages/CorrelationId/
