using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Events;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using EasySave.EasyLog.Options;

namespace EasySave.App.Services;

/// <summary>
/// Coordinates backup execution and publishes state snapshots.
/// </summary>
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

    /// <summary>
    /// Raised when job state changes during execution.
    /// </summary>
    public event EventHandler<JobStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupService"/> class.
    /// </summary>
    /// <param name="jobService">Service used to manage jobs.</param>
    /// <param name="logDirectory">Optional log directory.</param>
    /// <param name="stateWriter">Optional state writer override.</param>
    /// <param name="pathProvider">Optional path provider for default state writer.</param>
    /// <param name="logFormat">Optional fixed log format.</param>
    /// <param name="logFormatProvider">Optional log format provider for runtime changes.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jobService"/> is null.</exception>
    public BackupService(
        IJobService jobService,
        string? logDirectory = null,
        IStateWriter? stateWriter = null,
        IPathProvider? pathProvider = null,
        LogFormat? logFormat = null,
        Func<LogFormat>? logFormatProvider = null)
    {
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        // Si aucun writer n'est fourni, on cree celui par defaut avec un PathProvider local.
        _stateWriter = stateWriter ?? new StateWriter(pathProvider ?? new PathProvider());
        _logDirectory = logDirectory;
        // La source du format de log peut changer a l'execution (GUI/CLI).
        _logFormatProvider = logFormatProvider ?? (() => logFormat ?? LogFormat.Json);
        _currentLogFormat = _logFormatProvider();
        _backupEngine = CreateEngine(_currentLogFormat);
        _backupEngine.StateChanged += OnEngineStateChanged;
        // Publie un snapshot initial pour exposer l'etat au demarrage.
        InitializeSnapshot();
    }

    /// <summary>
    /// Executes a backup job and records its last run time.
    /// </summary>
    /// <param name="job">The job to execute.</param>
    /// <returns>The execution result.</returns>
    public BackupResultDto Run(BackupJob job)
    {
        // Reconfigure l'engine si le format de log a change.
        EnsureEngine();
        var result = _backupEngine.Run(job);
        // Marque le job comme execute pour conserver l'horodatage.
        _jobService.MarkExecuted(job.Id);
        return result;
    }

    /// <summary>
    /// Handles state updates from the engine and writes snapshots.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">State change event arguments.</param>
    private void OnEngineStateChanged(object? sender, JobStateChangedEventArgs e)
    {
        lock (_stateLock)
        {
            // Synchronise les jobs avant d'enregistrer le nouvel etat.
            SyncJobs();
            _jobStates[e.State.JobId] = CopyState(e.State);
            WriteSnapshot();
        }

        StateChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Initializes the state snapshot at startup.
    /// </summary>
    private void InitializeSnapshot()
    {
        lock (_stateLock)
        {
            // Cree un premier snapshot base sur les jobs existants.
            SyncJobs();
            WriteSnapshot();
        }
    }

    /// <summary>
    /// Syncs in-memory state with current job definitions.
    /// </summary>
    private void SyncJobs()
    {
        var jobs = _jobService.GetAll();
        var jobIds = new HashSet<string>(jobs.Select(job => job.Id));

        foreach (var job in jobs)
        {
            if (!_jobStates.TryGetValue(job.Id, out var state))
            {
                // Ajoute les jobs inconnus avec un etat Idle.
                _jobStates[job.Id] = CreateIdleState(job);
            }
            else if (!string.Equals(state.JobName, job.Name, StringComparison.Ordinal))
            {
                // Met a jour le nom si le job à ete renomme.
                state.JobName = job.Name;
            }
        }

        var staleIds = _jobStates.Keys.Where(id => !jobIds.Contains(id)).ToList();
        foreach (var id in staleIds)
        {
            // Supprime les jobs qui n'existent plus dans le repository.
            _jobStates.Remove(id);
        }
    }

    /// <summary>
    /// Writes the aggregated application state snapshot.
    /// </summary>
    private void WriteSnapshot()
    {
        var states = _jobStates.Values.ToList();
        // Agrege les etats pour produire un snapshot global.
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

    /// <summary>
    /// Computes a global status based on per-job states.
    /// </summary>
    /// <param name="states">The list of job states.</param>
    /// <returns>The aggregate status.</returns>
    private static JobStatus ComputeGlobalStatus(IReadOnlyList<JobStateDto> states)
    {
        // Priorise les statuts les plus critiques.
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

    /// <summary>
    /// Creates an idle state snapshot for a job.
    /// </summary>
    /// <param name="job">The job to represent.</param>
    /// <returns>An idle state snapshot.</returns>
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

    /// <summary>
    /// Creates a copy of a state snapshot.
    /// </summary>
    /// <param name="state">The source state.</param>
    /// <returns>A copy of the state.</returns>
    private static JobStateDto CopyState(JobStateDto state)
    {
        // Copie pour eviter des modifications concurrentes.
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

    /// <summary>
    /// Recreates the engine if the log format has changed.
    /// </summary>
    private void EnsureEngine()
    {
        var desiredFormat = _logFormatProvider();
        if (desiredFormat == _currentLogFormat)
            return;

        // Reinstancie l'engine pour appliquer le nouveau format de log.
        _backupEngine.StateChanged -= OnEngineStateChanged;
        _currentLogFormat = desiredFormat;
        _backupEngine = CreateEngine(_currentLogFormat);
        _backupEngine.StateChanged += OnEngineStateChanged;
    }

    /// <summary>
    /// Creates a backup engine instance for the specified format.
    /// </summary>
    /// <param name="format">The desired log format.</param>
    /// <returns>A configured backup engine.</returns>
    private IBackupEngine CreateEngine(LogFormat format)
    {
        return new BackupEngine(_logDirectory, format);
    }
}
