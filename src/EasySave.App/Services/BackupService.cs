using EasySave.App.Utils;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Events;
using EasySave.Core.Interfaces;
using EasySave.Core.Logging;
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
    private readonly AppConfig _config;
    private readonly Dictionary<string, JobStateDto> _jobStates = new();
    private readonly object _stateLock = new();
    private readonly IAppLogService? _logService;

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
    /// <param name="logService">Optional log service override.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jobService"/> is null.</exception>
    public BackupService(
        IJobService jobService,
        AppConfig config,
        string? logDirectory = null,
        IStateWriter? stateWriter = null,
        IPathProvider? pathProvider = null,
        LogFormat? logFormat = null,
        Func<LogFormat>? logFormatProvider = null,
        IAppLogService? logService = null)
    {
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        // Si aucun writer n'est fourni, on cree celui par defaut avec un PathProvider local.
        _stateWriter = stateWriter ?? new StateWriter(pathProvider ?? new PathProvider());
        // La source du format de log peut changer a l'execution (GUI/CLI).
        var formatProvider = logFormatProvider ?? (() => logFormat ?? LogFormat.Json);
        _logService = logService ?? (string.IsNullOrWhiteSpace(logDirectory)
            ? null
            : new AppLogService(logDirectory, formatProvider));
        _backupEngine = CreateEngine();
        _backupEngine.StateChanged += OnEngineStateChanged;
        // Publie un snapshot initial pour exposer l'etat au demarrage.
        InitializeSnapshot();
    }

    public Task<BackupResultDto> RunAsync(BackupJob job)
    {
        return Task.Run(() => Run(job));
    }

    /// <summary>
    /// Executes a backup job and records its last run time.
    /// </summary>
    /// <param name="job">The job to execute.</param>
    /// <returns>The execution result.</returns>
    public BackupResultDto Run(BackupJob job)
    {
        if (job is null)
            throw new ArgumentNullException(nameof(job));

        if (!job.IsActive)
        {
            WriteInactiveJobLog(job);
            return new BackupResultDto
            {
                Success = false,
                Message = "Job is inactive and was skipped.",
                Duration = TimeSpan.Zero
            };
        }

        if (IsBusinessSoftwareRunning(out var processName))
        {
            var message = $"Business software '{processName}' is currently running. Cannot start work.";
            LogBusinessSoftwareBlocked(processName, job);
            return new BackupResultDto
            {
                Success = false,
                Message = message,
                Duration = TimeSpan.Zero,
                ErrorCount = 1,
                Errors = new List<string> { message }
            };
        }

        JobStateDto? startedState = null;
        lock (_stateLock)
        {
            // Bloque le lancement si le job est deja en cours ou en pause.
            SyncJobs();
            if (_jobStates.TryGetValue(job.Id, out var existing) &&
                existing.Status is JobStatus.Running or JobStatus.Paused)
            {
                return new BackupResultDto
                {
                    Success = false,
                    Message = "Job is already running or paused.",
                    Duration = TimeSpan.Zero
                };
            }

            // Publie un etat "Running" immediat pour eviter les doubles lancements.
            _jobStates[job.Id] = new JobStateDto
            {
                JobId = job.Id,
                JobName = job.Name,
                Status = JobStatus.Running,
                LastActionTimestampUtc = DateTime.UtcNow
            };
            WriteSnapshot();
            startedState = CopyState(_jobStates[job.Id]);
        }

        if (startedState != null)
        {
            StateChanged?.Invoke(this, new JobStateChangedEventArgs(startedState));
        }

        var result = _backupEngine.Run(job);
        // Marque le job comme execute pour conserver l'horodatage.
        _jobService.MarkExecuted(job.Id);
        return result;
    }

    /// <summary>
    /// Requests a pause for a running job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns><c>true</c> when the pause request was accepted.</returns>
    public bool Pause(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return false;

        return _backupEngine.Pause(jobId);
    }

    /// <summary>
    /// Requests a resume for a paused job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns><c>true</c> when the resume request was accepted.</returns>
    public bool Resume(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return false;

        return _backupEngine.Resume(jobId);
    }

    /// <summary>
    /// Requests a stop for a running job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns><c>true</c> when the stop request was accepted.</returns>
    public bool Stop(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return false;

        return _backupEngine.Stop(jobId);
    }

    public bool IsBusinessSoftwareRunning(out string? processName)
    {
        processName = _config.BusinessSoftwareProcessName;
        if (string.IsNullOrWhiteSpace(processName))
            return false;

        return BusinessSoftwareDetector.IsRunning(processName);
    }

    public bool CanStartSequence(out string? reason)
    {
        if (!IsBusinessSoftwareRunning(out var processName))
        {
            reason = null;
            return true;
        }

        reason = $"Business software '{processName}' is currently running. Cannot start sequence.";
        LogBusinessSoftwareBlocked(processName, job: null);
        return false;
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
    /// Creates a backup engine instance.
    /// </summary>
    /// <returns>A configured backup engine.</returns>
    private IBackupEngine CreateEngine()
    {
        return new BackupEngine(_config, _logService);
    }

    private void WriteInactiveJobLog(BackupJob job)
    {
        if (_logService is null)
            return;

        var entry = LogEntryBuilder.Create(
                eventName: "job.skipped",
                category: LogEventCategory.Job,
                action: LogEventAction.Skip,
                message: "Job is inactive and was skipped")
            .WithLevel(LogLevel.Notice)
            .WithOutcome(LogEventOutcome.Success)
            .WithJob(
                id: job.Id,
                name: job.Name,
                type: job.Type,
                sourcePath: ToUncOrEmpty(job.SourcePath),
                targetPath: ToUncOrEmpty(job.TargetPath),
                status: JobStatus.Idle,
                isActive: job.IsActive)
            .Build();

        _logService.Write(entry);
    }

    private void LogBusinessSoftwareBlocked(string? processName, BackupJob? job)
    {
        if (_logService is null)
            return;

        var entryBuilder = LogEntryBuilder.Create(
                eventName: "job.start.blocked.businesssoftware",
                category: LogEventCategory.Job,
                action: LogEventAction.Skip,
                message: $"Backup start blocked because business software '{processName}' is running")
            .WithLevel(LogLevel.Notice)
            .WithOutcome(LogEventOutcome.Failure);

        if (job != null)
        {
            entryBuilder = entryBuilder.WithJob(
                id: job.Id,
                name: job.Name,
                type: job.Type,
                sourcePath: ToUncOrEmpty(job.SourcePath),
                targetPath: ToUncOrEmpty(job.TargetPath),
                status: JobStatus.Idle,
                isActive: job.IsActive);
        }

        _logService.Write(entryBuilder.Build());
    }

    /// <summary>
    /// Normalizes a path to UNC for logging.
    /// </summary>
    private static string ToUncOrEmpty(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        return UncResolver.ResolveToUncForLog(path);
    }
}
