using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CorrelationId;
using CorrelationId.DependencyInjection;
using MassTransit;
using MassTransit.Definition;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

using Sample.Application.Consumers;
using Sample.Application.Filters;
using Sample.Application.Services;
using Sample.Domain;
using Sample.Domain.Services;

namespace Sample.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public ILogger<Startup> _logger;

        public Startup(ILogger<Startup> logger, IConfiguration configuration)
        {
            Configuration = configuration;
            _logger = logger;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _logger.LogInformation("*** Startup ***");
            try
            {
                services.AddHttpContextAccessor();
                services.AddDefaultCorrelationId(options =>
                {
                    options.AddToLoggingScope = true;
                    options.IncludeInResponse = true;
                    options.RequestHeader = Consts.CorrelationIdHeaderKey;
                    options.ResponseHeader = Consts.CorrelationIdHeaderKey;
                    options.LoggingScopeKey = Consts.CorrelationIdHeaderKey;
                });

                // Services
                services.AddScoped<IThingService, ThingService>();

                ConfigureMassTransit(services);

                services.AddControllers();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCorrelationId();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSerilogRequestLogging();

            app.UseStaticFiles();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureMassTransit(IServiceCollection services)
        {
            services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

            services.AddMassTransit(cfg =>
            {
                cfg.AddConsumer<CreateThingConsumer>();
                cfg.AddConsumer<ThingCreatedConsumer>();

                cfg.UsingRabbitMq(ConfigureBus);
            });

            services.AddMassTransitHostedService();
        }

        private void ConfigureBus(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator cfg)
        {
            // my understanding is this applies these filters to all outgoing publish and send workflows
            cfg.UsePublishFilter(typeof(CorrelationPublishContextFilter<>), context);
            cfg.UseSendFilter(typeof(CorrelationSendContextFilter<>), context);



            cfg.ReceiveEndpoint("thing-created", e =>
            {
                e.UseConsumeFilter(typeof(CorrelationConsumeContextFilter<>), context);
                e.ConfigureConsumer<ThingCreatedConsumer>(context);
            });

            cfg.ReceiveEndpoint("create-thing", e =>
            {
                e.UseConsumeFilter(typeof(CorrelationConsumeContextFilter<>), context);
                e.ConfigureConsumer<CreateThingConsumer>(context);
            });
        }

    }
}
