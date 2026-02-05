namespace EasySave.EasyLog.Options
{
    public sealed class LogOptions
    {
        public string LogDirectory { get; init; } = "logs";
        public LogFormat Format { get; init; } = LogFormat.Json;
        public bool UseSafeLogger { get; init; } = true;
    }
}
