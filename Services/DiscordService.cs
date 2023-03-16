using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace CodeExecutor.Services;

internal class DiscordService
{
    private readonly DiscordClient client;
    private readonly Options options;

    public DiscordService(Options options, ILoggerFactory loggerFactory, CommandsService commands)
    {
        this.options = options;
        client = new(new()
        {
            Token = options.BotToken,
            LoggerFactory = loggerFactory,
            Intents = DiscordIntents.All
        });
        client.MessageCreated += commands.Setup();
    }

    public async Task Disconnect() => await client.DisconnectAsync();
    public async Task Connect() => await client.ConnectAsync();
}
