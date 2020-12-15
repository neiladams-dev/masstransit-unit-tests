using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CorrelationId;
using CorrelationId.Abstractions;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Logging;

using Sample.Application;
using Sample.Domain;

namespace Sample.Application.Filters
{
    public class CorrelationSendContextFilter<T> : IFilter<SendContext<T>> where T : class
    {
        private readonly ILogger<CorrelationSendContextFilter<T>> _logger;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public CorrelationSendContextFilter(ILogger<CorrelationSendContextFilter<T>> logger, ICorrelationContextAccessor correlationContextAccessor)
        {
            _logger = logger;
            _correlationContextAccessor = correlationContextAccessor;
        }

        /// <summary>
        /// When sending a command ensure the current correlationId is set in the message header and the message's context.
        /// Note, this is done because our product uses a string correlation-ID datatype and MassTransit uses a Guid correlation-ID datatype.
        /// In the event a non Guid string value was passed as a correlation-ID in our application this will log the association from one to the other.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
        {
            _logger.LogDebug("Start CorrelationSendContextFilter");

            // First try to find the CorrelationId in the CorrelationContextAccessor
            var headerCorrelationId = _correlationContextAccessor?.CorrelationContext?.CorrelationId;

            if (null == headerCorrelationId)
            {
                // Second try to find the CorrelationId already set in the outbound message header
                headerCorrelationId = context.Headers.Get<string>(Consts.CorrelationIdHeaderKey);

                if (null == headerCorrelationId)
                {
                    // Finally try to find the CorrelationId already set in the message context
                    if (null == context.CorrelationId)
                    {
                        context.CorrelationId = NewId.NextGuid();
                        headerCorrelationId = context.CorrelationId.ToString();
                        context.Headers.Set(Consts.CorrelationIdHeaderKey, headerCorrelationId);

                        _logger.LogWarning("CorrelationSendContextFilter unable to find a Correlation-ID.  Using new ID: {Correlation-ID}", headerCorrelationId);
                    }
                    else
                    {
                        _logger.LogWarning("CorrelationSendContextFilter found Correlation-ID in context: {Correlation-ID}", headerCorrelationId);
                    }
                }
                else // found Correlation-ID already exists in message header
                {
                    var correlationGuid = Guid.Empty;

                    if (Guid.TryParse(headerCorrelationId, out correlationGuid))
                    {
                        _logger.LogInformation("CorrelationSendContextFilter found Guid Correlation-ID in header: {Correlation-ID}", headerCorrelationId);
                    }
                    else
                    {
                        // Use current Guid correlationId if it is already set in the message else generate a new one.
                        correlationGuid = context.CorrelationId ?? NewId.NextGuid();
                        _logger.LogInformation("CorrelationSendContextFilter found a Correlation-ID in header that was not a Guid. " +
                                               "Associating header correlationId={headerCorrelationId} to message correlationId={loggingCorrelationId}",
                                               headerCorrelationId, correlationGuid.ToString());

                    }
                    context.Headers.Set(Consts.CorrelationIdHeaderKey, correlationGuid.ToString());
                    context.CorrelationId = correlationGuid;
                }
            }
            else // found _correlationContextAccessor, confirm it is a guid and use it for message/context Correlation-ID also
            {
                context.Headers.Set(Consts.CorrelationIdHeaderKey, headerCorrelationId);

                var correlationGuid = Guid.Empty;

                if (Guid.TryParse(headerCorrelationId, out correlationGuid))
                {
                    _logger.LogInformation("CorrelationSendContextFilter found Guid Correlation-ID in CorrelationContextAccessor: {Correlation-ID}", headerCorrelationId);
                }
                else
                {
                    // Use current Guid correlationId if it is already set in the message else generate a new one.
                    correlationGuid = context.CorrelationId ?? NewId.NextGuid();
                    _logger.LogInformation("CorrelationSendContextFilter found a Correlation-ID in CorrelationContextAccessor that was not a Guid. " +
                                           "Associate header correlationId={headerCorrelationId} to message correlationId={loggingCorrelationId}",
                                           headerCorrelationId, correlationGuid.ToString());
                }
                context.CorrelationId = correlationGuid;
            }

            await next.Send(context);
        }

        public void Probe(ProbeContext context) { }
    }

}
