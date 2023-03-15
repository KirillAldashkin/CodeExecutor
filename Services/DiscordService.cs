using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace CodeExecutor.Services;

internal class DiscordService
{
    private readonly DiscordClient client;
    private readonly Dictionary<string, CommandHandler> handlers = new();
    private readonly Options options;

    public DiscordService(Options options, ILoggerFactory loggerFactory)
    {
        client = new(new()
        {
            Token = options.BotToken,
            LoggerFactory = loggerFactory,
            Intents = DiscordIntents.All
        });
        client.MessageCreated += HandleCommand;
        this.options = options;
    }

    private async Task HandleCommand(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
    {
        var prefix = options.CommandPrefix;

        if (!e.Message.Content.StartsWith(prefix)) return;
        var cmd = e.Message.Content.AsMemory(prefix.Length);

        foreach(var pair in handlers)
        {
            if (!cmd.Span.StartsWith(pair.Key)) continue;
            _ = pair.Value(cmd[pair.Key.Length..].TrimStart(), e.Message);
            break;
        }
    }

    public async Task Disconnect() => await client.DisconnectAsync();
    public async Task Connect() => await client.ConnectAsync();
    public void AddHandler(string command, CommandHandler handler) => handlers[command] = handler;

    public delegate Task CommandHandler(ReadOnlyMemory<char> text, DiscordMessage message);
}
