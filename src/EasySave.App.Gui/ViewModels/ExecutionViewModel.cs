using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.App.Gui.Models;
using EasySave.Core.Events;
using EasySave.Core.Interfaces;

namespace EasySave.App.Gui.ViewModels;

/// <summary>
/// View model for the live execution page.
/// </summary>
public sealed partial class ExecutionViewModel : ViewModelBase, IDisposable
{
    private readonly IJobService? _jobService;
    private readonly IBackupService? _backupService;
    private readonly SynchronizationContext? _uiContext;
    private bool _disposed;

    public ObservableCollection<ExecutionJobItem> Jobs { get; } = new();

    [ObservableProperty]
    private bool _hasJobs;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _lastError;

    /// <summary>
    /// Design-time constructor.
    /// </summary>
    public ExecutionViewModel()
    {
        _uiContext = SynchronizationContext.Current;
        SeedSampleJobs();
    }

    /// <summary>
    /// Runtime constructor.
    /// </summary>
    public ExecutionViewModel(IJobService jobService, IBackupService backupService)
    {
        _uiContext = SynchronizationContext.Current;
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        RefreshJobs();
        _backupService.StateChanged += OnStateChanged;
    }

    /// <summary>
    /// Refreshes the job list from the repository.
    /// </summary>
    public void RefreshJobs()
    {
        if (_jobService is null)
            return;

        var jobs = _jobService.GetAll();
        var existing = Jobs.ToDictionary(item => item.JobId, StringComparer.Ordinal);

        foreach (var job in jobs)
        {
            if (existing.TryGetValue(job.Id, out var item))
            {
                item.UpdateDefinition(job);
            }
            else
            {
                Jobs.Add(new ExecutionJobItem(job));
            }
        }

        var stale = Jobs.Where(item => jobs.All(job => job.Id != item.JobId)).ToList();
        foreach (var item in stale)
        {
            Jobs.Remove(item);
        }

        HasJobs = Jobs.Count > 0;
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task TogglePlayPauseAsync(ExecutionJobItem? item)
    {
        if (item is null)
            return;
        if (_backupService is null || _jobService is null)
            return;

        LastError = null;

        if (item.IsRunning)
        {
            if (!_backupService.Pause(item.JobId))
                LastError = "Unable to pause job.";
            return;
        }

        if (item.IsPaused)
        {
            if (!_backupService.Resume(item.JobId))
                LastError = "Unable to resume job.";
            return;
        }

        var job = _jobService.GetById(item.JobId);
        if (job is null)
        {
            LastError = $"Job with ID {item.JobId} not found.";
            return;
        }

        try
        {
            await Task.Run(() => _backupService.Run(job));
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task RunAllAsync()
    {
        if (_backupService is null || _jobService is null)
            return;

        LastError = null;
        RefreshJobs();

        var runnableIds = Jobs
            .Where(item => item.CanStart)
            .Select(item => item.JobId)
            .ToHashSet(StringComparer.Ordinal);

        var jobsToRun = _jobService.GetAll()
            .Where(job => runnableIds.Contains(job.Id))
            .ToList();

        if (jobsToRun.Count == 0)
        {
            LastError = "No runnable jobs.";
            return;
        }

        try
        {
            await Task.Run(() =>
            {
                foreach (var job in jobsToRun)
                {
                    _backupService.Run(job);
                }
            });
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
        }
    }

    [RelayCommand]
    private void Stop(ExecutionJobItem? item)
    {
        if (item is null)
            return;
        if (_backupService is null)
            return;

        LastError = null;

        if (!_backupService.Stop(item.JobId))
        {
            LastError = "Unable to stop job.";
        }
    }

    private void OnStateChanged(object? sender, JobStateChangedEventArgs e)
    {
        if (_uiContext != null)
        {
            _uiContext.Post(_ => HandleStateChanged(e), null);
            return;
        }

        HandleStateChanged(e);
    }

    private void HandleStateChanged(JobStateChangedEventArgs e)
    {
        var item = Jobs.FirstOrDefault(job => job.JobId == e.State.JobId);
        if (item == null)
        {
            item = new ExecutionJobItem(e.State.JobId, e.State.JobName);
            Jobs.Add(item);
        }

        if (item.BackupTypeValue is null && _jobService != null)
        {
            var job = _jobService.GetById(e.State.JobId);
            if (job != null)
            {
                item.UpdateDefinition(job);
            }
        }

        item.UpdateFromState(e.State);
        HasJobs = Jobs.Count > 0;
    }

    private void SeedSampleJobs()
    {
        var running = new ExecutionJobItem("1", "Documents")
        {
            IsActive = true,
            BackupTypeValue = EasySave.Core.Enums.BackupType.Full,
            Status = EasySave.Core.Enums.JobStatus.Running,
            ProgressPercentage = 42,
            FilesProcessed = 84,
            TotalFiles = 200,
            RemainingFiles = 116
        };

        var idle = new ExecutionJobItem("2", "Photos")
        {
            IsActive = true,
            BackupTypeValue = EasySave.Core.Enums.BackupType.Differential
        };

        Jobs.Add(running);
        Jobs.Add(idle);
        HasJobs = Jobs.Count > 0;
    }

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

    public bool HasError => !string.IsNullOrWhiteSpace(LastError);
}
