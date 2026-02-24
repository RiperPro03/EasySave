using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Options;
using EasySave.EasyLog.WebSockets;

namespace EasySave.EasyLog.Readers
{
    /// <summary>
    /// Reads log entries from LogHub over WebSocket.
    /// </summary>
    internal sealed class WebSocketLogReader<T> : ILogReader<T>
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly LogHubWebSocketClient _client;

        /// <summary>
        /// Initializes a new remote reader using LogHub WebSocket transport.
        /// </summary>
        /// <param name="serverOptions">Remote server connection settings.</param>
        public WebSocketLogReader(LogServerOptions serverOptions)
        {
            _client = new LogHubWebSocketClient(serverOptions ?? throw new ArgumentNullException(nameof(serverOptions)));
        }

        /// <summary>
        /// Gets an empty local directory value because remote readers are not file-based.
        /// </summary>
        public string LogDirectory => string.Empty;

        /// <summary>
        /// Returns an empty file list because remote readers do not expose local files.
        /// </summary>
        /// <param name="maxFiles">Unused for remote readers.</param>
        /// <returns>An empty list.</returns>
        public IReadOnlyList<string> GetLogFiles(int? maxFiles = null)
            => Array.Empty<string>();

        /// <summary>
        /// Reads and deserializes remote entries from a limited number of files on the server.
        /// </summary>
        /// <param name="maxFiles">Maximum number of server files to read.</param>
        /// <returns>A read-only list of parsed entries.</returns>
        public IReadOnlyList<T> ReadEntries(int maxFiles = 7)
        {
            try
            {
                return Deserialize(_client.ReadEntries(maxFiles, readAll: false));
            }
            catch
            {
                // En lecture distante best-effort, on degrade en liste vide si le serveur est indisponible.
                return Array.Empty<T>();
            }
        }

        /// <summary>
        /// Reads and deserializes all remote entries available on the server.
        /// </summary>
        /// <returns>A read-only list of parsed entries.</returns>
        public IReadOnlyList<T> ReadAllEntries()
        {
            try
            {
                return Deserialize(_client.ReadEntries(null, readAll: true));
            }
            catch
            {
                // Meme comportement pour eviter de faire tomber l'UI si LogHub est eteint.
                return Array.Empty<T>();
            }
        }

        /// <summary>
        /// Remote readers do not support file path based reads.
        /// </summary>
        /// <param name="filePath">Unused remote file path.</param>
        /// <returns>This method always throws.</returns>
        /// <exception cref="NotSupportedException">Always thrown for remote readers.</exception>
        public IReadOnlyList<T> ReadEntriesFromFile(string filePath)
            => throw new NotSupportedException("Remote log reader does not support file-based reads.");

        private static IReadOnlyList<T> Deserialize(IReadOnlyList<RemoteLogEntry> remoteEntries)
        {
            var results = new List<T>();
            foreach (RemoteLogEntry remoteEntry in remoteEntries)
            {
                foreach (T entry in DeserializeEntry(remoteEntry))
                {
                    results.Add(entry);
                }
            }

            return results;
        }

        private static IEnumerable<T> DeserializeEntry(RemoteLogEntry remoteEntry)
        {
            string extension = remoteEntry.Extension;
            string content = remoteEntry.SerializedEntry ?? string.Empty;
            var items = new List<T>();

            if (string.Equals(extension, "json", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
            {
                string trimmed = content.Trim();
                // Le serveur renvoie une entree serialisee par item; on filtre les valeurs vides defensivement.
                if (trimmed.Length == 0
                    || trimmed.Equals("<logs>", StringComparison.OrdinalIgnoreCase)
                    || trimmed.Equals("</logs>", StringComparison.OrdinalIgnoreCase))
                {
                    return items;
                }

                T? entry;
                try
                {
                    entry = JsonSerializer.Deserialize<T>(trimmed, JsonOptions);
                }
                catch (JsonException)
                {
                    return items;
                }

                if (entry is not null)
                {
                    items.Add(entry);
                }

                return items;
            }

            if (string.Equals(extension, "xml", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase))
            {
                XElement element;
                try
                {
                    // Chaque item XML distant est attendu comme fragment XML autonome.
                    element = XElement.Parse(content);
                }
                catch (Exception)
                {
                    return items;
                }

                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                T? parsed = default;
                try
                {
                    using var reader = element.CreateReader();
                    object? value = serializer.Deserialize(reader);
                    if (value is T typed)
                    {
                        parsed = typed;
                    }
                }
                catch (InvalidOperationException)
                {
                    return items;
                }

                if (parsed is not null)
                {
                    items.Add(parsed);
                }
            }

            return items;
        }
    }
}
