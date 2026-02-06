using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Events;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using EasySave.EasyLog.Options;

namespace EasySave.App.Services;

public sealed class BackupService : IBackupService
{
    private IBackupEngine _backupEngine;
    private readonly IJobService _jobService;
    private readonly IStateWriter _stateWriter;
    private readonly Dictionary<string, JobStateDto> _jobStates = new();
    private readonly object _stateLock = new();
    private readonly string? _logDirectory;
    private readonly Func<LogFormat> _logFormatProvider;
    private LogFormat _currentLogFormat;

    public event EventHandler<JobStateChangedEventArgs>? StateChanged;

    public BackupService(
        IJobService jobService,
        string? logDirectory = null,
        IStateWriter? stateWriter = null,
        IPathProvider? pathProvider = null,
        LogFormat? logFormat = null,
        Func<LogFormat>? logFormatProvider = null)
    {
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _stateWriter = stateWriter ?? new StateWriter(pathProvider ?? new PathProvider());
        _logDirectory = logDirectory;
        _logFormatProvider = logFormatProvider ?? (() => logFormat ?? LogFormat.Json);
        _currentLogFormat = _logFormatProvider();
        _backupEngine = CreateEngine(_currentLogFormat);
        _backupEngine.StateChanged += OnEngineStateChanged;
        InitializeSnapshot();
    }

    public BackupResultDto Run(BackupJob job)
    {
        EnsureEngine();
        var result = _backupEngine.Run(job);
        _jobService.MarkExecuted(job.Id);
        return result;
    }

    private void OnEngineStateChanged(object? sender, JobStateChangedEventArgs e)
    {
        lock (_stateLock)
        {
            SyncJobs();
            _jobStates[e.State.JobId] = CopyState(e.State);
            WriteSnapshot();
        }

        StateChanged?.Invoke(this, e);
    }

    private void InitializeSnapshot()
    {
        lock (_stateLock)
        {
            SyncJobs();
            WriteSnapshot();
        }
    }

    private void SyncJobs()
    {
        var jobs = _jobService.GetAll();
        var jobIds = new HashSet<string>(jobs.Select(job => job.Id));

        foreach (var job in jobs)
        {
            if (!_jobStates.TryGetValue(job.Id, out var state))
            {
                _jobStates[job.Id] = CreateIdleState(job);
            }
            else if (!string.Equals(state.JobName, job.Name, StringComparison.Ordinal))
            {
                state.JobName = job.Name;
            }
        }

        var staleIds = _jobStates.Keys.Where(id => !jobIds.Contains(id)).ToList();
        foreach (var id in staleIds)
        {
            _jobStates.Remove(id);
        }
    }

    private void WriteSnapshot()
    {
        var states = _jobStates.Values.ToList();
        var snapshot = new AppStateDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            TotalJobs = states.Count,
            GlobalStatus = ComputeGlobalStatus(states),
            ActiveJobIds = states
                .Where(state => state.Status is JobStatus.Running or JobStatus.Paused)
                .Select(state => state.JobId)
                .ToList(),
            Jobs = states
                .OrderBy(state => state.JobId, StringComparer.Ordinal)
                .Select(CopyState)
                .ToList()
        };

        _stateWriter.Write(snapshot);
    }

    private static JobStatus ComputeGlobalStatus(IReadOnlyList<JobStateDto> states)
    {
        if (states.Count == 0)
            return JobStatus.Idle;
        if (states.Any(state => state.Status == JobStatus.Error))
            return JobStatus.Error;
        if (states.Any(state => state.Status == JobStatus.Running))
            return JobStatus.Running;
        if (states.Any(state => state.Status == JobStatus.Paused))
            return JobStatus.Paused;
        if (states.Any(state => state.Status == JobStatus.Completed))
            return JobStatus.Completed;
        return JobStatus.Idle;
    }

    private static JobStateDto CreateIdleState(BackupJob job)
    {
        return new JobStateDto
        {
            JobId = job.Id,
            JobName = job.Name,
            Status = JobStatus.Idle,
            LastActionTimestampUtc = DateTime.UtcNow
        };
    }

    private static JobStateDto CopyState(JobStateDto state)
    {
        return new JobStateDto
        {
            JobId = state.JobId,
            JobName = state.JobName,
            Status = state.Status,
            CurrentSourceFile = state.CurrentSourceFile,
            CurrentTargetFile = state.CurrentTargetFile,
            TotalFiles = state.TotalFiles,
            FilesProcessed = state.FilesProcessed,
            TotalSizeBytes = state.TotalSizeBytes,
            SizeProcessedBytes = state.SizeProcessedBytes,
            ProgressPercentage = state.ProgressPercentage,
            RemainingFiles = state.RemainingFiles,
            RemainingSizeBytes = state.RemainingSizeBytes,
            LastActionTimestampUtc = state.LastActionTimestampUtc,
            ErrorMessage = state.ErrorMessage
        };
    }

    private void EnsureEngine()
    {
        var desiredFormat = _logFormatProvider();
        if (desiredFormat == _currentLogFormat)
            return;

        _backupEngine.StateChanged -= OnEngineStateChanged;
        _currentLogFormat = desiredFormat;
        _backupEngine = CreateEngine(_currentLogFormat);
        _backupEngine.StateChanged += OnEngineStateChanged;
    }

    private IBackupEngine CreateEngine(LogFormat format)
    {
        return new BackupEngine(_logDirectory, format);
    }
}
