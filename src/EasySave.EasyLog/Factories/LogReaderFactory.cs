using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Options;
using EasySave.EasyLog.Readers;

namespace EasySave.EasyLog.Factories
{
    /// <summary>
    /// Creates log readers from configuration options.
    /// </summary>
    public static class LogReaderFactory
    {
        /// <summary>
        /// Builds a reader for the specified entry type.
        /// </summary>
        /// <typeparam name="T">The log entry type.</typeparam>
        /// <param name="options">Reader configuration options.</param>
        /// <returns>A configured log reader.</returns>
        public static ILogReader<T> Create<T>(LogOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return options.StorageMode switch
            {
                LogStorageMode.LocalOnly => CreateLocalReader<T>(options),
                LogStorageMode.ServerOnly => CreateServerReader<T>(options),
                LogStorageMode.LocalAndServer => new LocalAndServerLogReader<T>(
                    CreateLocalReader<T>(options),
                    CreateServerReader<T>(options)),
                _ => throw new ArgumentOutOfRangeException(nameof(options), options.StorageMode, "Unknown storage mode.")
            };
        }

        private static ILogReader<T> CreateLocalReader<T>(LogOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.LogDirectory))
            {
                throw new ArgumentException("Log directory is required for local log reading.", nameof(options));
            }

            return new LocalFileLogReader<T>(options.LogDirectory);
        }

        private static ILogReader<T> CreateServerReader<T>(LogOptions options)
        {
            if (options.Server is null)
            {
                throw new ArgumentException("Server options are required for remote log reading.", nameof(options));
            }

            return new WebSocketLogReader<T>(options.Server);
        }

        /// <summary>
        /// Aggregates local and remote readers for the LocalAndServer mode.
        /// </summary>
        /// <typeparam name="TEntry">The log entry type.</typeparam>
        private sealed class LocalAndServerLogReader<TEntry> : ILogReader<TEntry>
        {
            private readonly ILogReader<TEntry> _localReader;
            private readonly ILogReader<TEntry> _remoteReader;

            /// <summary>
            /// Initializes a new aggregated reader.
            /// </summary>
            /// <param name="localReader">Local file reader.</param>
            /// <param name="remoteReader">Remote WebSocket reader.</param>
            public LocalAndServerLogReader(ILogReader<TEntry> localReader, ILogReader<TEntry> remoteReader)
            {
                _localReader = localReader ?? throw new ArgumentNullException(nameof(localReader));
                _remoteReader = remoteReader ?? throw new ArgumentNullException(nameof(remoteReader));
            }

            public string LogDirectory => _localReader.LogDirectory;

            public IReadOnlyList<string> GetLogFiles(int? maxFiles = null)
                => _localReader.GetLogFiles(maxFiles);

            public IReadOnlyList<TEntry> ReadEntries(int maxFiles = 7)
            {
                var items = new List<TEntry>();
                // Concat simple: local d'abord, remote ensuite (pas de tri global ici).
                items.AddRange(_localReader.ReadEntries(maxFiles));
                try
                {
                    items.AddRange(_remoteReader.ReadEntries(maxFiles));
                }
                catch
                {
                    // En mode mixte, un echec distant ne doit jamais casser la lecture locale.
                }
                return items;
            }

            public IReadOnlyList<TEntry> ReadAllEntries()
            {
                var items = new List<TEntry>();
                // Meme strategie de fusion pour la lecture complete.
                items.AddRange(_localReader.ReadAllEntries());
                try
                {
                    items.AddRange(_remoteReader.ReadAllEntries());
                }
                catch
                {
                    // Fallback local uniquement si le serveur est indisponible.
                }
                return items;
            }

            public IReadOnlyList<TEntry> ReadEntriesFromFile(string filePath)
                => _localReader.ReadEntriesFromFile(filePath);
        }
    }
}
