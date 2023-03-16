using DSharpPlus.Entities;

namespace CodeExecutor.Services;

internal class CoderService
{
    private readonly ExecuteService execute;
    private readonly Options options;

    public CoderService(ExecuteService execute, Options options)
    {
        this.execute = execute;
        this.options = options;
    }

    public async Task PrintVersions(ReadOnlyMemory<char> text, DiscordMessage message)
    {
        var pythonInfo = await execute.Execute("python3", "--version");
        var cInfo = await execute.Execute("gcc", "--version");
        var dotnetInfo = Environment.Version.ToString();

        await message.RespondAsync($"""
            {pythonInfo?.StdOut.ReadLine() ?? "~~Python недоступен~~"}
            .NET {dotnetInfo}
            {cInfo?.StdOut.ReadLine() ?? "~~GCC недоступен~~"}
            """);
    }

    public async Task ExecutePython(ReadOnlyMemory<char> text, DiscordMessage message)
    {
        if (text.Span.StartsWith("```python") && text.Span.EndsWith("```")) text = text[9..^3].Trim();
        else if (text.Span.StartsWith("```py") && text.Span.EndsWith("```")) text = text[5..^3].Trim();
        var msg = await message.RespondAsync($"Выполняем... (таймаут {options.ExecuteProgramTimeoutMillis / 1000} секунд)");

        var path = Path.Combine(Path.GetTempPath(), $"python{Environment.TickCount64}temp.py");
        var str = string.Create(text.Length, text, (s, m) => m.Span.CopyTo(s));
        File.WriteAllText(path, str);

        var res = await execute.Execute("python3", path);

        await msg.ModifyAsync(FormatResult(res));
        File.Delete(path);
    }

    public async Task ExecuteCSharp(ReadOnlyMemory<char> text, DiscordMessage message)
    {
        if (text.Span.StartsWith("```cs") && text.Span.EndsWith("```")) text = text[5..^3].Trim();
        var msg = await message.RespondAsync($"Компилируем...");

        var dir = $"dotnet{Environment.TickCount64}temp";
        var newProjectRes = await execute.Execute("dotnet", $"new console --no-restore -o {dir}");
        if(!newProjectRes.HasValue || newProjectRes.Value.ExitCode != 0)
        {
            await msg.ModifyAsync($"Не удалось создать временный проект\r\n{FormatResult(newProjectRes)}");
            return;
        }

        var projectDir = Path.Combine(options.ExecuteWorkingDirectory, dir);
        var srcPath = Path.Combine(projectDir, "Program.cs");
        var src = string.Create(text.Length, text, (s, m) => m.Span.CopyTo(s));
        File.WriteAllText(srcPath, src);

        var projectPath = Path.Combine(projectDir, $"{dir}.csproj");
        var runRes = await execute.Execute("dotnet", $"run -c Release --project {projectPath}");
        await msg.ModifyAsync(FormatResult(runRes));

        Directory.Delete(projectDir, true);
    }

    public async Task ExecuteC(ReadOnlyMemory<char> text, DiscordMessage message)
    {
        if (text.Span.StartsWith("```c") && text.Span.EndsWith("```")) text = text[4..^3].Trim();
        var msg = await message.RespondAsync($"Компилируем... (таймаут {options.ExecuteProgramTimeoutMillis / 1000} секунд)");

        var srcPath = Path.Combine(Path.GetTempPath(), $"c{Environment.TickCount64}temp.c");
        var exePath = Path.Combine(Path.GetTempPath(), $"c{Environment.TickCount64}temp");
        var src = string.Create(text.Length, text, (s, m) => m.Span.CopyTo(s));

        File.WriteAllText(srcPath, src);
        var compileResult = await execute.Execute("gcc", $"-o {exePath} {srcPath}");
        if(!compileResult.HasValue || compileResult.Value.ExitCode != 0)
        {
            await msg.ModifyAsync($"Компиляция не удалась:\r\n{FormatResult(compileResult)}");
            File.Delete(srcPath);
            File.Delete(exePath);
            return;
        }

        var runResult = await execute.Execute(exePath, "");
        await msg.ModifyAsync(FormatResult(runResult));
        File.Delete(srcPath);
        File.Delete(exePath);
    }

    public async Task PrintHelp(ReadOnlyMemory<char> text, DiscordMessage message)
    {
        var p = options.CommandPrefix;
        await message.RespondAsync($"""
            `{p}exec help` - отображает данную справку
            `{p}exec info` - отображает сведения об используемых средах
            `{p}exec python` - выполняет код на Python
            `{p}exec csharp` - выполняет код на C#
            `{p}exec c` - выполняет код на C
            """);
    }

    private string FormatResult((int code, StreamReader output, StreamReader error)? data)
    {
        if (!data.HasValue) return "Не выполнилось (таймаут или иная ошибка)";

        var output = data.Value.output.ReadToEnd();
        var error = data.Value.error.ReadToEnd();
        return $"""
                Код возврата: `{data.Value.code}`
                stdout: {(string.IsNullOrWhiteSpace(output) ? "_ничего_" : $"```{output}```")}
                stderr: {(string.IsNullOrWhiteSpace(error) ? "_ничего_" : $"```{error}```")}
                """;
    }
}
