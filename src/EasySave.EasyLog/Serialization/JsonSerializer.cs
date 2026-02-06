using System.Text.Json;
using EasySave.EasyLog.Interfaces;

namespace EasySave.EasyLog.Serialization
{
    internal sealed class JsonSerializer : ILogSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        public string FileExtension => "json";

        public string Serialize(object entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            string json = System.Text.Json.JsonSerializer.Serialize(entry, Options);
            return json + Environment.NewLine;
        }
    }
}
