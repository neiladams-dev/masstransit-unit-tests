# masstransit-unit-tests
Simple Sample solution for evaluating unit testing of Send, Publish, Consumers & Filters with MassTransit

Contains
- Consumer of a command to create a thing
- Consumer of an event that a thing has been created
- Service for performing work on sending command and publishing event
- Publish/Send Filters to automate/ensure Correlation-ID is sent in message header.
- Consumer Filter to ensure Correltation-ID exists in message header and is utilized in logging framework.

Currently:
Application runs as expected.
   1. Post to /things sends Create Thing command
   2. Create thing command consumer simulates creating thing then publishing event that thing was created
   3. Thing created event consumer simulates receiving event that the thing was created.
   4. Correllation Id is logged in most all log messages accross workflows

Unit tests
1) Two Send/Pub/Sub Unit tests are setup but not working
2) I don't know yet how to unit test the filters.

Code utilizes
https://www.stevejgordon.co.uk/what-are-dotnet-worker-services
