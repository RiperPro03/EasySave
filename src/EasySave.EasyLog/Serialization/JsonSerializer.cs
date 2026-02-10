using System.Text.Json;
using EasySave.EasyLog.Interfaces;

namespace EasySave.EasyLog.Serialization
{
    /// <summary>
    /// Cette classe est responsable de la transformation des objets en format JSON
    /// </summary>
    internal sealed class JsonSerializer : ILogSerializer
    {
        /// <summary>
        /// On désactive l'indentation pour que chaque entrée de log tienne sur une seule ligne
        /// </summary>
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        /// <summary>
        /// Définit l'extension de fichier associée à ce format
        /// </summary>
        public string FileExtension => "json";

        /// <summary>
        /// Convertit l'objet de log en une chaîne de caractères JSON
        /// </summary>
        public string Serialize(object entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            string json = System.Text.Json.JsonSerializer.Serialize(entry, Options);
            return json + Environment.NewLine;
        }
    }
}
