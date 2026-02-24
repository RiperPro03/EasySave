using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using LogHub.Server.Contracts;
using LogHub.Server.Infrastructure.Queueing;
using LogHub.Server.Infrastructure.Storage;
using LogHub.Server.Options;
using Microsoft.Extensions.Options;

namespace LogHub.Server.WebSockets;

/// <summary>
/// Defines the LogHub WebSocket endpoint used for remote log read/write operations.
/// </summary>
public static class WebSocketEndpoint
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Maps the LogHub WebSocket endpoint to the provided path.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder.</param>
    /// <param name="path">WebSocket route path.</param>
    /// <returns>The same route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapLogHubWebSocket(this IEndpointRouteBuilder endpoints, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("WebSocket path is required.", nameof(path));
        }

        endpoints.MapMethods(path, new[] { "GET" }, HandleAsync);
        return endpoints;
    }

    private static async Task HandleAsync(
        HttpContext context,
        ChannelLogQueue queue,
        DailyFileLogWriter writer,
        IOptions<LogHubOptions> options,
        ILoggerFactory loggerFactory)
    {
        string expectedPath = options.Value.WebSocketPath;
        if (!string.Equals(context.Request.Path, expectedPath, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        ILogger logger = loggerFactory.CreateLogger("LogHub.WebSocket");
        using WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
        // Une connexion peut traiter plusieurs requetes (read/write) en sequence.

        try
        {
            while (socket.State is not WebSocketState.Closed and not WebSocketState.Aborted)
            {
                string? payload = await WebSocketMessageReader.ReadTextAsync(socket, context.RequestAborted);
                if (payload is null)
                {
                    break;
                }

                LogEnvelope? request;
                try
                {
                    request = JsonSerializer.Deserialize<LogEnvelope>(payload, JsonOptions);
                }
                catch (JsonException ex)
                {
                    // On renvoie une erreur protocolaire sans fermer la connexion.
                    await SendTextAsync(
                        socket,
                        JsonSerializer.Serialize(new LogEnvelopeResponse { Success = false, Error = ex.Message }, JsonOptions),
                        context.RequestAborted);
                    continue;
                }

                if (request is null)
                {
                    await SendTextAsync(
                        socket,
                        JsonSerializer.Serialize(new LogEnvelopeResponse { Success = false, Error = "Empty request." }, JsonOptions),
                        context.RequestAborted);
                    continue;
                }

                LogEnvelopeResponse response = await HandleRequestAsync(request, queue, writer, logger, context.RequestAborted);
                await SendTextAsync(socket, JsonSerializer.Serialize(response, JsonOptions), context.RequestAborted);

                // Le client EasyLog utilise actuellement une connexion WS par operation (request/response).
                // On ferme apres une reponse pour eviter d'attendre inutilement un 2e message.
                break;
            }
        }
        catch (WebSocketException ex)
        {
            // Connexion coupee abruptement par le client: bruit reseau attendu avec retries/fermetures rapides.
            logger.LogDebug(ex, "Client disconnected before WebSocket close handshake completed.");
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Fermeture de requete/connexion en cours d'arret serveur ou client.
        }
        finally
        {
            try
            {
                if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
                }
            }
            catch
            {
                // Le client peut deja avoir ferme/aborte la socket.
            }
        }
    }

    private static async Task<LogEnvelopeResponse> HandleRequestAsync(
        LogEnvelope request,
        ChannelLogQueue queue,
        DailyFileLogWriter writer,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (string.Equals(request.Operation, "write", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(request.Extension))
            {
                return new LogEnvelopeResponse { Success = false, Error = "Extension is required for write." };
            }

            if (string.IsNullOrWhiteSpace(request.SerializedEntry))
            {
                return new LogEnvelopeResponse { Success = false, Error = "SerializedEntry is required for write." };
            }

            DateTime timestampUtc = request.TimestampUtc ?? DateTime.UtcNow;
            if (timestampUtc.Kind != DateTimeKind.Utc)
            {
                timestampUtc = timestampUtc.ToUniversalTime();
            }

            await queue.EnqueueAsync(new QueuedLogWrite
            {
                Extension = request.Extension,
                SerializedEntry = request.SerializedEntry,
                TimestampUtc = timestampUtc
            }, cancellationToken);
            // L'ecriture disque est asynchrone via le worker de fond.

            return new LogEnvelopeResponse { Success = true };
        }

        if (string.Equals(request.Operation, "read", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                IReadOnlyList<SerializedLogEntry> items = writer.ReadEntries(request.MaxFiles, request.ReadAll);
                return new LogEnvelopeResponse
                {
                    Success = true,
                    Entries = items.ToList()
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read logs for remote client.");
                return new LogEnvelopeResponse { Success = false, Error = ex.Message };
            }
        }

        return new LogEnvelopeResponse
        {
            Success = false,
            Error = $"Unknown operation '{request.Operation}'."
        };
    }

    private static Task SendTextAsync(WebSocket socket, string text, CancellationToken cancellationToken)
    {
        // Envoi d'une reponse JSON unique (message texte complet).
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        return socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
    }
}
