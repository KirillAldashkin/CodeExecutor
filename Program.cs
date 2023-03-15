using CodeExecutor;
using CodeExecutor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(s => s
        .AddSingleton(sp => sp.GetRequiredService<IConfiguration>().Get<Options>()!)
        .AddHostedService<EntryService>()
        .AddSingleton<DiscordService>()
        .AddSingleton<FunService>()
        .AddSingleton<CoderService>()
        .AddSingleton<ExecuteService>())
    .RunConsoleAsync();
