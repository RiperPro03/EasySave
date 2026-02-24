using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using EasySave.App.Services;
using EasySave.App.Gui.Models;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Events;
using EasySave.Core.Interfaces;
using EasySave.Core.Resources;
using EasySave.EasyLog.Options;

namespace EasySave.App.Gui.ViewModels;

/// <summary>
/// View model pour les metriques du dashboard et la liste des activites recentes.
/// </summary>
public sealed partial class DashboardViewModel : ViewModelBase, IDisposable
{
    private static readonly TimeSpan LogRefreshCooldown = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan LogRefreshDelay = TimeSpan.FromMilliseconds(300);
    private const string ColorAccentGreen = "#30D158";
    private const string ColorAccentBlue = "#0A84FF";
    private const string ColorAccentOrange = "#FF9F0A";
    private const string ColorAccentRed = "#FF3B30";
    private const string ColorNeutralGray = "#A0A7B4";
    private const string ColorGlassOverlay = "#20FFFFFF";
    private const string ColorTextMuted = "#80FFFFFF";
    private readonly IJobService? _jobService;
    private readonly IBackupService? _backupService;
    private readonly LogReaderService? _logReader;
    private readonly SynchronizationContext? _uiContext;
    private DateTime _lastLogRefreshUtc;
    private CancellationTokenSource? _logRefreshCts;
    private int _recentActivitiesLoadVersion;
    private bool _disposed;

    [ObservableProperty]
    private int _totalJobs;

    [ObservableProperty]
    private int _activeJobs;

    [ObservableProperty]
    private int _inactiveJobs;

    [ObservableProperty]
    private string _lastBackup = Strings.Gui_Common_Never;

    [ObservableProperty]
    private string _systemStatus = Strings.Gui_JobStatus_Idle;

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
    /// <param name="logReader">Log reader used to parse entries.</param>
    /// <exception cref="ArgumentNullException">Thrown when a required service is null.</exception>
    public DashboardViewModel(IJobService jobService, IBackupService backupService, LogReaderService logReader)
    {
        _uiContext = SynchronizationContext.Current;
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _logReader = logReader ?? throw new ArgumentNullException(nameof(logReader));
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

        LastBackup = lastJob?.LastRun?.ToLocalTime().ToString("g", CultureInfo.CurrentCulture) ?? Strings.Gui_Common_Never;
    }

    /// <summary>
    /// Refreshes job counters when job definitions change.
    /// </summary>
    public void RefreshJobSummary()
    {
        if (_jobService is null)
            return;

        RefreshFromJobs();
    }

    /// <summary>
    /// Refreshes recent activities when logs are updated.
    /// </summary>
    public void NotifyLogWritten()
    {
        RefreshLogsIfNeeded();
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
        SystemStatus = ResolveJobStatusLabel(e.State.Status);
        RefreshLogsIfNeeded();

        if (e.State.Status is JobStatus.Completed or JobStatus.Error)
        {
            RefreshFromJobs();
            LoadRecentActivities();
            ScheduleLogRefresh(LogRefreshDelay);
        }
    }

    /// <summary>
    /// Refreshes logs with a cooldown to limit reads.
    /// </summary>
    private void RefreshLogsIfNeeded()
    {
        if (_logReader is null)
            return;

        var nowUtc = DateTime.UtcNow;
        // Evite de relire les logs a chaque fichier.
        if (nowUtc - _lastLogRefreshUtc < LogRefreshCooldown)
            return;

        _lastLogRefreshUtc = nowUtc;
        LoadRecentActivities();
    }

