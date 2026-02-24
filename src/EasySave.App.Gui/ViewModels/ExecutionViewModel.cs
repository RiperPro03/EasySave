using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.App.Gui.Models;
using EasySave.Core.Events;
using EasySave.Core.Interfaces;
using EasySave.Core.Resources;

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

    [ObservableProperty]
    private bool _canPauseAll;

    [ObservableProperty]
    private bool _canStopAll;

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
        RefreshGlobalControls();
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task PlayAsync(ExecutionJobItem? item)
    {
        if (item is null)
            return;
        if (_backupService is null || _jobService is null)
            return;

        LastError = null;

        if (item.IsRunning)
            return;

        if (item.IsPaused)
        {
            if (!_backupService.Resume(item.JobId))
                LastError = Strings.Gui_Execution_Error_Resume;
            return;
        }

        var job = _jobService.GetById(item.JobId);
        if (job is null)
        {
            LastError = string.Format(Strings.Gui_Execution_Error_NotFound, item.JobId);
            return;
        }

        try
        {
            var result = await Task.Run(() => _backupService.Run(job));
            if (!result.Success && !string.IsNullOrWhiteSpace(result.Message))
            {
                LastError = result.Message;
            }
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
        if (!TryPrepareRunAll(out var pausedItems, out var jobsToRun))
            return;

        try
        {
            var backgroundError = await Task.Run(() => RunAllInBackground(pausedItems, jobsToRun));

            if (backgroundError is not null)
                LastError = backgroundError;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
        }
    }

    [RelayCommand]
    private void Pause(ExecutionJobItem? item)
    {
        if (item is null)
            return;
        if (_backupService is null)
            return;

        LastError = null;
        if (!_backupService.Pause(item.JobId))
        {
            LastError = Strings.Gui_Execution_Error_Pause;
        }
    }

    /// <summary>
    /// Validates the global run request and prepares paused/runnable job lists.
    /// </summary>
    /// <param name="pausedItems">Paused jobs to resume first.</param>
    /// <param name="jobsToRun">Jobs that can be started now.</param>
    /// <returns><c>true</c> when there is at least one action to perform.</returns>
    private bool TryPrepareRunAll(out List<ExecutionJobItem> pausedItems, out List<EasySave.Core.Models.BackupJob> jobsToRun)
    {
        pausedItems = new List<ExecutionJobItem>();
        jobsToRun = new List<EasySave.Core.Models.BackupJob>();

        if (_backupService is null || _jobService is null)
            return false;

        if (!_backupService.CanStartSequence(out var reason))
        {
            // Blocage global (ex: business software detecte) avant toute reprise/lancement.
            LastError = string.IsNullOrWhiteSpace(reason)
                ? Strings.Gui_Execution_Error_BusinessSoftware
                : reason;
            return false;
        }

        RefreshJobs();

        pausedItems = Jobs
            .Where(item => item.IsPaused)
            .ToList();

        // On capture les IDs de la vue pour rester aligne avec l'etat UI courant (active/inactive, status).
        var runnableIds = Jobs
            .Where(item => item.CanStart)
            .Select(item => item.JobId)
            .ToHashSet(StringComparer.Ordinal);

        jobsToRun = _jobService.GetAll()
            .Where(job => runnableIds.Contains(job.Id))
            .ToList();

        if (pausedItems.Count != 0 || jobsToRun.Count != 0)
            return true;

        // Rien a faire: aucun job en pause a reprendre et aucun job demarrable.
        LastError = Strings.Gui_Execution_Error_NoRunnable;
        return false;
    }

    /// <summary>
    /// Executes the \"run all\" workflow on a background thread and returns the first user-facing error message, if any.
    /// </summary>
    private string? RunAllInBackground(
        IReadOnlyList<ExecutionJobItem> pausedItems,
        IReadOnlyList<EasySave.Core.Models.BackupJob> jobsToRun)
    {
        if (_backupService is null)
            return null;

        // L'ordre est intentionnel: on reprend d'abord les pauses, puis on lance les nouveaux jobs.
        ResumePausedJobs(pausedItems);
        return StartRunnableJobs(jobsToRun);
    }

    /// <summary>
    /// Resumes all paused jobs selected for a global run.
    /// </summary>
    /// <param name="pausedItems">Paused jobs to resume.</param>
    private void ResumePausedJobs(IReadOnlyList<ExecutionJobItem> pausedItems)
    {
        if (_backupService is null)
            return;

        foreach (var item in pausedItems)
        {
            _backupService.Resume(item.JobId);
        }
    }

    /// <summary>
    /// Starts all runnable jobs and returns the first non-empty error message produced by the service.
    /// </summary>
    /// <param name="jobsToRun">Jobs to start.</param>
    /// <returns>The first error message, or <c>null</c> when none was reported.</returns>
    private string? StartRunnableJobs(IReadOnlyList<EasySave.Core.Models.BackupJob> jobsToRun)
    {
        if (_backupService is null)
            return null;

        string? firstError = null;
        foreach (var job in jobsToRun)
        {
            // Revalide avant chaque lancement: le blocage global peut apparaitre pendant la sequence.
            if (!_backupService.CanStartSequence(out _))
                break;

            var result = _backupService.Run(job);
            // On conserve uniquement le premier message utile pour l'affichage UI.
            if (result.Success || firstError is not null || string.IsNullOrWhiteSpace(result.Message))
                continue;

            firstError = result.Message;
        }

        return firstError;
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
            LastError = Strings.Gui_Execution_Error_Stop;
        }
    }

    [RelayCommand]
    private void PauseAll()
    {
        if (_backupService is null)
            return;

        LastError = null;
        foreach (var item in Jobs.Where(job => job.IsRunning).ToList())
        {
            _backupService.Pause(item.JobId);
        }
    }

    [RelayCommand]
    private void StopAll()
    {
        if (_backupService is null)
            return;

        LastError = null;
        foreach (var item in Jobs.Where(job => job.IsRunning || job.IsPaused).ToList())
        {
            _backupService.Stop(item.JobId);
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
        RefreshGlobalControls();
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
        RefreshGlobalControls();
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

    /// <summary>
    /// Refreshes the enabled state of global pause/stop buttons based on live job statuses.
    /// </summary>
    private void RefreshGlobalControls()
    {
        // "Pause all" n'a de sens que s'il existe au moins un job en cours.
        CanPauseAll = Jobs.Any(job => job.IsRunning);
        // "Stop all" s'applique aussi aux jobs deja en pause.
        CanStopAll = Jobs.Any(job => job.IsRunning || job.IsPaused);
    }
}
