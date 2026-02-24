using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Loggers;
using EasySave.EasyLog.Options;
using EasySave.EasyLog.Serialization;
using EasySave.EasyLog.Writers;

namespace EasySave.EasyLog.Factories
{
    /// <summary>
    /// Creates logger instances from configuration options.
    /// Pattern "Factory"
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
        /// Thrown when local/server options required by the selected storage mode are missing.
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

            ILogSerializer serializer = options.Format switch
            {
                LogFormat.Json => new JsonSerializer(),
                LogFormat.Xml => new XmlSerializer(),
                _ => throw new ArgumentOutOfRangeException(nameof(options), options.Format, "Unknown log format.")
            };

            ILogger<T> logger = CreateLogger<T>(options, serializer);

            return options.UseSafeLogger ? new SafeLogger<T>(logger) : logger;
        }

        private static ILogger<T> CreateLogger<T>(LogOptions options, ILogSerializer serializer)
        {
            return options.StorageMode switch
            {
                LogStorageMode.LocalOnly => CreateLocalLogger<T>(options, serializer),
                LogStorageMode.ServerOnly => CreateServerLogger<T>(options, serializer),
                LogStorageMode.LocalAndServer => new LocalAndServerLogger<T>(
                    CreateLocalLogger<T>(options, serializer),
                    CreateServerLogger<T>(options, serializer)),
                _ => throw new ArgumentOutOfRangeException(nameof(options), options.StorageMode, "Unknown storage mode.")
            };
        }

        private static ILogger<T> CreateLocalLogger<T>(LogOptions options, ILogSerializer serializer)
        {
            if (string.IsNullOrWhiteSpace(options.LogDirectory))
            {
                throw new ArgumentException("Log directory is required for local log writing.", nameof(options));
            }

            ILogWriter writer = new FileLogWriter();
            return new DailyLogger<T>(options.LogDirectory, serializer, writer);
        }

        private static ILogger<T> CreateServerLogger<T>(LogOptions options, ILogSerializer serializer)
        {
            if (options.Server is null)
            {
                throw new ArgumentException("Server options are required for remote log writing.", nameof(options));
            }

            return new WebSocketLogger<T>(serializer, options.Server);
        }

        /// <summary>
        /// Writes entries to both local and remote loggers for the LocalAndServer mode.
        /// </summary>
        /// <typeparam name="TEntry">The log entry type.</typeparam>
        private sealed class LocalAndServerLogger<TEntry> : ILogger<TEntry>
        {
            private readonly ILogger<TEntry> _localLogger;
            private readonly ILogger<TEntry> _remoteLogger;

            /// <summary>
            /// Initializes a new local-first mirrored logger wrapper.
            /// </summary>
            /// <param name="localLogger">Local logger used for the primary write path.</param>
            /// <param name="remoteLogger">Remote logger used for best-effort mirroring.</param>
            public LocalAndServerLogger(ILogger<TEntry> localLogger, ILogger<TEntry> remoteLogger)
            {
                _localLogger = localLogger ?? throw new ArgumentNullException(nameof(localLogger));
                _remoteLogger = remoteLogger ?? throw new ArgumentNullException(nameof(remoteLogger));
            }

            /// <summary>
            /// Writes the same entry to all configured loggers.
            /// </summary>
            /// <param name="entry">Entry to write.</param>
            /// <returns>
            /// <c>true</c> when the local write succeeds; otherwise <c>false</c>.
            /// </returns>
            public bool Write(TEntry entry)
            {
                bool localSuccess;
                try
                {
                    localSuccess = _localLogger.Write(entry);
                }
                catch
                {
                    localSuccess = false;
                }

                // En mode LocalAndServer, on ne bloque pas l'appelant sur un serveur indisponible.
                // ne doit pas casser l'ecriture locale.
                _ = Task.Run(() =>
                {
                    try
                    {
                        _remoteLogger.Write(entry);
                    }
                    catch
                    {
                        // Ignore: le mode local doit continuer a fonctionner meme sans serveur.
                    }
                });

                return localSuccess;
            }
        }
    }
}
