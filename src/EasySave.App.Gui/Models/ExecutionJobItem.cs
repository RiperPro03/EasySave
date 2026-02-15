using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Models;

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
    private long _totalSizeBytes;

    [ObservableProperty]
    private long _sizeProcessedBytes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressLabel))]
    private int _progressPercentage;

    [ObservableProperty]
    private long _remainingFiles;

    [ObservableProperty]
    private long _remainingSizeBytes;

    [ObservableProperty]
    private DateTime _lastActionTimestampUtc;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string? _errorMessage;

    public bool IsRunning => Status == JobStatus.Running;
    public bool IsPaused => Status == JobStatus.Paused;
    public bool IsIdle => Status == JobStatus.Idle;
    public bool IsCompleted => Status == JobStatus.Completed;
    public bool IsError => Status == JobStatus.Error;

    public string StatusLabel => Status.ToString();

    public string BackupTypeLabel => BackupTypeValue switch
    {
        BackupType.Full => "Full",
        BackupType.Differential => "Differential",
        _ => "-"
    };

    public string PlayPauseLabel => Status switch
    {
        JobStatus.Running => "Pause",
        JobStatus.Paused => "Resume",
        _ => "Play"
    };

    public bool CanPlayPause => IsRunning || IsPaused || IsActive;
    public bool CanStop => IsRunning || IsPaused;
    public bool CanStart => IsActive && (IsIdle || IsCompleted || IsError);

    public string ProgressLabel => $"{ProgressPercentage}%";

    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

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
}
