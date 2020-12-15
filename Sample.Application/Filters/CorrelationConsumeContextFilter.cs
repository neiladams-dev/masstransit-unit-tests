using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CorrelationId;
using CorrelationId.Abstractions;
using GreenPipes;
using MassTransit;
using MassTransit.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sample.Application;
using Sample.Domain;

namespace Sample.Application.Filters
{
    public class CorrelationConsumeContextFilter<T> : IFilter<ConsumeContext<T>> where T : class
    {
        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            var serviceProvider = context.GetPayload<IServiceProvider>();
            var logger = serviceProvider.GetService<ILogger<CorrelationConsumeContextFilter<T>>>();

            logger.LogDebug("Start CorrelationConsumeContextFilter");

            var correlationId = context.Headers.Get<string>(Consts.CorrelationIdHeaderKey);

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = NewId.NextGuid().ToString();

                logger.LogWarning("CorrelationConsumeContextFilter received a message missing a required Correlation-ID in its header. Type={type}, Set new Id={Correlation-ID}", TypeMetadataCache<T>.ShortName, correlationId);
            }

            var correlationContextAccessor = serviceProvider.GetService<ICorrelationContextAccessor>();

            correlationContextAccessor.CorrelationContext = new CorrelationContext(correlationId, Consts.CorrelationIdHeaderKey);

            using (logger.BeginScope(new Dictionary<string, object>
            {
                [Consts.CorrelationIdHeaderKey] = correlationId
            }))
            {
                
                
                logger.LogInformation("CorrelationConsumeContextFilter found correlationId in header and set in logger and correlationContextAccessor.");

                await next.Send(context);
            };
        }

        public void Probe(ProbeContext context)
        {
        }
    }

}
