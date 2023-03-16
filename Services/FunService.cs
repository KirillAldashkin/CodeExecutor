using DSharpPlus.Entities;

namespace CodeExecutor.Services;

internal class FunService
{
    public CommandsService.CommandHandler GetChance(string prefix) => async (text, message) =>
    {
        var what = string.Create(text.Length, text, (s, m) => m.Span.CopyTo(s));
        var chance = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1000);
        await message.RespondAsync($"{prefix} {what} составляет {chance / 10}.{chance % 10}%");
    };
}
