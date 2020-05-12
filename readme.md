A really basic smtp -> discord webhook program.

It redirects all emails received on port 25 to a discord webhook.

Simply add the webhook to `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Webhook": "https://discordapp.com/api/webhooks/..."
}
```
