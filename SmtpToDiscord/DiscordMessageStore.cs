using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Microsoft.Extensions.Logging;
using MimeKit;
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
            
            var content = MimeMessage.Load(message.Content);
            logger.LogInformation($"{content.From}\t{content.Subject}");

            if (content.From.ToString().Contains("ups") && content.Subject.Contains("Daily Report"))
            {
                logger.LogInformation($"Ignoring message since it's a boring daily report");
                return Task.FromResult(SmtpResponse.Ok);
            }

            var embeds = new[] { 
                new EmbedBuilder()
                    .WithAuthor(content.From.ToString())
                    .WithTitle(content.Subject)
                    .WithDescription(content.TextBody)
                    .WithCurrentTimestamp()
                    .Build()
            };
            
            webhook.SendMessageAsync("", false, embeds);
            
            foreach (var attachment in content.Attachments)
            {
                Thread.Sleep(1000);
                
                using var stream = new MemoryStream();
                string fileName;
                    
                if (attachment is MessagePart rfc822)
                {
                    fileName = rfc822.ContentDisposition?.FileName ?? "attached-message.eml";
                    rfc822.Message.WriteTo(stream, cancellationToken);
                }
                else
                {
                    var part = (MimePart) attachment;
                    fileName = part.FileName;
                    part.Content.DecodeTo(stream, cancellationToken);
                }
                
                stream.Seek(0, SeekOrigin.Begin);
                webhook.SendFileAsync(stream, fileName, "");
            }
            
            return Task.FromResult(SmtpResponse.Ok);
        }
    }
}