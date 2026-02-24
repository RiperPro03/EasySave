using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using EasySave.EasyLog.Interfaces;

namespace EasySave.EasyLog.Readers
{
    /// <summary>
    /// Reads JSON and XML daily log files from a local directory.
    /// </summary>
    /// <typeparam name="T">The log entry type.</typeparam>
    internal sealed class LocalFileLogReader<T> : ILogReader<T>
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public LocalFileLogReader(string logDirectory)
        {
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                throw new ArgumentException("Log directory is required.", nameof(logDirectory));
            }

            LogDirectory = logDirectory;
        }

        /// <summary>
        /// Gets the local log directory used by this reader.
        /// </summary>
        public string LogDirectory { get; }

        /// <summary>
        /// Lists supported local log files sorted by descending file name.
        /// </summary>
        /// <param name="maxFiles">Optional maximum number of files to return.</param>
        /// <returns>A read-only list of local log file paths.</returns>
        public IReadOnlyList<string> GetLogFiles(int? maxFiles = null)
        {
            if (!Directory.Exists(LogDirectory))
            {
                return Array.Empty<string>();
            }

            IEnumerable<string> files = Directory
                .EnumerateFiles(LogDirectory, "*.json")
                .Concat(Directory.EnumerateFiles(LogDirectory, "*.xml"))
                .OrderByDescending(Path.GetFileName);

            if (maxFiles.HasValue)
            {
                files = files.Take(maxFiles.Value);
            }

            return files.ToList();
        }

        /// <summary>
        /// Reads entries from a limited number of local files.
        /// </summary>
        /// <param name="maxFiles">Maximum number of files to parse.</param>
        /// <returns>A read-only list of parsed entries.</returns>
        public IReadOnlyList<T> ReadEntries(int maxFiles = 7)
        {
            var entries = new List<T>();
            foreach (string filePath in GetLogFiles(maxFiles))
            {
                entries.AddRange(ReadEntriesFromFile(filePath));
            }

            return entries;
        }

        /// <summary>
        /// Reads entries from all supported local files.
        /// </summary>
        /// <returns>A read-only list of parsed entries.</returns>
        public IReadOnlyList<T> ReadAllEntries()
        {
            var entries = new List<T>();
            foreach (string filePath in GetLogFiles(null))
            {
                entries.AddRange(ReadEntriesFromFile(filePath));
            }

            return entries;
        }

        /// <summary>
        /// Reads entries from a specific local log file.
        /// </summary>
        /// <param name="filePath">Path to a supported local log file.</param>
        /// <returns>A read-only list of parsed entries.</returns>
        public IReadOnlyList<T> ReadEntriesFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return Array.Empty<T>();
            }

            string extension = Path.GetExtension(filePath);
            return string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase)
                ? ReadXmlEntries(filePath).ToList()
                : string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase)
                    ? ReadJsonEntries(filePath).ToList()
                    : Array.Empty<T>();
        }

        private static IEnumerable<T> ReadJsonEntries(string filePath)
        {
            IEnumerable<string> lines;
            try
            {
                lines = File.ReadLines(filePath);
            }
            catch (IOException)
            {
                return Array.Empty<T>();
            }

            var entries = new List<T>();
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                // Filtre defensif (fichier vide / traces XML parasites dans un flux JSON).
                if (trimmed.Length == 0
                    || trimmed.Equals("<logs>", StringComparison.OrdinalIgnoreCase)
                    || trimmed.Equals("</logs>", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                T? entry;
                try
                {
                    entry = JsonSerializer.Deserialize<T>(trimmed, JsonOptions);
                }
                catch (JsonException)
                {
                    continue;
                }

                if (entry is not null)
                {
                    entries.Add(entry);
                }
            }

            return entries;
        }

        private static IEnumerable<T> ReadXmlEntries(string filePath)
        {
            XDocument document;
            try
            {
                document = XDocument.Load(filePath);
            }
            catch (Exception)
            {
                return Array.Empty<T>();
            }

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            XElement? root = document.Root;
            if (root is null)
            {
                return Array.Empty<T>();
            }

            var entries = new List<T>();
            foreach (XElement element in root.Elements())
            {
                T? entry = default;
                try
                {
                    // Chaque noeud enfant est deserialize comme une entree autonome.
                    using var reader = element.CreateReader();
                    object? deserialized = serializer.Deserialize(reader);
                    if (deserialized is T typed)
                    {
                        entry = typed;
                    }
                }
                catch (InvalidOperationException)
                {
                    entry = default;
                }

                if (entry is not null)
                {
                    entries.Add(entry);
                }
            }

            return entries;
        }
    }
}
