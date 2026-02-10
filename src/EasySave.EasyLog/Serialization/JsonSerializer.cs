using System.Text.Json;
using EasySave.EasyLog.Interfaces;

namespace EasySave.EasyLog.Serialization
{
    /// <summary>
    /// Serializes log entries to JSON.
    /// </summary>
    internal sealed class JsonSerializer : ILogSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        /// <summary>
        /// Gets the JSON file extension.
        /// </summary>
        public string FileExtension => "json";

        /// <summary>
        /// Serializes an entry to JSON.
        /// </summary>
        /// <param name="entry">The entry to serialize.</param>
        /// <returns>The serialized JSON line.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is null.</exception>
        public string Serialize(object entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            string json = System.Text.Json.JsonSerializer.Serialize(entry, Options);
            return json + Environment.NewLine;
        }
    }
}
