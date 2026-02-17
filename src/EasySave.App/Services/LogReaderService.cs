using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using EasySave.Core.DTO;

namespace EasySave.App.Services;

/// <summary>
/// Service responsable de la lecture, de l'analyse et du formatage des fichiers de logs.
/// Supporte les formats JSON et XML.
/// </summary>
public sealed class LogReaderService
{
    private static readonly JsonSerializerOptions LogJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions PrettyJsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _logDirectory;

    /// <summary>
    /// Initialise une nouvelle instance du service. 
    /// Si aucun chemin n'est fourni, utilise le dossier AppData par défaut.
    /// </summary>
    /// <param name="logDirectory">Chemin optionnel vers le dossier des logs.</param>
    public LogReaderService(string? logDirectory = null)
    {
        _logDirectory = string.IsNullOrWhiteSpace(logDirectory)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ProSoft",
                "EasySave",
                "Logs")
            : logDirectory;
    }

    /// <summary>
    /// Retourne le chemin du dossier de logs actuel.
    /// </summary>
    public string LogDirectory => _logDirectory;

    /// <summary>
    /// Lit les entrées des fichiers de logs les plus récents.
    /// </summary>
    /// <param name="maxFiles">Nombre maximum de fichiers à analyser.</param>
    /// <returns>Une liste d'objets LogEntryDto.</returns>
    public IReadOnlyList<LogEntryDto> ReadEntries(int maxFiles = 7)
    {
        if (string.IsNullOrWhiteSpace(_logDirectory) || !Directory.Exists(_logDirectory))
            return Array.Empty<LogEntryDto>();

        var entries = new List<LogEntryDto>();
        foreach (var file in EnumerateLogFiles(maxFiles))
        {
            entries.AddRange(ReadEntriesFromFile(file));
        }

        return entries;
    }

    /// <summary>
    /// Lit l'intégralité des logs présents dans le dossier.
    /// </summary>
    public IReadOnlyList<LogEntryDto> ReadAllEntries()
    {
        if (string.IsNullOrWhiteSpace(_logDirectory) || !Directory.Exists(_logDirectory))
            return Array.Empty<LogEntryDto>();

        var entries = new List<LogEntryDto>();
        foreach (var file in EnumerateLogFiles(null))
        {
            entries.AddRange(ReadEntriesFromFile(file));
        }

        return entries;
    }

    /// <summary>
    /// Prépare le contenu textuel formaté pour l'interface graphique.
    /// Gère les cas d'erreur .
    /// </summary>
    /// <param name="maxFiles">Limite de fichiers à lire.</param>
    /// <returns>Une liste d'objets LogFileEntry </returns>
    public IReadOnlyList<LogFileEntry> ReadLogFiles(int? maxFiles = null)
    {
        if (!Directory.Exists(_logDirectory))
        {
            return new[] { new LogFileEntry { FileName = "Erreur", Content = "Le dossier de logs n'existe pas." } };
        }

        var files = EnumerateLogFiles(maxFiles).ToList();
        if (!files.Any())
        {
            return new[] { new LogFileEntry { FileName = "Aucun log", Content = "Aucun fichier de log trouvé." } };
        }

        var results = new List<LogFileEntry>();
        foreach (var file in files)
        {
            var extension = Path.GetExtension(file);
            var entries = ReadEntriesFromFile(file).ToList();
            var content = FormatEntries(entries, extension, file);
            var label = string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase) ? "XML" : "JSON";

            results.Add(new LogFileEntry
            {
                FileName = $"{label}: {Path.GetFileName(file)}",
                Content = content
            });
        }

        return results;
    }

    /// <summary>
    /// Liste les fichiers .json et .xml triés par nom décroissant (le plus récent d'abord).
    /// </summary>
    private IEnumerable<string> EnumerateLogFiles(int? maxFiles)
    {
        var files = Directory
            .EnumerateFiles(_logDirectory, "*.json")
            .Concat(Directory.EnumerateFiles(_logDirectory, "*.xml"))
            .OrderByDescending(Path.GetFileName);

        return maxFiles.HasValue ? files.Take(maxFiles.Value) : files;
    }

    /// <summary>
    /// Identifie l'extension du fichier et appelle la méthode de lecture appropriée.
    /// </summary>
    private static IEnumerable<LogEntryDto> ReadEntriesFromFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase))
        {
            return ReadXmlEntries(filePath);
        }

        if (string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
        {
            return ReadJsonEntries(filePath);
        }

        return Array.Empty<LogEntryDto>();
    }

    /// <summary>
    /// Lit un fichier JSON ligne par ligne et désérialise chaque ligne en LogEntryDto.
    /// Gère les balises racine optionnelles <logs>.
    /// </summary>
    private static IEnumerable<LogEntryDto> ReadJsonEntries(string filePath)
    {
        IEnumerable<string> lines;
        try { lines = File.ReadLines(filePath); }
        catch (IOException) { yield break; }

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.Equals("<logs>", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("</logs>", StringComparison.OrdinalIgnoreCase))
                continue;

            LogEntryDto? entry;
            try { entry = JsonSerializer.Deserialize<LogEntryDto>(trimmed, LogJsonOptions); }
            catch (JsonException) { continue; }

            if (entry != null) yield return entry;
        }
    }

    /// <summary>
    /// Charge un document XML et désérialise chaque élément enfant de la racine en LogEntryDto.
    /// </summary>
    private static IEnumerable<LogEntryDto> ReadXmlEntries(string filePath)
    {
        XDocument document;
        try { document = XDocument.Load(filePath); }
        catch (Exception) { yield break; }

        var serializer = new XmlSerializer(typeof(LogEntryDto));
        var root = document.Root;
        if (root is null) yield break;

        foreach (var element in root.Elements())
        {
            LogEntryDto? entry = null;
            try
            {
                using var reader = element.CreateReader();
                entry = serializer.Deserialize(reader) as LogEntryDto;
            }
            catch (InvalidOperationException) { entry = null; }

            if (entry != null) yield return entry;
        }
    }

    /// <summary>
    /// Formate une liste d'entrées en une chaîne de caractères lisible .
    /// </summary>
    private static string FormatEntries(IEnumerable<LogEntryDto> entries, string extension, string filePath)
    {
        var entryList = entries.ToList();
        if (entryList.Count == 0)
        {
            try { return File.ReadAllText(filePath); }
            catch (IOException) { return string.Empty; }
        }

        return string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase)
            ? FormatXml(entryList)
            : FormatJson(entryList);
    }

    /// <summary>
    /// Transforme les entrées en un bloc de texte JSON indenté.
    /// </summary>
    private static string FormatJson(IEnumerable<LogEntryDto> entries)
    {
        var blocks = entries
            .Select(entry => JsonSerializer.Serialize(entry, PrettyJsonOptions))
            .ToList();

        return string.Join(Environment.NewLine + Environment.NewLine, blocks);
    }

    /// <summary>
    /// Reconstruit un document XML complet avec une racine <logs> à partir des entrées.
    /// </summary>
    private static string FormatXml(IEnumerable<LogEntryDto> entries)
    {
        var serializer = new XmlSerializer(typeof(LogEntryDto));
        var root = new XElement("logs");

        foreach (var entry in entries)
        {
            var entryXml = SerializeXml(serializer, entry);
            if (entryXml is not null) root.Add(entryXml);
        }

        var doc = new XDocument(root);
        var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };

        using var writer = new StringWriter();
        using var xmlWriter = XmlWriter.Create(writer, settings);
        doc.Save(xmlWriter);
        xmlWriter.Flush();
        return writer.ToString();
    }

    /// <summary>
    /// Sérialise un objet LogEntryDto unique en XElement pour manipulation XML.
    /// </summary>
    private static XElement? SerializeXml(XmlSerializer serializer, LogEntryDto entry)
    {
        try
        {
            using var stringWriter = new StringWriter();
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true };
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            serializer.Serialize(xmlWriter, entry);
            return XElement.Parse(stringWriter.ToString());
        }
        catch (InvalidOperationException) { return null; }
    }
}

/// <summary>
/// Modèle de données représentant un fichier de log prêt à être affiché dans la vue.
/// </summary>
public sealed class LogFileEntry
{
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}