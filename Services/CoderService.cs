using DSharpPlus.Entities;
using System.Xml;

namespace CodeExecutor.Services;

internal class CoderService
{
    private const string JavaMainClass = "Program";

    private readonly ExecuteService execute;
    private readonly Options options;

    public CoderService(ExecuteService execute, Options options)
    {
        this.execute = execute;
        this.options = options;
    }

    public async Task PrintVersions(ReadOnlyMemory<char> text, DiscordMessage message)
    {
        var pythonTask = execute.Execute("python3", "--version");
        var cTask = execute.Execute("gcc", "--version");
        var javaTask = execute.Execute("java", "--version");
        var (pythonInfo, cInfo, javaInfo) = (await pythonTask, await cTask, await javaTask);
        var dotnetInfo = Environment.Version.ToString();

        await message.RespondAsync($"""
            {pythonInfo?.StdOut.ReadLine() ?? "~~Python недоступен~~"}
            .NET {dotnetInfo}
            {cInfo?.StdOut.ReadLine() ?? "~~GCC недоступен~~"}
            {javaInfo?.StdOut.ReadLine() ?? "~~Java недоступна~~"}
            """);
    }

    public async Task ExecutePython(ReadOnlyMemory<char> text, DiscordMessage message)
    {
        if (text.Span.StartsWith("```python") && text.Span.EndsWith("```")) text = text[9..^3].Trim();
        else if (text.Span.StartsWith("```py") && text.Span.EndsWith("```")) text = text[5..^3].Trim();
        var msg = await message.RespondAsync($"Выполняем... (таймаут {options.ExecuteProgramTimeoutMillis / 1000} секунд)");

        var path = Path.Combine(options.ExecuteWorkingDirectory, $"python{Environment.TickCount64}temp.py");
        await WriteFile(path, text);

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
        await WriteFile(srcPath, text);

        var projectPath = Path.Combine(projectDir, $"{dir}.csproj");
        var projectXml = new XmlDocument();
        projectXml.Load(projectPath);
        var blocks = projectXml.CreateElement("AllowUnsafeBlocks");
        blocks.InnerText = "true";
        projectXml["Project"]!["PropertyGroup"]!.AppendChild(blocks);
        projectXml.Save(projectPath);

        var runRes = await execute.Execute("dotnet", $"run -c Release --project {projectPath}");
        await msg.ModifyAsync(FormatResult(runRes));

        Directory.Delete(projectDir, true);
    }

    public async Task ExecuteC(ReadOnlyMemory<char> text, DiscordMessage message)
    {
        if (text.Span.StartsWith("```c") && text.Span.EndsWith("```")) text = text[4..^3].Trim();
        var msg = await message.RespondAsync($"Компилируем... (таймаут {options.ExecuteProgramTimeoutMillis / 1000} секунд)");

        var srcPath = Path.Combine(options.ExecuteWorkingDirectory, $"c{Environment.TickCount64}temp.c");
        var exePath = Path.Combine(options.ExecuteWorkingDirectory, $"c{Environment.TickCount64}temp");

        await WriteFile(srcPath, text);
        var compileResult = await execute.Execute("gcc", $"-o {exePath} {srcPath}");
        if(!compileResult.HasValue || compileResult.Value.ExitCode != 0)
        {
            await msg.ModifyAsync($"Компиляция не удалась:\r\n{FormatResult(compileResult)}");
            File.Delete(srcPath);
            File.Delete(exePath);
            return;
        }

        await msg.ModifyAsync($"Запускаем... (таймаут {options.ExecuteProgramTimeoutMillis / 1000} секунд)");
        var runResult = await execute.Execute(exePath, "");
        await msg.ModifyAsync(FormatResult(runResult));
        File.Delete(srcPath);
        File.Delete(exePath);
    }

    public async Task ExecuteJava(ReadOnlyMemory<char> text, DiscordMessage message)
    {
        if (text.Span.StartsWith("```java") && text.Span.EndsWith("```")) text = text[7..^3].Trim();
        var msg = await message.RespondAsync($"Запускаем... (таймаут {options.ExecuteProgramTimeoutMillis / 1000} секунд)");

        var folder = Path.Combine(options.ExecuteWorkingDirectory, $"java{Environment.TickCount64}temp");
        var srcPath = Path.Combine(folder, $"{JavaMainClass}.java");

        Directory.CreateDirectory(folder);
        await WriteFile(srcPath, text);

        var runResult = await execute.Execute("java", srcPath);
        await msg.ModifyAsync(FormatResult(runResult));
        Directory.Delete(folder, true);
    }

    public async Task PrintHelp(ReadOnlyMemory<char> _, DiscordMessage message)
    {
        var p = $"{options.CommandPrefix}exec";
        await message.RespondAsync($"""
            `{p} help` - отображает данную справку
            `{p} info` - отображает сведения об используемых средах
            `{p} python` - выполняет Python код
            `{p} csharp` - выполняет C# код
            `{p} java` - выполняет Java код (главный класс - `{JavaMainClass}`)
            `{p} c` - выполняет C код
            """);
    }

    private string FormatResult((int code, StreamReader output, StreamReader error)? data)
    {
        if (!data.HasValue) return "Не выполнилось (таймаут или иная ошибка)";

        var output = data.Value.output.ReadToEnd();
        var error = data.Value.error.ReadToEnd();
        return $"""
                Код возврата: `{data.Value.code}`
                stdout: {WrapTextBlock(output)}
                stderr: {WrapTextBlock(error)}
                """;

        static string WrapTextBlock(string? text) => string.IsNullOrWhiteSpace(text) ? "_ничего_" : $"```{text}```";
    }

    private static async Task WriteFile(string path, ReadOnlyMemory<char> data)
    {
        using var stream = new StreamWriter(path, false);
        await stream.WriteAsync(data);
    }
}
