using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Options;
using EasySave.EasyLog.WebSockets;

namespace EasySave.EasyLog.Loggers
{
    /// <summary>
    /// Sends serialized log entries to LogHub over WebSocket.
    /// </summary>
    internal sealed class WebSocketLogger<T> : ILogger<T>
    {
        private readonly ILogSerializer _serializer;
        private readonly LogHubWebSocketClient _client;
        private readonly Func<DateTime> _timestampProvider;

        /// <summary>
        /// Initializes a new WebSocket-backed logger.
        /// </summary>
        /// <param name="serializer">Serializer used before transport.</param>
        /// <param name="serverOptions">Remote server connection settings.</param>
        /// <param name="timestampProvider">Optional timestamp provider for testing.</param>
        public WebSocketLogger(
            ILogSerializer serializer,
            LogServerOptions serverOptions,
            Func<DateTime>? timestampProvider = null)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _client = new LogHubWebSocketClient(serverOptions ?? throw new ArgumentNullException(nameof(serverOptions)));
            _timestampProvider = timestampProvider ?? (() => DateTime.UtcNow);
        }

        /// <summary>
        /// Serializes and sends an entry to the remote LogHub server.
        /// </summary>
        /// <param name="entry">Entry to write.</param>
        /// <returns><c>true</c> when the remote write succeeds; otherwise <c>false</c>.</returns>
        public bool Write(T entry)
        {
            if (entry is null)
            {
                return false;
            }

            string serializedEntry = _serializer.Serialize(entry);
            string extension = _serializer.FileExtension;
            DateTime timestampUtc = _timestampProvider();
            if (timestampUtc.Kind != DateTimeKind.Utc)
            {
                // Normalise en UTC pour la partition journaliere cote serveur.
                timestampUtc = timestampUtc.ToUniversalTime();
            }

            try
            {
                return _client.SendWrite(extension, serializedEntry, timestampUtc);
            }
            catch
            {
                // Le logger "safe" peut envelopper celui-ci; ici on reste best-effort.
                return false;
            }
        }
    }
}
