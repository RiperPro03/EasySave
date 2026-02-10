namespace EasySave.EasyLog.Options
{
    /// <summary>
    /// Configuration options for loggers.
    /// </summary>
    public sealed class LogOptions
    {
        /// <summary>
        /// Gets the directory where logs are stored.
        /// </summary>
        public string LogDirectory { get; init; } = "logs";

        /// <summary>
        /// Gets the log serialization format.
        /// </summary>
        public LogFormat Format { get; init; } = LogFormat.Json;

        /// <summary>
        /// Gets a value indicating whether to wrap the logger in a safe wrapper.
        /// </summary>
        public bool UseSafeLogger { get; init; } = true;
    }
}
