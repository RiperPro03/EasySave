namespace EasySave.Core.Logging;

/// <summary>
/// Provides default context values for log entries.
/// </summary>
public sealed class LogContext
{
    public string AppName { get; set; } = "EasySave";
    public string AppVersion { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int? ProcessId { get; set; }
}
