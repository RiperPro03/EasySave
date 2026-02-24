using System.Xml.Linq;
using LogHub.Server.Contracts;
using LogHub.Server.Infrastructure.Queueing;
using LogHub.Server.Options;
using Microsoft.Extensions.Options;

namespace LogHub.Server.Infrastructure.Storage;

/// <summary>
/// Persists serialized log entries to daily JSON/XML files and reads them back.
/// </summary>
public sealed class DailyFileLogWriter
{
    private readonly string _logDirectory;
    private readonly object _writeLock = new();

    /// <summary>
    /// Initializes a new storage instance using configured log directory settings.
    /// </summary>
    /// <param name="options">Server options wrapper.</param>
    public DailyFileLogWriter(IOptions<LogHubOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _logDirectory = options.Value.LogDirectory;
        Directory.CreateDirectory(_logDirectory);
    }

    /// <summary>
    /// Writes a queued log payload to the corresponding daily file.
    /// </summary>
    /// <param name="item">Queued log write payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed value task when the write finishes.</returns>
    public ValueTask WriteAsync(QueuedLogWrite item, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(item);

        string extension = NormalizeExtension(item.Extension);
        DateTime timestampUtc = item.TimestampUtc.Kind == DateTimeKind.Utc
            ? item.TimestampUtc
            : item.TimestampUtc.ToUniversalTime();
        string fileName = $"{timestampUtc:yyyy-MM-dd}.{extension}";
        string path = Path.Combine(_logDirectory, fileName);

        lock (_writeLock)
        {
            // Section critique: evite les corruptions quand plusieurs clients ecrivent en meme temps.
            if (string.Equals(extension, "xml", StringComparison.OrdinalIgnoreCase))
            {
                WriteXml(path, item.SerializedEntry);
            }
            else
            {
                Directory.CreateDirectory(_logDirectory);
                File.AppendAllText(path, item.SerializedEntry);
            }
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Reads serialized log entries from daily files.
    /// </summary>
    /// <param name="maxFiles">Maximum number of files to scan when <paramref name="readAll"/> is false.</param>
    /// <param name="readAll">Whether all files should be scanned.</param>
    /// <returns>A read-only list of serialized entries.</returns>
    public IReadOnlyList<SerializedLogEntry> ReadEntries(int? maxFiles, bool readAll)
    {
        if (!Directory.Exists(_logDirectory))
        {
            return Array.Empty<SerializedLogEntry>();
        }

        IEnumerable<string> files = Directory
            .EnumerateFiles(_logDirectory, "*.json")
            .Concat(Directory.EnumerateFiles(_logDirectory, "*.xml"))
            .OrderByDescending(Path.GetFileName);

        if (!readAll && maxFiles.HasValue)
        {
            files = files.Take(maxFiles.Value);
        }

        var items = new List<SerializedLogEntry>();
        foreach (string file in files)
        {
            string extension = NormalizeExtension(Path.GetExtension(file));
            if (string.Equals(extension, "json", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string line in ReadJsonLines(file))
                {
                    items.Add(new SerializedLogEntry
                    {
                        Extension = extension,
                        SerializedEntry = line,
                        FileName = Path.GetFileName(file)
                    });
                }

                continue;
            }

            foreach (string xmlFragment in ReadXmlFragments(file))
            {
                items.Add(new SerializedLogEntry
                {
                    Extension = extension,
                    SerializedEntry = xmlFragment,
                    FileName = Path.GetFileName(file)
                });
            }
        }

        return items;
    }

    private static void WriteXml(string path, string serializedEntry)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");

        if (!File.Exists(path) || new FileInfo(path).Length == 0)
        {
            // Initialise un document XML complet avec une racine unique.
            File.WriteAllText(path, $"<logs>{Environment.NewLine}{serializedEntry}{Environment.NewLine}</logs>");
            return;
        }

        using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        if (fs.Length >= 7)
        {
            // Retire la fermeture </logs> pour inserer un nouveau fragment juste avant.
            fs.SetLength(fs.Length - 7);
            fs.Position = fs.Length;
        }

        using var writer = new StreamWriter(fs);
        writer.WriteLine(serializedEntry.TrimEnd('\r', '\n'));
        writer.Write("</logs>");
    }

    private static IEnumerable<string> ReadJsonLines(string file)
    {
        IEnumerable<string> lines;
        try
        {
            lines = File.ReadLines(file);
        }
        catch (IOException)
        {
            return Array.Empty<string>();
        }

        var items = new List<string>();
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            // Ignore les lignes vides et d'eventuelles balises XML parasites.
            if (trimmed.Length == 0
                || trimmed.Equals("<logs>", StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("</logs>", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            items.Add(trimmed);
        }

        return items;
    }

    private static IEnumerable<string> ReadXmlFragments(string file)
    {
        XDocument document;
        try
        {
            document = XDocument.Load(file);
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }

        XElement? root = document.Root;
        if (root is null)
        {
            return Array.Empty<string>();
        }

        var items = new List<string>();
        foreach (XElement element in root.Elements())
        {
            // Renvoie chaque noeud enfant comme fragment XML autonome.
            items.Add(element.ToString(SaveOptions.DisableFormatting));
        }

        return items;
    }

    private static string NormalizeExtension(string extension)
    {
        string normalized = extension.Trim();
        if (normalized.StartsWith(".", StringComparison.Ordinal))
        {
            normalized = normalized[1..];
        }

        // Fallback defensif vers json si l'extension est absente.
        return normalized.Length == 0 ? "json" : normalized.ToLowerInvariant();
    }
}
