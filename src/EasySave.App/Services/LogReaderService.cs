using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace EasySave.App.Gui.Services
{
    public class LogReaderService
    {
        private readonly string _logDirectory;

        public LogReaderService()
        {
            _logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ProSoft",
                "EasySave",
                "Logs");
        }

        // Méthode pour récupérer tous les logs sous forme de texte formaté
        public IEnumerable<LogEntry> GetAllLogs()
        {
            if (!Directory.Exists(_logDirectory))
            {
                yield return new LogEntry
                {
                    FileName = "Erreur",
                    Content = "Le dossier de logs n'existe pas."
                };
                yield break;
            }

            // Récupère les fichiers XML et JSON
            var xmlFiles = Directory.GetFiles(_logDirectory, "*.xml")
                                    .OrderByDescending(f => f);
            var jsonFiles = Directory.GetFiles(_logDirectory, "*.json")
                                     .OrderByDescending(f => f);

            if (!xmlFiles.Any() && !jsonFiles.Any())
            {
                yield return new LogEntry
                {
                    FileName = "Aucun log",
                    Content = "Aucun fichier de log trouvé."
                };
                yield break;
            }

            // Traite les fichiers XML
            foreach (var file in xmlFiles)
            {
                yield return new LogEntry
                {
                    FileName = $"XML: {Path.GetFileName(file)}",
                    Content = FormatXml(File.ReadAllText(file))
                };
            }

            // Traite les fichiers JSON
            foreach (var file in jsonFiles)
            {
                yield return new LogEntry
                {
                    FileName = $"JSON: {Path.GetFileName(file)}",
                    Content = FormatJson(File.ReadAllText(file))
                };
            }
        }

        // Formatte le XML pour un affichage lisible
        private string FormatXml(string xml)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                using (var stringWriter = new StringWriter())
                {
                    using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
                    {
                        xmlDoc.WriteTo(xmlWriter);
                    }
                    return stringWriter.ToString();
                }
            }
            catch
            {
                return xml; // Retourne le XML brut en cas d'erreur
            }
        }

        // Formatte le JSON pour un affichage lisible
        private string FormatJson(string json)
        {
            try
            {
                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    using (var stream = new MemoryStream())
                    {
                        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                        {
                            document.WriteTo(writer);
                        }
                        return Encoding.UTF8.GetString(stream.ToArray());
                    }
                }
            }
            catch
            {
                return json; // Retourne le JSON brut en cas d'erreur
            }
        }
    }

    // Classe pour représenter une entrée de log
    public class LogEntry
    {
        public string FileName { get; set; }
        public string Content { get; set; }
    }
}
