using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using EasySave.App.Gui.Models;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Events;
using EasySave.Core.Interfaces;

namespace EasySave.App.Gui.ViewModels;

/// <summary>
/// View model pour les metriques du dashboard et la liste des activites recentes.
/// </summary>
public sealed partial class DashboardViewModel : ViewModelBase, IDisposable
{
    private static readonly TimeSpan LogRefreshCooldown = TimeSpan.FromSeconds(2);
    private readonly IJobService? _jobService;
    private readonly IBackupService? _backupService;
    private readonly string? _logsPath;
    private readonly SynchronizationContext? _uiContext;
    private DateTime _lastLogRefreshUtc;
    private bool _disposed;

    [ObservableProperty]
    private int _totalJobs;

    [ObservableProperty]
    private int _activeJobs;

    [ObservableProperty]
    private int _inactiveJobs;

    [ObservableProperty]
    private string _lastBackup = "Never";

    [ObservableProperty]
    private string _systemStatus = "Idle";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRecentActivityEmpty))]
    private bool _hasRecentActivities;

    public ObservableCollection<RecentActivityItem> RecentActivities { get; } = new();

    /// <summary>
    /// Creates a design-time instance without service dependencies.
    /// </summary>
    public DashboardViewModel()
    {
        _uiContext = SynchronizationContext.Current;
    }

    /// <summary>
    /// Creates a runtime instance wired to core services and logs.
    /// </summary>
    /// <param name="jobService">Job service used for counts and types.</param>
    /// <param name="backupService">Backup service that publishes state updates.</param>
    /// <param name="logsPath">Directory containing log files.</param>
    /// <exception cref="ArgumentNullException">Thrown when a required service is null.</exception>
    public DashboardViewModel(IJobService jobService, IBackupService backupService, string? logsPath)
    {
        _uiContext = SynchronizationContext.Current;
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _logsPath = logsPath;
        RefreshFromJobs();
        LoadRecentActivities();
        _backupService.StateChanged += OnStateChanged;
    }

    /// <summary>
    /// Updates counters using the job list.
    /// </summary>
    private void RefreshFromJobs()
    {
        var jobs = _jobService!.GetAll();
        TotalJobs = jobs.Count;
        ActiveJobs = jobs.Count(job => job.IsActive);
        InactiveJobs = TotalJobs - ActiveJobs;

        var lastJob = jobs
            .Where(job => job.LastRun.HasValue)
            .OrderByDescending(job => job.LastRun)
            .FirstOrDefault();

        LastBackup = lastJob?.LastRun?.ToLocalTime().ToString("g", CultureInfo.CurrentCulture) ?? "Never";
    }

    /// <summary>
    /// Relays state updates on the UI thread when available.
    /// </summary>
    private void OnStateChanged(object? sender, JobStateChangedEventArgs e)
    {
        if (_uiContext != null)
        {
            _uiContext.Post(_ => HandleStateChanged(e), null);
            return;
        }
        HandleStateChanged(e);
    }

    /// <summary>
    /// Applies the new state and triggers required refreshes.
    /// </summary>
    private void HandleStateChanged(JobStateChangedEventArgs e)
    {
        SystemStatus = e.State.Status.ToString();
        RefreshLogsIfNeeded();

        if (e.State.Status is JobStatus.Completed or JobStatus.Error)
        {
            RefreshFromJobs();
            LoadRecentActivities();
        }
    }

    /// <summary>
    /// Refreshes logs with a cooldown to limit reads.
    /// </summary>
    private void RefreshLogsIfNeeded()
    {
        if (string.IsNullOrWhiteSpace(_logsPath))
            return;

        var nowUtc = DateTime.UtcNow;
        // Evite de relire les logs a chaque fichier.
        if (nowUtc - _lastLogRefreshUtc < LogRefreshCooldown)
            return;

        _lastLogRefreshUtc = nowUtc;
        LoadRecentActivities();
    }

    /// <summary>
    /// Loads recent activities from the logs.
    /// </summary>
    private void LoadRecentActivities()
    {
        RecentActivities.Clear();
        HasRecentActivities = false;

        if (string.IsNullOrWhiteSpace(_logsPath) || !Directory.Exists(_logsPath))
            return;

        var entries = ReadLogEntries(_logsPath);
        if (entries.Count == 0)
            return;

        // Prefere les lignes de resume pour ne pas surcharger l UI.
        var summaryEntries = entries.Where(IsSummaryEntry).ToList();
        var sourceEntries = summaryEntries.Count > 0 ? summaryEntries : entries;

        foreach (var entry in sourceEntries
                     .OrderByDescending(entry => entry.TimestampUtc)
                     .Take(5))
        {
            RecentActivities.Add(CreateActivityItem(entry));
        }

        HasRecentActivities = RecentActivities.Count > 0;
    }

    /// <summary>
    /// Indique si la liste des activites recentes est vide.
    /// </summary>
    public bool IsRecentActivityEmpty => !HasRecentActivities;

    /// <summary>
    /// Reads recent log entries from a directory.
    /// </summary>
    /// <param name="logsPath">Directory containing log files.</param>
    /// <returns>List of log entries.</returns>
    private static List<LogEntryDto> ReadLogEntries(string logsPath)
    {
        // Lecture d un sous-ensemble des fichiers journaliers recents pour limiter les IO.
        var files = Directory
            .EnumerateFiles(logsPath, "*.json")
            .Concat(Directory.EnumerateFiles(logsPath, "*.xml"))
            .OrderByDescending(Path.GetFileName)
            .Take(7)
            .ToList();

        var entries = new List<LogEntryDto>();
        foreach (var file in files)
        {
            var extension = Path.GetExtension(file);
            if (string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    entries.AddRange(ReadJsonEntries(file));
                }
                catch (IOException)
                {
                    // Ignore les fichiers illisibles.
                }
            }
            else if (string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    entries.AddRange(ReadXmlEntries(file));
                }
                catch (IOException)
                {
                    // Ignore les fichiers illisibles.
                }
            }
        }

        return entries
            .Where(entry => entry != null)
            .Where(entry => entry.TimestampUtc != default)
            .ToList();
    }

    /// <summary>
    /// Reads JSON entries line by line.
    /// </summary>
    /// <param name="filePath">Path to the JSON file.</param>
    /// <returns>Sequence of log entries.</returns>
    private static IEnumerable<LogEntryDto> ReadJsonEntries(string filePath)
    {
        foreach (var line in File.ReadLines(filePath))
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
                entry = JsonSerializer.Deserialize<LogEntryDto>(trimmed);
            }
            catch (JsonException)
            {
                continue;
            }

            if (entry != null)
                yield return entry;
        }
    }

    /// <summary>
    /// Reads XML entries from a file.
    /// </summary>
    /// <param name="filePath">Path to the XML file.</param>
    /// <returns>Sequence of log entries.</returns>
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

    /// <summary>
    /// Indicates whether the entry is a job summary row.
    /// </summary>
    /// <param name="entry">Entry to analyze.</param>
    /// <returns><c>true</c> when the entry is a summary.</returns>
    private static bool IsSummaryEntry(LogEntryDto entry)
    {
        if (entry.ErrorMessage is null)
            return false;

        // Les entrées de type résumé incluent des compteurs agrégés.
        return entry.ErrorMessage.Contains("Copied=", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Builds a recent activity item for the UI.
    /// </summary>
    /// <param name="entry">Source entry.</param>
    /// <returns>Item ready for display.</returns>
    private RecentActivityItem CreateActivityItem(LogEntryDto entry)
    {
        var timestamp = entry.TimestampUtc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);
        var (glyph, color) = MapStatusGlyph(entry.Status);
        var title = string.IsNullOrWhiteSpace(entry.JobName) ? "Backup activity" : entry.JobName;
        var subtitle = IsSummaryEntry(entry)
            ? BuildSummarySubtitle(entry)
            : BuildNonSummarySubtitle(entry);

        return new RecentActivityItem(
            title,
            subtitle,
            timestamp,
            glyph,
            "#20FFFFFF",
            color);
    }

    /// <summary>
    /// Builds the subtitle for a summary entry.
    /// </summary>
    /// <param name="entry">Source entry.</param>
    /// <returns>Subtitle text.</returns>
    private string BuildSummarySubtitle(LogEntryDto entry)
    {
        var typeLabel = ResolveBackupType(entry.JobName);
        var sizeLabel = entry.FileSizeBytes > 0 ? FormatBytes(entry.FileSizeBytes) : "0 B";
        var countsLabel = BuildCountsSummary(entry.ErrorMessage);
        var durationLabel = entry.TransferTimeMs > 0
            ? FormatDuration(TimeSpan.FromMilliseconds(entry.TransferTimeMs))
            : string.Empty;

        var parts = new List<string> { typeLabel, $"{sizeLabel} transferred" };
        if (!string.IsNullOrWhiteSpace(countsLabel))
            parts.Add(countsLabel);
        if (!string.IsNullOrWhiteSpace(durationLabel))
            parts.Add(durationLabel);

        return Truncate(string.Join(" | ", parts), 140);
    }

    /// <summary>
    /// Builds the subtitle for a detailed entry.
    /// </summary>
    /// <param name="entry">Source entry.</param>
    /// <returns>Subtitle text.</returns>
    private static string BuildNonSummarySubtitle(LogEntryDto entry)
    {
        if (string.Equals(entry.Status, "ERROR", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(entry.ErrorMessage))
        {
            return Truncate($"Error | {entry.ErrorMessage}", 140);
        }

        if (string.Equals(entry.Status, "DIR_CREATED", StringComparison.OrdinalIgnoreCase))
        {
            var targetName = Path.GetFileName(entry.TargetPath);
            if (string.IsNullOrWhiteSpace(targetName))
                targetName = entry.TargetPath;
            return Truncate($"Directory created | {targetName}", 140);
        }

        var sourceName = Path.GetFileName(entry.SourcePath);
        var targetNameFallback = Path.GetFileName(entry.TargetPath);
        if (string.IsNullOrWhiteSpace(sourceName))
            sourceName = entry.SourcePath;
        if (string.IsNullOrWhiteSpace(targetNameFallback))
            targetNameFallback = entry.TargetPath;

        var sizeLabel = entry.FileSizeBytes > 0 ? FormatBytes(entry.FileSizeBytes) : "0 B";
        var action = string.Equals(entry.Status, "SKIPPED", StringComparison.OrdinalIgnoreCase)
            ? "Skipped"
            : "File copy";

        return Truncate($"{action} | {sizeLabel} | {sourceName} -> {targetNameFallback}", 140);
    }

    /// <summary>
    /// Resolves the backup type for a job name.
    /// </summary>
    /// <param name="jobName">Job name.</param>
    /// <returns>Backup type label.</returns>
    private string ResolveBackupType(string? jobName)
    {
        if (_jobService is null || string.IsNullOrWhiteSpace(jobName))
            return "Backup";

        var job = _jobService.GetAll()
            .FirstOrDefault(candidate => string.Equals(candidate.Name, jobName, StringComparison.Ordinal));

        return job?.Type switch
        {
            BackupType.Full => "Full backup",
            BackupType.Differential => "Differential backup",
            _ => "Backup"
        };
    }

    /// <summary>
    /// Extracts counters from a summary message.
    /// </summary>
    /// <param name="errorMessage">Message containing counters.</param>
    /// <returns>Counter summary text.</returns>
    private static string BuildCountsSummary(string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            return string.Empty;

        int? copied = null;
        int? skipped = null;
        int? errors = null;

        var segments = errorMessage.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            var trimmed = segment.Trim();
            if (trimmed.StartsWith("Copied=", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(trimmed["Copied=".Length..], out var value))
                    copied = value;
            }
            else if (trimmed.StartsWith("Skipped=", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(trimmed["Skipped=".Length..], out var value))
                    skipped = value;
            }
            else if (trimmed.StartsWith("Errors=", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(trimmed["Errors=".Length..], out var value))
                    errors = value;
            }
        }

        var parts = new List<string>();
        if (copied.HasValue)
            parts.Add($"Copied {copied.Value}");
        if (skipped.HasValue)
            parts.Add($"Skipped {skipped.Value}");
        if (errors.HasValue)
            parts.Add($"Errors {errors.Value}");

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Maps a status to a glyph and color.
    /// </summary>
    /// <param name="status">Status to interpret.</param>
    /// <returns>Glyph and color.</returns>
    private static (string glyph, string color) MapStatusGlyph(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return ("?", "#80FFFFFF");

        return status.ToUpperInvariant() switch
        {
            "OK" => ("✓", "#30D158"),
            "ERROR" => ("!", "#FF3B30"),
            "SKIPPED" => ("-", "#FF9F0A"),
            "DIR_CREATED" => ("+", "#0A84FF"),
            _ => (status.ToUpperInvariant(), "#80FFFFFF")
        };
    }

    /// <summary>
    /// Formats a duration as a short label.
    /// </summary>
    /// <param name="duration">Duration to format.</param>
    /// <returns>Formatted label.</returns>
    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";
        if (duration.TotalMinutes >= 1)
            return $"{duration.Minutes}m {duration.Seconds}s";

        var seconds = Math.Max(0, (int)Math.Round(duration.TotalSeconds));
        return $"{seconds}s";
    }

    /// <summary>
    /// Formats a byte size with a readable unit.
    /// </summary>
    /// <param name="bytes">Size in bytes.</param>
    /// <returns>Formatted label.</returns>
    private static string FormatBytes(long bytes)
    {
        const double kilo = 1024.0;
        const double mega = kilo * 1024.0;
        const double giga = mega * 1024.0;

        if (bytes >= giga)
            return $"{bytes / giga:0.##} GB";
        if (bytes >= mega)
            return $"{bytes / mega:0.##} MB";
        if (bytes >= kilo)
            return $"{bytes / kilo:0.##} KB";

        return $"{bytes} B";
    }

    /// <summary>
    /// Truncates text to a maximum length.
    /// </summary>
    /// <param name="value">Text to truncate.</param>
    /// <param name="maxLength">Maximum length.</param>
    /// <returns>Truncated text.</returns>
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength - 3) + "...";
    }

    /// <summary>
    /// Unsubscribes from events and releases managed resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        if (_backupService != null)
        {
            _backupService.StateChanged -= OnStateChanged;
        }

        GC.SuppressFinalize(this);
    }
}
