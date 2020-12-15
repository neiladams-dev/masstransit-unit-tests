using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Sample.Api
{
    public class Program
    {
        private static readonly string aspnetcoreEnvironmentKey = "ASPNETCORE_ENVIRONMENT";

        public static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable(aspnetcoreEnvironmentKey) ?? "localhost";

            try
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true)
                    .Build();

                var logger = new LoggerConfiguration().ReadFrom.Configuration(configuration);
                if(environment.Equals("development", StringComparison.OrdinalIgnoreCase) || environment.Equals("localhost" ,StringComparison.OrdinalIgnoreCase))
                {
                    logger.WriteTo.Debug(outputTemplate: "===> {Timestamp:HH:mm:ss} {SourceContext} - [{Level}] {Message}{NewLine}{Exception}");
                }

                Log.Logger = logger.CreateLogger();

                CreateWebHostBuilder(args).Build().Run();

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Exception occurred in Main: {ex.Message}");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseStartup<Startup>();

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .UseSerilog()
        //        .ConfigureWebHostDefaults(webBuilder =>
        //        {
        //            webBuilder.UseStartup<Startup>();
        //        });
    }
}
