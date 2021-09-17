using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailWorkerService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    var emailWorkerService = services.GetRequiredService<EmailWorkerService>();
                    await emailWorkerService.SendEmailAsync("email", "subject11", "htmlMessage");
                    await emailWorkerService.SendEmailAsync("email", "subject22", "htmlMessage");
                    await emailWorkerService.SendEmailAsync("email", "subject33", "htmlMessage");

                    var emailWorkerServiceWithQueue = services.GetRequiredService<EmailWorkerServiceWithQueue>();
                    emailWorkerServiceWithQueue?.SendEmail("email", "subject1", "htmlMessage");
                    emailWorkerServiceWithQueue?.SendEmail("email", "subject2", "htmlMessage");
                    emailWorkerServiceWithQueue?.SendEmail("email", "subject3", "htmlMessage");
                    emailWorkerServiceWithQueue?.SendEmail("email", "subject4", "htmlMessage");
                    emailWorkerServiceWithQueue?.SendEmail("email", "subject5", "htmlMessage");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred.");
                }
            }

            await host.RunAsync();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<EmailWorkerService>();
                    services.AddSingleton<IHostedService>(p => p.GetRequiredService<EmailWorkerService>());

                    services.AddSingleton<EmailWorkerServiceWithQueue>();
                    services.AddSingleton<IHostedService>(p => p.GetRequiredService<EmailWorkerServiceWithQueue>());

                    //services.AddHostedService<EmailWorkerService>();
                });
    }
}
