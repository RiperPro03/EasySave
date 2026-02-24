using System.Net.WebSockets;
using System.Text;

namespace LogHub.Server.WebSockets;

/// <summary>
/// Reads complete text messages from a WebSocket connection.
/// </summary>
public static class WebSocketMessageReader
{
    /// <summary>
    /// Reads a complete text message, handling fragmented frames.
    /// Returns <see langword="null"/> when the socket receives a close frame.
    /// </summary>
    /// <param name="socket">The source WebSocket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The text payload, or <see langword="null"/> on close.</returns>
    public static async Task<string?> ReadTextAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(socket);

        byte[] buffer = new byte[8192];
        using var ms = new MemoryStream();

        while (true)
        {
            WebSocketReceiveResult result;
            try
            {
                // Accumule les frames jusqu'a la fin du message WebSocket.
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            }
            catch (WebSocketException)
            {
                // Le client peut couper la socket sans close handshake complet (app kill/timeout/retry).
                // On traite cela comme une fin de connexion normale pour eviter un 500 serveur.
                return null;
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return null;
            }

            ms.Write(buffer, 0, result.Count);
            if (result.EndOfMessage)
            {
                break;
            }
        }

        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
