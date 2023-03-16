namespace CodeExecutor;

#pragma warning disable 8618
internal class Options
{
    public string CommandPrefix { get; set; }
    public string BotToken { get; set; }
    public string ExecuteUserName { get; set; }
    public string ExecuteWorkingDirectory { get; set; }
    public int ExecuteProgramTimeoutMillis { get; set; }
}
#pragma warning restore 8618