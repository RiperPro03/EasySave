using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Models;
using EasySave.Core.Resources;

namespace EasySave.App.Gui.Models;

/// <summary>
/// UI-friendly wrapper around job execution state.
/// </summary>
public sealed partial class ExecutionJobItem : ObservableObject
{
    public ExecutionJobItem(BackupJob job)
        : this(job.Id, job.Name)
    {
        IsActive = job.IsActive;
        BackupTypeValue = job.Type;
    }

    public ExecutionJobItem(string jobId, string jobName)
    {
        JobId = jobId;
        JobName = jobName;
        Status = JobStatus.Idle;
    }

    [ObservableProperty]
    private string _jobId = string.Empty;

    [ObservableProperty]
    private string _jobName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPlayPause))]
    [NotifyPropertyChangedFor(nameof(CanPlay))]
    private bool _isActive;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackupTypeLabel))]
    private BackupType? _backupTypeValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusLabel))]
    [NotifyPropertyChangedFor(nameof(PlayPauseLabel))]
    [NotifyPropertyChangedFor(nameof(IsRunning))]
    [NotifyPropertyChangedFor(nameof(IsPaused))]
    [NotifyPropertyChangedFor(nameof(IsIdle))]
    [NotifyPropertyChangedFor(nameof(IsCompleted))]
    [NotifyPropertyChangedFor(nameof(IsError))]
    [NotifyPropertyChangedFor(nameof(CanPlayPause))]
    [NotifyPropertyChangedFor(nameof(CanPlay))]
    [NotifyPropertyChangedFor(nameof(CanPause))]
    [NotifyPropertyChangedFor(nameof(CanStop))]
    [NotifyPropertyChangedFor(nameof(CanStart))]
    private JobStatus _status;

    [ObservableProperty]
    private string? _currentSourceFile;

    [ObservableProperty]
    private string? _currentTargetFile;

    [ObservableProperty]
    private long _totalFiles;

    [ObservableProperty]
    private long _filesProcessed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalSizeMbLabel))]
    private long _totalSizeBytes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SizeProcessedMbLabel))]
    private long _sizeProcessedBytes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressLabel))]
    private int _progressPercentage;

    [ObservableProperty]
    private long _remainingFiles;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RemainingSizeMbLabel))]
    private long _remainingSizeBytes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LastActionLabel))]
    private DateTime _lastActionTimestampUtc;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string? _errorMessage;

    public bool IsRunning => Status == JobStatus.Running;
    public bool IsPaused => Status == JobStatus.Paused;
    public bool IsIdle => Status == JobStatus.Idle;
    public bool IsCompleted => Status == JobStatus.Completed;
    public bool IsError => Status == JobStatus.Error;

    public string StatusLabel => Status switch
    {
        JobStatus.Idle => Strings.Gui_JobStatus_Idle,
        JobStatus.Running => Strings.Gui_JobStatus_Running,
        JobStatus.Paused => Strings.Gui_JobStatus_Paused,
        JobStatus.Completed => Strings.Gui_JobStatus_Completed,
        JobStatus.Error => Strings.Gui_JobStatus_Error,
        _ => Strings.Gui_Common_Unknown
    };

    public string BackupTypeLabel => BackupTypeValue switch
    {
        BackupType.Full => Strings.Gui_Common_Full,
        BackupType.Differential => Strings.Gui_Common_Differential,
        _ => Strings.Gui_Common_NotAvailable
    };

    public string PlayPauseLabel => Status switch
    {
        JobStatus.Running => Strings.Gui_Execution_Pause,
        JobStatus.Paused => Strings.Gui_Execution_Resume,
        _ => Strings.Gui_Execution_Play
    };

    public bool CanPlayPause => IsRunning || IsPaused || IsActive;
    public bool CanPlay => IsPaused || CanStart;
    public bool CanPause => IsRunning;
    public bool CanStop => IsRunning || IsPaused;
    public bool CanStart => IsActive && (IsIdle || IsCompleted || IsError);

    public string ProgressLabel => string.Format(CultureInfo.CurrentCulture, "{0}%", ProgressPercentage);
    public string SizeProcessedMbLabel => FormatMegabytes(SizeProcessedBytes);
    public string TotalSizeMbLabel => FormatMegabytes(TotalSizeBytes);
    public string RemainingSizeMbLabel => FormatMegabytes(RemainingSizeBytes);

    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string LastActionLabel
        => LastActionTimestampUtc == default
            ? Strings.Gui_Common_NotAvailable
            : LastActionTimestampUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");

    public void UpdateDefinition(BackupJob job)
    {
        JobName = job.Name;
        IsActive = job.IsActive;
        BackupTypeValue = job.Type;
    }

    public void UpdateFromState(JobStateDto state)
    {
        JobName = state.JobName;
        Status = state.Status;
        CurrentSourceFile = state.CurrentSourceFile;
        CurrentTargetFile = state.CurrentTargetFile;
        TotalFiles = state.TotalFiles;
        FilesProcessed = state.FilesProcessed;
        TotalSizeBytes = state.TotalSizeBytes;
        SizeProcessedBytes = state.SizeProcessedBytes;
        ProgressPercentage = state.ProgressPercentage;
        RemainingFiles = state.RemainingFiles;
        RemainingSizeBytes = state.RemainingSizeBytes;
        LastActionTimestampUtc = state.LastActionTimestampUtc;
        ErrorMessage = state.ErrorMessage;
    }

    /// <summary>
    /// Formate une taille en Mo pour l'affichage de la vue d'execution.
    /// </summary>
    private static string FormatMegabytes(long bytes)
    {
        if (bytes <= 0)
            return "0 MB";

        var megabytes = bytes / (1024d * 1024d);
        return string.Format(CultureInfo.CurrentCulture, "{0:0.##} MB", megabytes);
    }
}
