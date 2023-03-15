using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CodeExecutor.Services;

internal class EntryService : IHostedService
{
    private readonly DiscordService discord;
    private readonly ILogger<EntryService> logger;
    private readonly Options options;

    public EntryService(DiscordService discord, ILogger<EntryService> logger, Options options, CoderService coder)
    {
        this.discord = discord;
        this.logger = logger;
        this.options = options;
    }

    public async Task StartAsync(CancellationToken cancellation)
    {
        await discord.Connect();
    }

    public async Task StopAsync(CancellationToken cancellation)
    {
        await discord.Disconnect();
    }
}