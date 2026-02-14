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
/// Reads log files and exposes parsed entries or formatted content.
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

    public string LogDirectory => _logDirectory;

    /// <summary>
    /// Reads all entries across recent log files.
    /// </summary>
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
    /// Reads all entries across all available log files.
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
    /// Reads log files and returns formatted content for the log view.
    /// </summary>
    public IReadOnlyList<LogFileEntry> ReadLogFiles(int? maxFiles = null)
    {
        if (!Directory.Exists(_logDirectory))
        {
            return new[]
            {
                new LogFileEntry
                {
                    FileName = "Erreur",
                    Content = "Le dossier de logs n'existe pas."
                }
            };
        }

        var files = EnumerateLogFiles(maxFiles).ToList();
        if (!files.Any())
        {
            return new[]
            {
                new LogFileEntry
                {
                    FileName = "Aucun log",
                    Content = "Aucun fichier de log trouvé."
                }
            };
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

    private IEnumerable<string> EnumerateLogFiles(int? maxFiles)
    {
        var files = Directory
            .EnumerateFiles(_logDirectory, "*.json")
            .Concat(Directory.EnumerateFiles(_logDirectory, "*.xml"))
            .OrderByDescending(Path.GetFileName);

        return maxFiles.HasValue ? files.Take(maxFiles.Value) : files;
    }

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

    private static IEnumerable<LogEntryDto> ReadJsonEntries(string filePath)
    {
        IEnumerable<string> lines;
        try
        {
            lines = File.ReadLines(filePath);
        }
        catch (IOException)
        {
            yield break;
        }

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0)
                continue;
            if (string.Equals(trimmed, "<logs>", StringComparison.OrdinalIgnoreCase))
                continue;
            if (string.Equals(trimmed, "</logs>", StringComparison.OrdinalIgnoreCase))
                continue;

            LogEntryDto? entry;
            try
            {
                entry = JsonSerializer.Deserialize<LogEntryDto>(trimmed, LogJsonOptions);
            }
            catch (JsonException)
            {
                continue;
            }

            if (entry != null)
                yield return entry;
        }
    }

    private static IEnumerable<LogEntryDto> ReadXmlEntries(string filePath)
    {
        XDocument document;
        try
        {
            document = XDocument.Load(filePath);
        }
        catch (Exception)
        {
            yield break;
        }

        var serializer = new XmlSerializer(typeof(LogEntryDto));
        var root = document.Root;
        if (root is null)
            yield break;

        foreach (var element in root.Elements())
        {
            LogEntryDto? entry = null;
            try
            {
                using var reader = element.CreateReader();
                entry = serializer.Deserialize(reader) as LogEntryDto;
            }
            catch (InvalidOperationException)
            {
                entry = null;
            }

            if (entry != null)
                yield return entry;
        }
    }

    private static string FormatEntries(IEnumerable<LogEntryDto> entries, string extension, string filePath)
    {
        var entryList = entries.ToList();
        if (entryList.Count == 0)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (IOException)
            {
                return string.Empty;
            }
        }

        if (string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase))
            return FormatXml(entryList);

        return FormatJson(entryList);
    }

    private static string FormatJson(IEnumerable<LogEntryDto> entries)
    {
        var blocks = entries
            .Select(entry => JsonSerializer.Serialize(entry, PrettyJsonOptions))
            .ToList();

        return string.Join(Environment.NewLine + Environment.NewLine, blocks);
    }

    private static string FormatXml(IEnumerable<LogEntryDto> entries)
    {
        var serializer = new XmlSerializer(typeof(LogEntryDto));
        var root = new XElement("logs");

        foreach (var entry in entries)
        {
            var entryXml = SerializeXml(serializer, entry);
            if (entryXml is not null)
                root.Add(entryXml);
        }

        var doc = new XDocument(root);
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Indent = true
        };

        using var writer = new StringWriter();
        using var xmlWriter = XmlWriter.Create(writer, settings);
        doc.Save(xmlWriter);
        xmlWriter.Flush();
        return writer.ToString();
    }

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
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}

/// <summary>
/// Represents a formatted log file entry for the log view.
/// </summary>
public sealed class LogFileEntry
{
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
