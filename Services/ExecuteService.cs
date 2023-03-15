using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CodeExecutor.Services;

internal class ExecuteService
{
    private readonly Options options;
    private readonly ILogger<ExecuteService> logger;

    public ExecuteService(Options options, ILogger<ExecuteService> logger)
    {
        this.options = options;
        this.logger = logger;
    }

    public async Task<(int ExitCode, StreamReader StdOut, StreamReader StdErr)?> Execute(string path, string args)
    {
        logger.LogDebug("Starting {Path} {Arg}", path, args);
        var info = new ProcessStartInfo(path, args)
        {
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UserName = options.ExecuteUserName,
            WorkingDirectory = options.ExecuteWorkingDirectory
        };
        if(Environment.OSVersion.Platform == PlatformID.Unix)
        {
            info.EnvironmentVariables["USER"] = options.ExecuteUserName;
            info.EnvironmentVariables["HOME"] = $"/home/{options.ExecuteUserName}";
        }
        var proc = Process.Start(info);
        if(proc is null)
        {
            logger.LogError("Could not start {Path} {Arg}: '{VarName}' is null", path, args, nameof(proc));
            return null;
        }
        var timeout = new CancellationTokenSource(options.ExecuteProgramTimeoutMillis).Token;
        try
        {
            await proc.WaitForExitAsync(timeout);
        }
        catch (Exception)
        {
            return null;
        }
        if (!proc.HasExited) proc.Kill(true);

        logger.LogDebug("Program {Path} exited with code {Code}", path, proc.ExitCode);
        return (proc.ExitCode, proc.StandardOutput, proc.StandardError);
    }
}