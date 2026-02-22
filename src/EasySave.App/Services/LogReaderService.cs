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
/// Reads, parses, and formats application log files for display and inspection.
/// Supports JSON and XML log formats.
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
    /// Initializes a new instance of the service.
    /// Uses the default EasySave AppData log folder when no path is provided.
    /// </summary>
    /// <param name="logDirectory">Optional path to the logs folder.</param>
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
    /// Gets the log directory path used by this service.
    /// </summary>
    public string LogDirectory => _logDirectory;

    /// <summary>
    /// Reads log entries from a limited number of files in the log directory.
    /// Files are selected using descending file-name order.
    /// </summary>
    /// <param name="maxFiles">Maximum number of log files to parse.</param>
    /// <returns>A read-only list of parsed <see cref="LogEntryDto"/> entries.</returns>
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
    /// Reads all parseable log entries from every supported file in the log directory.
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
    /// Builds formatted log-file payloads for the graphical interface.
    /// Returns placeholder entries when the log directory is missing or contains no files.
    /// </summary>
    /// <param name="maxFiles">Optional maximum number of log files to include.</param>
    /// <returns>A read-only list of <see cref="LogFileEntry"/> objects ready for display.</returns>
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
    /// Enumerates supported log files (.json and .xml) sorted by descending file name.
    /// Applies an optional file-count limit.
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
    /// Dispatches file parsing to the reader that matches the file extension.
    /// Returns an empty sequence for unsupported extensions.
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
    /// Reads a JSON log file line by line and deserializes each valid line into a <see cref="LogEntryDto"/>.
    /// Ignores empty lines, optional wrapper tags such as &lt;logs&gt;, and malformed JSON lines.
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
    /// Loads an XML document and deserializes each child element of the root into a <see cref="LogEntryDto"/>.
    /// Invalid documents or invalid child elements are skipped.
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
    /// Formats parsed log entries into a readable text representation.
    /// If no entries were parsed, returns the raw file content when available.
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
    /// Serializes entries as indented JSON blocks separated by blank lines.
    /// </summary>
    private static string FormatJson(IEnumerable<LogEntryDto> entries)
    {
        var blocks = entries
            .Select(entry => JsonSerializer.Serialize(entry, PrettyJsonOptions))
            .ToList();

        return string.Join(Environment.NewLine + Environment.NewLine, blocks);
    }

    /// <summary>
    /// Rebuilds a complete XML document with a &lt;logs&gt; root element from the parsed entries.
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
    /// Serializes a single <see cref="LogEntryDto"/> into an <see cref="XElement"/> for XML document reconstruction.
    /// Returns <see langword="null"/> when serialization fails.
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
/// Represents a formatted log file entry ready to be displayed in the UI.
/// </summary>
public sealed class LogFileEntry
{
    /// <summary>
    /// Gets or sets the display label for the log file.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the formatted file content to display.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
