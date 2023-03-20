using DSharpPlus.Entities;

namespace CodeExecutor.Services;

internal class FunService
{
    public CommandsService.CommandHandler GetChance(string prefix) => async (text, message) =>
    {
        var chance = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1000);
        await message.RespondAsync($"{prefix} {text} составляет {chance / 10}.{chance % 10}%");
    };

    public async Task WhoIs(ReadOnlyMemory<char> text, DiscordMessage message)
    {
        var members = message.Channel.Guild.Members;
        var i = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, members.Count);
        var user = members.Skip(i).First().Value;
        await message.RespondAsync($"{user.DisplayName} {text}");
    }
}
