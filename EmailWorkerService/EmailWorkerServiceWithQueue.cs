using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace EmailWorkerService
{
    public class EmailWorkerServiceWithQueue : BackgroundService
    {
        private readonly ILogger<EmailWorkerServiceWithQueue> _logger;
        private readonly Queue<string> _mailMessages;

        public EmailWorkerServiceWithQueue(ILogger<EmailWorkerServiceWithQueue> logger)
        {
            _logger = logger;
            _mailMessages = new Queue<string>();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("starting email worker service");

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("stopping email worker service");

            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                string message = string.Empty;
                try
                {
                    // Let's wait for a message to appear in the queue
                    // If the token gets canceled, then we'll stop waiting
                    // since an OperationCanceledException will be thrown
                    if (_mailMessages.Count > 0)
                    {
                        message = _mailMessages.Dequeue();
                        // As soon as a message is available, we'll send it
                        _logger.LogInformation($"E-mail sent to {message}");
                    }
                }
                catch (OperationCanceledException)
                {
                    // We need to terminate the delivery, so we'll just break the while loop
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Couldn't send an e-mail to {recipient}", message);
                    // Wait a few seconds, maybe the mail server was busy
                    await Task.Delay(5000);
                    // Then re-queue this email for later delivery
#warning Some email messages will be stuck in the loop if the SMTP server will always reject them because of their content
                    _mailMessages.Enqueue(message);
                }
            }
        }

        public void SendEmail(string email, string subject, string htmlMessage)
        {
            //var message = new MimeMessage();
            //message.From.Add(MailboxAddress.Parse(optionsMonitor.CurrentValue.Sender));
            //message.To.Add(MailboxAddress.Parse(email));
            //message.Subject = subject;
            //message.Body = new TextPart("html")
            //{
            //    Text = htmlMessage
            //};
            // We just enqueue the message in memory. Delivery will be attempted in background (see the DeliverAsync method)
#warning Implement durable persistence or messages will be lost when the application is restarted
            //await this.mailMessages.SendAsync(message);
            //await _mailMessages.SendAsync(subject);
            _mailMessages.Enqueue(subject);
        }
    }
}
