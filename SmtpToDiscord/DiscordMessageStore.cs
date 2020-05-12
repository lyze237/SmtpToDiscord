using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Microsoft.Extensions.Logging;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace SmtpToDiscord
{
    public class DiscordMessageStore : MessageStore
    {
        private readonly ILogger<DiscordMessageStore> logger;
        private readonly DiscordWebhookClient webhook;

        public DiscordMessageStore(ILogger<DiscordMessageStore> logger, DiscordWebhookClient webhook) =>
            (this.logger, this.webhook) = (logger, webhook);

        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken)
        {
            if (!(transaction.Message is ITextMessage message))
                return Task.FromResult(SmtpResponse.ServiceReady);
            
            var content = MimeKit.MimeMessage.Load(message.Content);
            logger.LogInformation($"{content.From}\t{content.Subject}");
            
            webhook.SendMessageAsync("", false, new[] { 
                new EmbedBuilder()
                .WithAuthor(content.From.ToString())
                .WithTitle(content.Subject)
                .WithDescription(content.TextBody)
                .WithCurrentTimestamp()
                .Build()
            });
            
            return Task.FromResult(SmtpResponse.Ok);
        }
    }
}