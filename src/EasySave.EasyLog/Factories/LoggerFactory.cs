using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Loggers;
using EasySave.EasyLog.Options;
using EasySave.EasyLog.Serialization;
using EasySave.EasyLog.Writers;

namespace EasySave.EasyLog.Factories
{
    /// <summary>
    /// Cette classe statique implémente le pattern "Factory" pour créer des loggers configurés
    /// </summary>
    public static class LoggerFactory
    {
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
