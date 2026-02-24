namespace EasySave.EasyLog.Options
{
    /// <summary>
    /// Configuration for remote log transport.
    /// </summary>
    public sealed class LogServerOptions
    {
        /// <summary>
        /// Optional full WebSocket endpoint URL (ws:// or wss://).
        /// When empty, EasyLog builds one from Host/Port/WebSocketPath.
        /// </summary>
        public string WebSocketUrl { get; init; } = string.Empty;

        /// <summary>
        /// Gets the server host or IP address used when WebSocketUrl is not provided.
        /// </summary>
        public string Host { get; init; } = "localhost";

        /// <summary>
        /// Gets the server port used when WebSocketUrl is not provided.
        /// </summary>
        public int Port { get; init; } = 9696;

        /// <summary>
        /// Gets the server WebSocket path used when WebSocketUrl is not provided.
        /// </summary>
        public string WebSocketPath { get; init; } = "/ws/logs";

        /// <summary>
        /// Gets whether to use TLS (wss) when WebSocketUrl is not provided.
        /// </summary>
        public bool UseTls { get; init; }

        /// <summary>
        /// Gets the connection timeout in milliseconds.
        /// </summary>
        public int ConnectTimeoutMs { get; init; } = 5000;

        /// <summary>
        /// Gets the receive timeout in milliseconds.
        /// </summary>
        public int ReceiveTimeoutMs { get; init; } = 10000;

        /// <summary>
        /// Optional bearer token sent as Authorization header.
        /// </summary>
        public string? BearerToken { get; init; }
    }
}
