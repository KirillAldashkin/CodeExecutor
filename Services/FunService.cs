using DSharpPlus.Entities;

namespace CodeExecutor.Services;

internal class FunService
{
    public FunService(DiscordService discord)
    {
        discord.AddHandler("шанс", GetChance);
    }

    private async Task GetChance(ReadOnlyMemory<char> text, DiscordMessage message)
    {
        var what = string.Create(text.Length, text, (s, m) => m.Span.CopyTo(s));
        var chance = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1000);
        await message.RespondAsync($"Шанс того, что {what} составляет {chance/10}.{chance%10}%");
    }
}
