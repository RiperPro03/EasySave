using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Loggers;
using EasySave.EasyLog.Options;
using EasySave.EasyLog.Serialization;
using EasySave.EasyLog.Writers;

namespace EasySave.EasyLog.Factories
{
    /// <summary>
    /// Creates logger instances from configuration options.
    /// </summary>
    public static class LoggerFactory
    {
        /// <summary>
        /// Builds a logger for the specified entry type.
        /// </summary>
        /// <typeparam name="T">The log entry type.</typeparam>
        /// <param name="options">Logger configuration options.</param>
        /// <returns>A configured logger instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <see cref="LogOptions.LogDirectory"/> is missing.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the log format is unknown.
        /// </exception>
        public static ILogger<T> Create<T>(LogOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.LogDirectory))
            {
                throw new ArgumentException("Log directory is required.", nameof(options));
            }

            ILogSerializer serializer = options.Format switch
            {
                LogFormat.Json => new JsonSerializer(),
                LogFormat.Xml => new XmlSerializer(),
                _ => throw new ArgumentOutOfRangeException(nameof(options), options.Format, "Unknown log format.")
            };

            ILogWriter writer = new FileLogWriter();
            ILogger<T> logger = new DailyLogger<T>(options.LogDirectory, serializer, writer);

            return options.UseSafeLogger ? new SafeLogger<T>(logger) : logger;
        }
    }
}
