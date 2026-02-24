using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.EasyLog.Options;

namespace EasySave.EasyLog.WebSockets
{
    /// <summary>
    /// Minimal request/response WebSocket client for LogHub.
    /// </summary>
    internal sealed class LogHubWebSocketClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly LogServerOptions _options;
        private readonly Uri _serverUri;

        /// <summary>
        /// Initializes a new LogHub WebSocket client.
        /// </summary>
        /// <param name="options">Remote server connection settings.</param>
        public LogHubWebSocketClient(LogServerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _serverUri = BuildUri(_options);
        }

        /// <summary>
        /// Sends a remote write request.
        /// </summary>
        /// <param name="extension">Serialized entry extension (json/xml).</param>
        /// <param name="serializedEntry">Serialized entry payload.</param>
        /// <param name="timestampUtc">UTC timestamp used by the server for daily partitioning.</param>
        /// <returns><c>true</c> when the server acknowledges the write.</returns>
        public bool SendWrite(string extension, string serializedEntry, DateTime timestampUtc)
        {
            var request = new LogHubRequest
            {
                Operation = "write",
                Extension = extension,
                SerializedEntry = serializedEntry,
                TimestampUtc = timestampUtc
            };

            LogHubResponse response = Send(request);
            return response.Success;
        }

        /// <summary>
        /// Sends a remote read request and returns serialized entries.
        /// </summary>
        /// <param name="maxFiles">Maximum number of files to read when <paramref name="readAll"/> is false.</param>
        /// <param name="readAll">Whether the server should read all files.</param>
        /// <returns>A read-only list of serialized remote entries.</returns>
        public IReadOnlyList<RemoteLogEntry> ReadEntries(int? maxFiles, bool readAll)
        {
            var request = new LogHubRequest
            {
                Operation = "read",
                MaxFiles = readAll ? null : maxFiles,
                ReadAll = readAll
            };

            LogHubResponse response = Send(request);
            if (!response.Success || response.Entries is null)
            {
                return Array.Empty<RemoteLogEntry>();
            }

            return response.Entries;
        }

        private LogHubResponse Send(LogHubRequest request)
        {
            using var socket = new ClientWebSocket();
            if (!string.IsNullOrWhiteSpace(_options.BearerToken))
            {
                // Header optionnel pour securiser le serveur plus tard (Bearer token).
                socket.Options.SetRequestHeader("Authorization", $"Bearer {_options.BearerToken}");
            }

            using var connectCts = new CancellationTokenSource(_options.ConnectTimeoutMs);
            // Mode request/response simple: une connexion WS par operation.
            socket.ConnectAsync(_serverUri, connectCts.Token).GetAwaiter().GetResult();
            try
            {
                string requestJson = JsonSerializer.Serialize(request, JsonOptions);
                byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);
                // Envoi d'un message texte JSON unique.
                socket.SendAsync(
                        new ArraySegment<byte>(requestBytes),
                        WebSocketMessageType.Text,
                        endOfMessage: true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                string responseJson = ReceiveText(socket, _options.ReceiveTimeoutMs);
                try
                {
                    return JsonSerializer.Deserialize<LogHubResponse>(responseJson, JsonOptions) ?? new LogHubResponse
                    {
                        Success = false,
                        Error = "Invalid server response."
                    };
                }
                catch (JsonException ex)
                {
                    return new LogHubResponse { Success = false, Error = ex.Message };
                }
            }
            finally
            {
                TryCloseGracefully(socket);
            }
        }

        private static string ReceiveText(ClientWebSocket socket, int timeoutMs)
        {
            using var receiveCts = new CancellationTokenSource(timeoutMs);
            byte[] buffer = new byte[8192];
            using var ms = new MemoryStream();

            while (true)
            {
                // Accumule les frames WS jusqu'a la fin du message.
                WebSocketReceiveResult result = socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        receiveCts.Token)
                    .GetAwaiter()
                    .GetResult();

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                ms.Write(buffer, 0, result.Count);
                if (result.EndOfMessage)
                {
                    break;
                }
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static Uri BuildUri(LogServerOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.WebSocketUrl))
            {
                // Priorite a l'URL complete si fournie explicitement.
                return new Uri(options.WebSocketUrl);
            }

            if (string.IsNullOrWhiteSpace(options.Host))
            {
                throw new ArgumentException("Server host is required when WebSocketUrl is not provided.", nameof(options));
            }

            if (options.Port <= 0 || options.Port > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(options), options.Port, "Port must be between 1 and 65535.");
            }

            string scheme = options.UseTls ? "wss" : "ws";
            string path = string.IsNullOrWhiteSpace(options.WebSocketPath) ? "/ws/logs" : options.WebSocketPath;
            if (!path.StartsWith("/", StringComparison.Ordinal))
            {
                path = "/" + path;
            }

            return new Uri($"{scheme}://{options.Host}:{options.Port}{path}");
        }

        private static void TryCloseGracefully(ClientWebSocket socket)
        {
            try
            {
                if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    // Close handshake pour eviter les "remote party closed without completing handshake" cote serveur.
                    socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }
            }
            catch
            {
                // Ignore: la socket va etre disposee juste apres.
            }
        }

        /// <summary>
        /// Represents a LogHub protocol request sent over WebSocket.
        /// </summary>
        internal sealed class LogHubRequest
        {
            /// <summary>
            /// Gets or sets the protocol operation name.
            /// </summary>
            public string Operation { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the serialized payload extension for write operations.
            /// </summary>
            public string? Extension { get; set; }

            /// <summary>
            /// Gets or sets the serialized payload for write operations.
            /// </summary>
            public string? SerializedEntry { get; set; }

            /// <summary>
            /// Gets or sets the UTC timestamp for write operations.
            /// </summary>
            public DateTime? TimestampUtc { get; set; }

            /// <summary>
            /// Gets or sets the maximum number of files to read.
            /// </summary>
            public int? MaxFiles { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether all files should be read.
            /// </summary>
            public bool ReadAll { get; set; }
        }

        /// <summary>
        /// Represents a LogHub protocol response received over WebSocket.
        /// </summary>
        internal sealed class LogHubResponse
        {
            /// <summary>
            /// Gets or sets a value indicating whether the request succeeded.
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// Gets or sets the error message when the request fails.
            /// </summary>
            public string? Error { get; set; }

            /// <summary>
            /// Gets or sets serialized entries for read operations.
            /// </summary>
            public List<RemoteLogEntry>? Entries { get; set; }
        }
    }

    /// <summary>
    /// Represents a serialized entry returned by the remote LogHub server.
    /// </summary>
    internal sealed class RemoteLogEntry
    {
        /// <summary>
        /// Gets or sets the serialized payload extension (json/xml).
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the serialized entry payload.
        /// </summary>
        public string SerializedEntry { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source file name when provided by the server.
        /// </summary>
        public string? FileName { get; set; }
    }
}
