namespace EasySave.EasyLog.Options;
    /// <summary>
    /// Supported log serialization formats.
    /// </summary>

public enum LogFormat
{
    Json,
    Xml
}

public static class LogFormatExtensions
{
    public static LogFormat[] GetValues() =>
        (LogFormat[])Enum.GetValues(typeof(LogFormat));
}

