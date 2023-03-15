namespace CodeExecutor;

internal class Options
{
    public string CommandPrefix { get; set; }
    public string BotToken { get; set; }
    public string ExecuteUserName { get; set; }
    public string ExecuteWorkingDirectory { get; set; }
    public int ExecuteProgramTimeoutMillis { get; set; }
}