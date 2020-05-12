using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.Webhook;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmtpServer;
using SmtpServer.Storage;

namespace SmtpToDiscord
{
    public class Worker : BackgroundService
    {
        private SmtpServer.SmtpServer smtpServer;

        public Worker(MessageStore messageStore)
        {
            smtpServer = new SmtpServer.SmtpServer(new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Port(25)
                .MessageStore(messageStore)
                .Build());
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) => await smtpServer.StartAsync(stoppingToken);
    }
}