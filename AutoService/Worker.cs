using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using AutoService.Extension;
using AutoService.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoService
{
    public class Worker : BackgroundService
    {
        public enum SimpleServiceCustomCommands
        { StopWorker = 128, RestartWorker, CheckWorker };
        private readonly ILogger<Worker> _logger;
        private readonly AppSettings _appSettings;

        public Worker(ILogger<Worker> logger, AppSettings appSettings)
        {
            _logger = logger;
            _appSettings = appSettings;
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker Start: {time}", DateTimeOffset.Now);
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker Stop: {time}", DateTimeOffset.Now);
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var serviceList = _appSettings.ServiceNameList ?? new string[] { };
            while (!stoppingToken.IsCancellationRequested)
            {
                var service = new ServiceExtension();
                var scs = service.GetServices(serviceList);
                if (scs.Count > 0)
                {
                    foreach (var sc in scs)
                    {
                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            sc.Start();
                            while (sc.Status == ServiceControllerStatus.Stopped)
                            {
                                Thread.Sleep(1000);
                                _logger.LogInformation($"Worker Start {sc.DisplayName} at: {DateTimeOffset.Now}");
                                sc.Refresh();
                            }
                        }
                        if (sc.Status == ServiceControllerStatus.Paused)
                        {
                            sc.Start();
                            while (sc.Status == ServiceControllerStatus.Paused)
                            {
                                Thread.Sleep(1000);
                                _logger.LogInformation($"Worker Start {sc.DisplayName} at: {DateTimeOffset.Now}");
                                sc.Refresh();
                            }
                        }
                    }

                }
                // _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}
