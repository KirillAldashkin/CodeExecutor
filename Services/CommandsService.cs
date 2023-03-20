using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Emzi0767.Utilities;
using Microsoft.Extensions.Logging;

namespace CodeExecutor.Services;

internal class CommandsService
{
    public delegate Task CommandHandler(ReadOnlyMemory<char> text, DiscordMessage message);

    private readonly Dictionary<string, CommandHandler> handlers = new();
    private readonly CoderService coder;
    private readonly FunService fun;
    private readonly Options options;
    private readonly ILogger<CommandsService> logger;

    public CommandsService(CoderService coder, FunService fun, Options options, ILogger<CommandsService> logger)
    {
        this.coder = coder;
        this.fun = fun;
        this.options = options;
        this.logger = logger;
    }

    public AsyncEventHandler<DiscordClient, MessageCreateEventArgs> Setup()
    {
        AddHandler("кто", fun.WhoIs);
        AddHandler("шанс", fun.GetChance("Шанс"));
        AddHandler("вероятность", fun.GetChance("Вероятность"));

        AddHandler("exec help", coder.PrintHelp);
        AddHandler("exec info", coder.PrintVersions);
        AddHandler("exec python", coder.ExecutePython);
        AddHandler("exec csharp", coder.ExecuteCSharp);
        AddHandler("exec java", coder.ExecuteJava);
        AddHandler("exec c", coder.ExecuteC);

        return HandleCommand;
    }

    private void AddHandler(string cmd, CommandHandler handler) => handlers[cmd] = handler;

    private Task HandleCommand(DiscordClient sender, MessageCreateEventArgs e)
    {
        var prefix = options.CommandPrefix;

        if (!e.Message.Content.StartsWith(prefix)) return Task.CompletedTask;
        var cmd = e.Message.Content.AsMemory(prefix.Length);

        foreach (var pair in handlers)
        {
            if (!cmd.Span.StartsWith(pair.Key)) continue;
            if (cmd.Length > pair.Key.Length && !char.IsWhiteSpace(cmd.Span[pair.Key.Length])) continue;
            pair.Value(cmd[pair.Key.Length..].Trim(), e.Message).ContinueWith(task =>
            {
                if (task.Exception is null) return;
                logger.LogError(task.Exception, "Command '{Command}' thrown an exception", pair.Key);
            });
            break;
        }
        return Task.CompletedTask;
    }
}