    private void ScheduleLogRefresh(TimeSpan delay)
    {
        if (_logReader is null)
            return;

        _logRefreshCts?.Cancel();
        var cts = new CancellationTokenSource();
        _logRefreshCts = cts;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay, cts.Token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            if (cts.IsCancellationRequested)
                return;

            if (_uiContext != null)
            {
                _uiContext.Post(_ => LoadRecentActivities(), null);
            }
            else
            {
                LoadRecentActivities();
            }
        });
    }

    /// <summary>
    /// Loads recent activities from the logs.
    /// </summary>
    private void LoadRecentActivities()
    {
        if (_logReader is null)
        {
            ApplyRecentActivities(Array.Empty<RecentActivityItem>());
            return;
        }

        int loadVersion = Interlocked.Increment(ref _recentActivitiesLoadVersion);
        _ = Task.Run(() =>
        {
            List<RecentActivityItem> activityItems;
            try
            {
                // Lecture/parsing des logs hors thread UI pour eviter les freezes reseau/IO.
                activityItems = BuildRecentActivities();
            }
            catch
            {
                activityItems = new List<RecentActivityItem>();
            }

            if (loadVersion != Volatile.Read(ref _recentActivitiesLoadVersion))
                return;

            if (_uiContext != null)
            {
                _uiContext.Post(_ =>
                {
                    if (loadVersion != Volatile.Read(ref _recentActivitiesLoadVersion))
                        return;

                    ApplyRecentActivities(activityItems);
                }, null);
            }
            else
            {
                ApplyRecentActivities(activityItems);
            }
        });
    }

    private void ApplyRecentActivities(IReadOnlyList<RecentActivityItem> items)
    {
        RecentActivities.Clear();
        foreach (var item in items)
        {
            RecentActivities.Add(item);
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
    private List<RecentActivityItem> BuildRecentActivities()
    {
        var activities = new List<(DateTime TimestampUtc, RecentActivityItem Item)>();
        if (_logReader is null)
            return new List<RecentActivityItem>();

        foreach (var entry in _logReader.ReadEntries())
        {
            if (entry.TimestampUtc == default)
                continue;

            if (!ShouldShowOnDashboard(entry))
                continue;

            RecentActivityItem item = entry.Event.Category switch
            {
                LogEventCategory.Job when IsJobCrudAction(entry.Event.Action)
                    => CreateJobActivityItem(entry),
                LogEventCategory.Settings
                    => CreateSettingsActivityItem(entry),
                _ => CreateActivityItem(entry)
            };

            activities.Add((entry.TimestampUtc, item));
        }

        return activities
            .OrderByDescending(item => item.TimestampUtc)
            .Take(7)
            .Select(item => item.Item)
            .ToList();
    }


    /// <summary>
    /// Indicates whether the entry is a job summary row.
    /// </summary>
    /// <param name="entry">Entry to analyze.</param>
    /// <returns><c>true</c> when the entry is a summary.</returns>
    /// <summary>
    /// Indicates whether an entry represents a summary.
    /// </summary>
    /// <param name="entry">Entry to inspect.</param>
    /// <returns><c>true</c> when the entry is a summary.</returns>
    private static bool IsSummaryEntry(LogEntryDto entry)
    {
        if (entry.Summary is not null)
            return true;

        return entry.Event.Action == LogEventAction.Summary;
    }

    /// <summary>
    /// Filters which log entries are visible on the dashboard.
    /// </summary>
    /// <param name="entry">Entry to inspect.</param>
    /// <returns><c>true</c> when the entry should be displayed.</returns>
    private static bool ShouldShowOnDashboard(LogEntryDto entry)
    {
        if (IsSummaryEntry(entry))
            return true;

        return entry.Event.Category switch
        {
            LogEventCategory.Job => IsJobCrudAction(entry.Event.Action),
            LogEventCategory.Settings => true,
            _ => false
        };
    }

    /// <summary>
    /// Indicates whether an action is a job CRUD action.
    /// </summary>
    /// <param name="action">Action to inspect.</param>
    /// <returns><c>true</c> for job actions shown on the dashboard.</returns>
    private static bool IsJobCrudAction(LogEventAction action)
    {
        return action is LogEventAction.Create
            or LogEventAction.Update
            or LogEventAction.Delete
            or LogEventAction.Pause
            or LogEventAction.Resume
            or LogEventAction.Stop;
    }

    /// <summary>
    /// Builds a recent activity item for the UI.
    /// </summary>
    /// <param name="entry">Source entry.</param>
    /// <returns>Item ready for display.</returns>
    private RecentActivityItem CreateActivityItem(LogEntryDto entry)
    {
        var timestamp = entry.TimestampUtc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);
        var (glyph, color) = MapStatusGlyph(entry.Event.Action, entry.Event.Outcome);
        var title = string.IsNullOrWhiteSpace(entry.Job?.Name) ? Strings.Gui_Dashboard_Activity_Backup : entry.Job.Name;
        var subtitle = IsSummaryEntry(entry)
            ? BuildSummarySubtitle(entry)
            : BuildNonSummarySubtitle(entry);

        return new RecentActivityItem(
            title,
            subtitle,
            timestamp,
            glyph,
            ColorGlassOverlay,
            color);
    }

    private RecentActivityItem CreateJobActivityItem(LogEntryDto entry)
    {
        var timestamp = entry.TimestampUtc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);
        var title = string.IsNullOrWhiteSpace(entry.Job?.Name) ? Strings.Gui_Dashboard_Activity_Job : entry.Job.Name;
        var actionLabel = entry.Event.Action switch
        {
            LogEventAction.Create => Strings.Gui_Dashboard_Job_Created,
            LogEventAction.Update => Strings.Gui_Dashboard_Job_Updated,
            LogEventAction.Delete => Strings.Gui_Dashboard_Job_Deleted,
            LogEventAction.Pause => Strings.Gui_Dashboard_Job_Paused,
            LogEventAction.Resume => Strings.Gui_Dashboard_Job_Resumed,
            LogEventAction.Stop => Strings.Gui_Dashboard_Job_Stopped,
            _ => Strings.Gui_Dashboard_Job_Changed
        };

        var statusLabel = entry.Job?.IsActive == true ? Strings.Gui_Common_Active : Strings.Gui_Common_Inactive;
        var typeLabel = entry.Job?.Type switch
        {
            BackupType.Full => Strings.Gui_Common_Full,
            BackupType.Differential => Strings.Gui_Common_Differential,
            _ => Strings.Gui_Common_Backup
        };
        var subtitle = Truncate($"{actionLabel} | {typeLabel} | {statusLabel}", 140);
        var (glyph, color) = entry.Event.Action switch
        {
            LogEventAction.Create => ("+", ColorAccentGreen),
            LogEventAction.Update => ("~", ColorAccentBlue),
            LogEventAction.Delete => ("-", ColorNeutralGray),
            LogEventAction.Pause => ("||", ColorAccentOrange),
            LogEventAction.Resume => (">", ColorAccentGreen),
            LogEventAction.Stop => ("■", ColorNeutralGray),
            _ => ("J", ColorTextMuted)
        };

        return new RecentActivityItem(title, subtitle, timestamp, glyph, ColorGlassOverlay, color);
    }

    private RecentActivityItem CreateSettingsActivityItem(LogEntryDto entry)
    {
        var timestamp = entry.TimestampUtc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);
        var title = Strings.Gui_Dashboard_Settings_Title;
        var actionLabel = entry.Event.Action switch
        {
            LogEventAction.Update => Strings.Gui_Dashboard_Settings_Updated,
            _ => Strings.Gui_Dashboard_Settings_Saved
        };

        var languageLabel = entry.Settings?.Language switch
        {
            Language.English => Strings.Lang_English,
            Language.French => Strings.Lang_French,
            _ => Strings.Gui_Common_Unknown
        };
        var formatLabel = entry.Settings?.LogFormat switch
        {
            LogFormat.Json => Strings.Gui_LogFormat_Json,
            LogFormat.Xml => Strings.Gui_LogFormat_Xml,
            _ => Strings.Gui_Common_Unknown
        };
        var subtitle = Truncate($"{actionLabel} | {languageLabel} | {formatLabel}", 140);
        return new RecentActivityItem(title, subtitle, timestamp, "S", ColorGlassOverlay, ColorAccentOrange);
    }

    /// <summary>
    /// Builds the subtitle for a summary entry.
    /// </summary>
    /// <param name="entry">Source entry.</param>
    /// <returns>Subtitle text.</returns>
    private string BuildSummarySubtitle(LogEntryDto entry)
    {
        if (entry.Summary is null)
            return Truncate(entry.Message, 140);

        var typeLabel = ResolveBackupType(entry.Job?.Type, entry.Job?.Name);
        var sizeLabel = entry.Summary.TotalBytes > 0
            ? FormatBytes(entry.Summary.TotalBytes)
            : "0 B";
        var countsLabel = BuildCountsSummary(entry.Summary);
        var durationLabel = entry.Summary.DurationMs > 0
            ? FormatDuration(TimeSpan.FromMilliseconds(entry.Summary.DurationMs))
            : string.Empty;

        var transferredLabel = string.Format(Strings.Gui_Dashboard_Summary_Transferred, sizeLabel);
        var parts = new List<string> { typeLabel, transferredLabel };
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
        if (entry.Event.Outcome == LogEventOutcome.Failure)
        {
            var errorText = entry.Error?.Message ?? entry.Message;
            if (!string.IsNullOrWhiteSpace(errorText))
                return Truncate(string.Format(Strings.Gui_Dashboard_Error_Format, errorText), 140);
        }

        if (entry.Event.Action == LogEventAction.DirectoryCreated)
        {
            var targetName = Path.GetFileName(entry.File?.TargetPath);
            if (string.IsNullOrWhiteSpace(targetName))
                targetName = entry.File?.TargetPath ?? string.Empty;
            return Truncate(string.Format(Strings.Gui_Dashboard_DirectoryCreated_Format, targetName), 140);
        }

        var sourceName = Path.GetFileName(entry.File?.SourcePath);
        var targetNameFallback = Path.GetFileName(entry.File?.TargetPath);
        if (string.IsNullOrWhiteSpace(sourceName))
            sourceName = entry.File?.SourcePath ?? string.Empty;
        if (string.IsNullOrWhiteSpace(targetNameFallback))
            targetNameFallback = entry.File?.TargetPath ?? string.Empty;

        var sizeBytes = entry.File?.SizeBytes ?? 0;
        var sizeLabel = sizeBytes > 0 ? FormatBytes(sizeBytes) : "0 B";
        var action = entry.Event.Action == LogEventAction.Skip
            ? Strings.Gui_Dashboard_Skipped
            : Strings.Gui_Dashboard_FileCopy;

        if (string.IsNullOrWhiteSpace(sourceName) && string.IsNullOrWhiteSpace(targetNameFallback))
            return Truncate(entry.Message, 140);

        return Truncate($"{action} | {sizeLabel} | {sourceName} -> {targetNameFallback}", 140);
    }

    /// <summary>
    /// Resolves the backup type for a job name.
    /// </summary>
    /// <param name="jobName">Job name.</param>
    /// <returns>Backup type label.</returns>
    private string ResolveBackupType(BackupType? type, string? jobName)
    {
        if (type.HasValue)
        {
            return type.Value switch
            {
                BackupType.Full => Strings.Gui_Dashboard_Backup_Full,
                BackupType.Differential => Strings.Gui_Dashboard_Backup_Differential,
                _ => Strings.Gui_Dashboard_Backup_Generic
            };
        }

        if (_jobService is null || string.IsNullOrWhiteSpace(jobName))
            return Strings.Gui_Dashboard_Backup_Generic;

        var job = _jobService.GetAll()
            .FirstOrDefault(candidate => string.Equals(candidate.Name, jobName, StringComparison.Ordinal));

        return job?.Type switch
        {
            BackupType.Full => Strings.Gui_Dashboard_Backup_Full,
            BackupType.Differential => Strings.Gui_Dashboard_Backup_Differential,
            _ => Strings.Gui_Dashboard_Backup_Generic
        };
    }

    private static string ResolveJobStatusLabel(JobStatus status)
    {
        return status switch
        {
            JobStatus.Idle => Strings.Gui_JobStatus_Idle,
            JobStatus.Running => Strings.Gui_JobStatus_Running,
            JobStatus.Paused => Strings.Gui_JobStatus_Paused,
            JobStatus.Completed => Strings.Gui_JobStatus_Completed,
            JobStatus.Error => Strings.Gui_JobStatus_Error,
            _ => Strings.Gui_Common_Unknown
        };
    }

    /// <summary>
    /// Extracts counters from a summary object.
    /// </summary>
    /// <param name="summary">Summary counters.</param>
    /// <returns>Counter summary text.</returns>
    private static string BuildCountsSummary(LogSummaryDto summary)
    {
        var parts = new List<string>
        {
            string.Format(CultureInfo.CurrentCulture, Strings.Gui_Dashboard_Summary_Copied, summary.CopiedCount),
            string.Format(CultureInfo.CurrentCulture, Strings.Gui_Dashboard_Summary_Skipped, summary.SkippedCount),
            string.Format(CultureInfo.CurrentCulture, Strings.Gui_Dashboard_Summary_Errors, summary.ErrorCount)
        };

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Maps an action/outcome to a glyph and color.
    /// </summary>
    /// <param name="action">Action to interpret.</param>
    /// <param name="outcome">Outcome to interpret.</param>
    /// <returns>Glyph and color.</returns>
    private static (string glyph, string color) MapStatusGlyph(LogEventAction action, LogEventOutcome outcome)
    {
        if (outcome == LogEventOutcome.Failure)
            return ("!", ColorAccentRed);

        return action switch
        {
            LogEventAction.Skip => ("-", ColorAccentOrange),
            LogEventAction.DirectoryCreated => ("+", ColorAccentBlue),
            LogEventAction.Update => ("~", ColorAccentBlue),
            LogEventAction.Delete => ("-", ColorNeutralGray),
            LogEventAction.Create => ("+", ColorAccentGreen),
            LogEventAction.Pause => ("||", ColorAccentOrange),
            LogEventAction.Resume => (">", ColorAccentGreen),
            LogEventAction.Stop => ("■", ColorNeutralGray),
            _ => ("✓", ColorAccentGreen)
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
        _logRefreshCts?.Cancel();
        _logRefreshCts?.Dispose();
        _logRefreshCts = null;
        if (_backupService != null)
        {
            _backupService.StateChanged -= OnStateChanged;
        }

        GC.SuppressFinalize(this);
    }
}
