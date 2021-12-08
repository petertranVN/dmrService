
using System;
using System.IO;
using AutoService.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace AutoService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
             .ConfigureLogging(loggerFactory =>
             {
                 var path = Directory.GetCurrentDirectory();
                 loggerFactory.AddFile($"{path}\\Logs\\Log.txt");
             })
            .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;

                    AppSettings options = configuration.GetSection("AppSettings").Get<AppSettings>();
                    services.AddSingleton(options);
                    services.AddLogging();
                    services.AddHostedService<Worker>();
                });
    }
}
