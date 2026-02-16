using System.Collections.Concurrent;
using System.Diagnostics;
using EasySave.App.Utils;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Events;
using EasySave.Core.Interfaces;
using EasySave.Core.Logging;
using EasySave.Core.Models;
using EasySave.Core.Resources;

namespace EasySave.App.Services;

/// <summary>
/// Executes backup jobs and produces state updates and logs.
/// </summary>
internal sealed class BackupEngine : IBackupEngine
{
    private readonly IAppLogService? _logService;
    private readonly AppConfig _config;
    private readonly ICryptoService _cryptoService;
    private readonly ConcurrentDictionary<string, JobExecutionControl> _jobControls = new(StringComparer.Ordinal);

    /// <summary>
    /// Raised when job state changes during execution. (update state.json)
    /// </summary>
    public event EventHandler<JobStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupEngine"/> class.
    /// </summary>
    /// <param name="logDirectory">Optional log directory; when null, logging is disabled.</param>
    /// <param name="logFormat">Log serialization format.</param>
    /// <param name="logService">Optional log service.</param>
    public BackupEngine(AppConfig config, IAppLogService? logService = null, ICryptoService? cryptoService = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logService = logService;
        _cryptoService = cryptoService ?? CreateDefaultCryptoService();
    }

    /// <summary>
    /// Executes a backup job and returns the result.
    /// </summary>
    /// <param name="job">The job to execute.</param>
    /// <returns>The execution result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="job"/> is null.</exception>
    public BackupResultDto Run(BackupJob job)
    {
        if (job is null)
            throw new ArgumentNullException(nameof(job));
            
        var traceId = Guid.NewGuid().ToString("N");
        // Resultat cumule pour l'appelant (CLI/GUI/tests).
        var result = new BackupResultDto();
        // Etat initial publie des le debut de l'execution.
        var state = CreateInitialState(job);
        Stopwatch? stopwatch = null;
        var control = new JobExecutionControl(state);
        _jobControls[job.Id] = control;

        try
        {
            try
            {
                // Check that the configured business software is not currently running before starting the backup.
                BusinessSoftwareDetector.ValidateNotRunning(_config.BusinessSoftwareProcessName);
            }
            catch (InvalidOperationException ex)
            {
                // if the business software is running, create an explicit failure result for the job.
                result.Success = false;
                result.Message = ex.Message;
                result.Duration = TimeSpan.Zero;
                result.Errors.Add(ex.Message);
                result.ErrorCount = result.Errors.Count;
                if (_logService != null)
                {
                    var blockedEntry = LogEntryBuilder.Create(
                            eventName: "job.start.blocked.businesssoftware",
                            category: LogEventCategory.Job,
                            action: LogEventAction.Skip,
                            message: ex.Message)
                        .WithLevel(LogLevel.Warning)
                        .WithOutcome(LogEventOutcome.Failure)
                        .WithJob(
                            id: job.Id,
                            name: job.Name,
                            type: job.Type,
                            sourcePath: ToUncOrEmpty(job.SourcePath),
                            targetPath: ToUncOrEmpty(job.TargetPath),
                            status: JobStatus.Idle,
                            isActive: job.IsActive)
                        .Build();
                    _logService.Write(blockedEntry);
                }
                WriteSummaryLog(job, result, traceId);
                UpdateTerminalState(control, state, JobStatus.Error, ex.Message);
                return result;
            }

            // Chronometre la duree totale de la sauvegarde.
            stopwatch = Stopwatch.StartNew();

            if (!Directory.Exists(job.SourcePath))
            {
                // Dossier source manquant: on termine avec une erreur explicite.
                result.Success = false;
                result.Message = string.Format(Strings.Error_SourceFolderMissing, job.SourcePath);
                result.Errors.Add(result.Message);
                result.ErrorCount = result.Errors.Count;
                result.Duration = stopwatch.Elapsed;
                WriteSummaryLog(job, result, traceId);
                UpdateTerminalState(control, state, JobStatus.Error, result.Message);
                return result;
            }

            // Choisit la strategie de copie selon le type de sauvegarde.
            IBackupCopyStrategy? strategy = job.Type switch
            {
                BackupType.Differential => new DifferentialCopyStrategy(),
                BackupType.Full => new FullCopyStrategy(),
                _ => null
            };

            if (strategy is null)
            {
                // Type de sauvegarde inconnu: on ne sait pas copier correctement.
                result.Success = false;
                result.Message = string.Format(Strings.Error_BackupTypeNotSupported, job.Type);
                result.Errors.Add(result.Message);
                result.ErrorCount = result.Errors.Count;
                result.Duration = stopwatch.Elapsed;
                WriteSummaryLog(job, result, traceId);
                UpdateTerminalState(control, state, JobStatus.Error, result.Message);
                return result;
            }

            // Charge la liste des fichiers pour calculer les totaux avant execution.
            var files = Directory.EnumerateFiles(job.SourcePath, "*", SearchOption.AllDirectories).ToList();
            var totalSizeBytes = files.Sum(file => new FileInfo(file).Length);
            InitializeTotals(control, state, files.Count, totalSizeBytes);
            PublishState(state);

            // Execute la copie fichier par fichier.
            var cancelled = ExecuteBackup(control, job, files, job.SourcePath, job.TargetPath, strategy, result, state, traceId);
            if (cancelled)
            {
                result.Success = false;
                result.Message = Strings.Error_BackupStoppedByUser;
                result.Errors.Add(result.Message);
                result.ErrorCount = result.Errors.Count;
                result.Duration = stopwatch.Elapsed;
                WriteSummaryLog(job, result, traceId);
                UpdateTerminalState(control, state, JobStatus.Error, result.Message);
                return result;
            }

            // Le succes est determine par l'absence d'erreurs.
            result.Success = result.ErrorCount == 0;
            result.Message = result.Success ? Strings.Backup_Success : Strings.Info_BackupCompletedWithErrors;
            result.Duration = stopwatch?.Elapsed ?? TimeSpan.Zero;
            
            // Statut final dependant du succes global.
            var finalStatus = result.Success ? JobStatus.Completed : JobStatus.Error;
            
            // Concatene les erreurs pour l'etat final (utile pour la GUI).
            var finalError = result.Success ? null : string.Join(" | ", result.Errors);
            WriteSummaryLog(job, result, traceId);
            UpdateTerminalState(control, state, finalStatus, finalError);
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = string.Format(Strings.Error_Generic, ex.Message);
            result.Errors.Add(result.Message);
            result.ErrorCount = result.Errors.Count;
            result.Duration = stopwatch?.Elapsed ?? TimeSpan.Zero;

            WriteSummaryLog(job, result, traceId);
            UpdateTerminalState(control, state, JobStatus.Error, result.Message);
            return result;
        }
        finally
        {
            _jobControls.TryRemove(job.Id, out var removed);
            removed?.Dispose();
        }
    }

    /// <summary>
    /// Requests a pause for a running job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns><c>true</c> when the pause was requested.</returns>
    public bool Pause(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return false;

        if (!_jobControls.TryGetValue(jobId, out var control))
            return false;

        return TrySetPaused(control);
    }

    /// <summary>
    /// Requests a resume for a paused job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns><c>true</c> when the resume was requested.</returns>
    public bool Resume(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return false;

        if (!_jobControls.TryGetValue(jobId, out var control))
            return false;

        control.RequestResume();
        return TrySetRunning(control);
    }

    /// <summary>
    /// Requests a stop for a running job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns><c>true</c> when the stop was requested.</returns>
    public bool Stop(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return false;

        if (!_jobControls.TryGetValue(jobId, out var control))
            return false;

        control.RequestStop();
        return TrySetStopped(control);
    }

    /// <summary>
    /// Runs the copy loop for all files and updates state and results.
    /// </summary>
    /// <param name="job">The job being executed.</param>
    /// <param name="files">The list of files to process.</param>
    /// <param name="sourceRoot">The source root path.</param>
    /// <param name="targetRoot">The target root path.</param>
    /// <param name="strategy">The copy strategy to use.</param>
    /// <param name="result">The mutable result collector.</param>
    /// <param name="state">The mutable state snapshot.</param>
    /// <param name="traceId">Trace identifier for the execution.</param>
    private bool ExecuteBackup(
        JobExecutionControl control,
        BackupJob job,
        IReadOnlyList<string> files,
        string sourceRoot,
        string targetRoot,
        IBackupCopyStrategy strategy,
        BackupResultDto result,
        JobStateDto state,
        string traceId)
    {
        foreach (var sourcePath in files)
        {
            if (control.IsStopRequested)
                return true;

            WaitIfPaused(control, state);
            if (control.IsStopRequested)
                return true;
            // Construit le chemin cible en preservant la structure relative.
            var relativePath = Path.GetRelativePath(sourceRoot, sourcePath);
            var targetPath = Path.Combine(targetRoot, relativePath);
            var fileSize = new FileInfo(sourcePath).Length;
            var transferStopwatch = new Stopwatch();

            // Publie l'etat du fichier courant sans incrementer les compteurs.
            UpdateProgressState(control, state, sourcePath, targetPath, fileSize, incrementProcessed: false);

            // Compte le fichier comme traite (meme s'il est saute ensuite).
            result.FilesProcessed++;

            try
            {
                if (!strategy.ShouldCopy(sourcePath, targetPath))
                {
                    // Cas "skip": on journalise et on avance.
                    result.SkippedCount++;
                    WriteLogEntry(
                        job,
                        sourcePath,
                        targetPath,
                        fileSize,
                        0,
                        LogEventAction.Skip,
                        LogEventOutcome.Success,
                        traceId);
                    UpdateProgressState(control, state, sourcePath, targetPath, fileSize, incrementProcessed: true);
                    continue;
                }

                // Cree le dossier cible si necessaire.
                EnsureTargetDirectory(job, sourcePath, targetPath, traceId);
                transferStopwatch.Start();
                CopyFile(sourcePath, targetPath);
                transferStopwatch.Stop();
                result.CopiedCount++;
                result.TotalBytesProcessed += fileSize;
                WriteLogEntry(
                    job,
                    sourcePath,
                    targetPath,
                    fileSize,
                    transferStopwatch.Elapsed.TotalMilliseconds,
                    LogEventAction.Transfer,
                    LogEventOutcome.Success,
                    traceId);

                if (ShouldEncrypt(sourcePath))
                {
                    var cryptoMissing = !File.Exists(GetDefaultCryptoSoftPath());
                    if (cryptoMissing && _logService != null)
                    {
                        var logEntry = LogEntryBuilder.Create(
                                eventName: "crypto.fallback",
                                category: LogEventCategory.Settings,
                                action: LogEventAction.Unknown,
                                message: "CryptoSoft.exe not found. Encryption skipped.")
                            .WithLevel(LogLevel.Warning)
                            .WithOutcome(LogEventOutcome.Failure)
                            .Build();
                        _logService.Write(logEntry);
                    }

                    int encryptionTimeMs;
                    bool encryptionFailed = false;
                    string? encryptionError = null;
                    try
                    {
                        encryptionTimeMs = _cryptoService
                            .EncryptFileAsync(sourcePath, _config.EncryptionKey)
                            .GetAwaiter()
                            .GetResult();
                        if (encryptionTimeMs < 0)
                        {
                            encryptionFailed = true;
                            encryptionError = $"Encryption failed for {Path.GetFileName(sourcePath)} (code {encryptionTimeMs}).";
                        }
                    }
                    catch (Exception ex)
                    {
                        encryptionTimeMs = -1;
                        encryptionFailed = true;
                        encryptionError = $"Encryption failed for {Path.GetFileName(sourcePath)}: {ex.Message}";
                    }

                    if (encryptionFailed && !string.IsNullOrWhiteSpace(encryptionError))
                    {
                        result.Errors.Add(encryptionError);
                        result.ErrorCount = result.Errors.Count;
                    }

                    if (_logService != null)
                    {
                        var level = encryptionTimeMs < 0 ? LogLevel.Error : LogLevel.Info;
                        var outcome = encryptionTimeMs < 0 ? LogEventOutcome.Failure : LogEventOutcome.Success;
                        var message = encryptionTimeMs < 0
                            ? $"Encryption failed for {Path.GetFileName(sourcePath)}"
                            : $"File {Path.GetFileName(sourcePath)} encrypted";

                        var cryptoDto = new LogCryptoDto
                        {
                            Tool = "CryptoSoft",
                            ExtensionMatched = true,
                            EncryptionTimeMs = encryptionTimeMs,
                            Extension = Path.GetExtension(sourcePath),
                            InstanceLock = null
                        };
                        var logEntry = LogEntryBuilder.Create(
                              eventName: "file.encrypted",
                              category: LogEventCategory.File,
                              action: LogEventAction.Unknown,
                              message: message)
                          .WithLevel(level)
                          .WithOutcome(outcome)
                          .WithFile(
                              sourcePath: sourcePath,
                              targetPath: targetPath,
                              sizeBytes: fileSize,
                            transferTimeMs: encryptionTimeMs)
                        .WithCrypto(
                            tool: cryptoDto.Tool,
                            extensionMatched: cryptoDto.ExtensionMatched,
                            encryptionTimeMs: cryptoDto.EncryptionTimeMs,
                            extension: cryptoDto.Extension)
                        .Build();

                        _logService.Write(logEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                if (transferStopwatch.IsRunning)
                    transferStopwatch.Stop();

                // Convertit en UNC pour des logs coherents.
                var sourceUnc = UncResolver.ResolveToUncForLog(sourcePath);
                var targetUnc = UncResolver.ResolveToUncForLog(targetPath);

                // Temps de transfert negatif pour signaler un echec dans les logs.
                var transferMs = -transferStopwatch.Elapsed.TotalMilliseconds;
                if (transferMs >= 0)
                    transferMs = -1;

                WriteLogEntry(
                    job,
                    sourcePath,
                    targetPath,
                    fileSize,
                    transferMs,
                    LogEventAction.Transfer,
                    LogEventOutcome.Failure,
                    traceId,
                    ex.Message);
                // Stocke l'erreur detaillee pour le resume final.
                result.Errors.Add(
                    $"{sourceUnc} -> {targetUnc}: {ex.Message}; SizeBytes={fileSize}; TransferMs={transferMs:0.###}");
                result.ErrorCount++;
            }

            // Publie l'etat apres traitement du fichier.
            UpdateProgressState(control, state, sourcePath, targetPath, fileSize, incrementProcessed: true);

            if (!string.IsNullOrWhiteSpace(_config.BusinessSoftwareProcessName)
                && BusinessSoftwareDetector.IsRunning(_config.BusinessSoftwareProcessName))
            {
                if (_logService != null)
                {
                    var logEntry = LogEntryBuilder.Create(
                            eventName: "job.stopped.businesssoftware",
                            category: LogEventCategory.Job,
                            action: LogEventAction.Summary,
                            message: "Backup stopped because business software detected")
                        .WithLevel(LogLevel.Warning)
                        .WithOutcome(LogEventOutcome.Failure)
                        .WithJob(
                            id: job.Id,
                            name: job.Name,
                            type: job.Type,
                            sourcePath: ToUncOrEmpty(job.SourcePath),
                            targetPath: ToUncOrEmpty(job.TargetPath),
                            status: JobStatus.Error)
                        .WithOutcome(LogEventOutcome.Failure)
                        .Build();
                    _logService.Write(logEntry);
                }

                result.Errors.Add($"Backup stopped because {_config.BusinessSoftwareProcessName} detected");
                result.ErrorCount++;
                break;
            }
        }

        return control.IsStopRequested;
    }

    /// <summary>
    /// Copies a file to a target path.
    /// </summary>
    /// <param name="sourcePath">Source file path.</param>
    /// <param name="targetPath">Target file path.</param>
    private static void CopyFile(string sourcePath, string targetPath)
        => File.Copy(sourcePath, targetPath, true);

    /// <summary>
    /// Writes a summary log entry for the job.
    /// </summary>
    /// <param name="job">The job that completed.</param>
    /// <param name="result">The execution result.</param>
    private void WriteSummaryLog(BackupJob job, BackupResultDto result, string? traceId)
    {
        if (_logService is null)
            return;

        // Resume lisible par l'utilisateur final.
        var summary = $"Copied={result.CopiedCount}; Skipped={result.SkippedCount}; Errors={result.ErrorCount}";
        var details = result.Errors.Count > 0 ? string.Join(" | ", result.Errors) : null;
        if (!string.IsNullOrWhiteSpace(details))
            summary = $"{summary}; Details={details}";

        var outcome = result.ErrorCount == 0 ? LogEventOutcome.Success : LogEventOutcome.Failure;
        var level = result.ErrorCount == 0 ? LogLevel.Info : LogLevel.Error;

        var builder = LogEntryBuilder.Create(
                eventName: "job.summary",
                category: LogEventCategory.Job,
                action: LogEventAction.Summary,
                message: summary)
            .WithLevel(level)
            .WithTraceIfPresent(traceId)
            .WithJob(
                id: job.Id,
                name: job.Name,
                type: job.Type,
                sourcePath: ToUncOrEmpty(job.SourcePath),
                targetPath: ToUncOrEmpty(job.TargetPath),
                status: result.ErrorCount == 0 ? JobStatus.Completed : JobStatus.Error)
            .WithSummary(
                copiedCount: result.CopiedCount,
                skippedCount: result.SkippedCount,
                errorCount: result.ErrorCount,
                totalBytes: result.TotalBytesProcessed,
                durationMs: result.Duration.TotalMilliseconds,
                details: details);

        if (outcome == LogEventOutcome.Failure && !string.IsNullOrWhiteSpace(details))
        {
            builder.Fail("JobSummary", details);
        }
        else
        {
            builder.WithOutcome(outcome);
        }

        _logService.Write(builder.Build());
    }

    /// <summary>
    /// Ensures the target directory exists and logs directory creation.
    /// </summary>
    /// <param name="job">The job being executed.</param>
    /// <param name="sourcePath">The source file path.</param>
    /// <param name="targetPath">The target file path.</param>
    /// <param name="traceId">Trace identifier for the execution.</param>
    private void EnsureTargetDirectory(BackupJob job, string sourcePath, string targetPath, string traceId)
    {
        var directory = Path.GetDirectoryName(targetPath);
        if (string.IsNullOrWhiteSpace(directory))
            return;

        if (Directory.Exists(directory))
            return;

        // Cree le dossier cible et log l'operation.
        Directory.CreateDirectory(directory);

        var sourceDirectory = Path.GetDirectoryName(sourcePath);
        if (string.IsNullOrWhiteSpace(sourceDirectory))
            sourceDirectory = job.SourcePath;

        WriteLogEntry(
            job,
            sourceDirectory,
            directory,
            0,
            0,
            LogEventAction.DirectoryCreated,
            LogEventOutcome.Success,
            traceId,
            isDirectory: true);
    }

    /// <summary>
    /// Writes a single log entry for a file operation.
    /// </summary>
    /// <param name="job">The job being executed.</param>
    /// <param name="sourcePath">The source file path.</param>
    /// <param name="targetPath">The target file path.</param>
    /// <param name="fileSizeBytes">The file size in bytes.</param>
    /// <param name="transferTimeMs">The transfer time in milliseconds.</param>
    /// <param name="action">The log action.</param>
    /// <param name="outcome">The log outcome.</param>
    /// <param name="traceId">Trace identifier for the execution.</param>
    /// <param name="errorMessage">Optional error message.</param>
    /// <param name="isDirectory">Whether the entry describes a directory.</param>
    private void WriteLogEntry(
        BackupJob job,
        string? sourcePath,
        string? targetPath,
        long fileSizeBytes,
        double transferTimeMs,
        LogEventAction action,
        LogEventOutcome outcome,
        string traceId,
        string? errorMessage = null,
        bool isDirectory = false)
    {
        if (_logService is null)
            return;

        var sourceUnc = string.IsNullOrWhiteSpace(sourcePath)
            ? string.Empty
            : UncResolver.ResolveToUncForLog(sourcePath);
        var targetUnc = string.IsNullOrWhiteSpace(targetPath)
            ? string.Empty
            : UncResolver.ResolveToUncForLog(targetPath);

        var (eventName, message, level) = action switch
        {
            LogEventAction.Skip => ("file.skipped", "File skipped", LogLevel.Notice),
            LogEventAction.DirectoryCreated => ("directory.created", "Directory created", LogLevel.Info),
            LogEventAction.Transfer when outcome == LogEventOutcome.Failure
                => ("file.transfer.failed", "File transfer failed", LogLevel.Error),
            LogEventAction.Transfer => ("file.transferred", "File transferred", LogLevel.Info),
            _ => ("file.event", "File event", LogLevel.Info)
        };

        var builder = LogEntryBuilder.Create(
                eventName: eventName,
                category: LogEventCategory.File,
                action: action,
                message: message)
            .WithLevel(level)
            .WithTraceIfPresent(traceId)
            .WithJob(
                id: job.Id,
                name: job.Name,
                type: job.Type,
                sourcePath: ToUncOrEmpty(job.SourcePath),
                targetPath: ToUncOrEmpty(job.TargetPath),
                status: JobStatus.Running)
            .WithFile(
                sourcePath: sourceUnc,
                targetPath: targetUnc,
                sizeBytes: fileSizeBytes,
                transferTimeMs: transferTimeMs,
                isDirectory: isDirectory);

        if (outcome == LogEventOutcome.Failure && !string.IsNullOrWhiteSpace(errorMessage))
        {
            builder.Fail("FileTransfer", errorMessage);
        }
        else
        {
            builder.WithOutcome(outcome);
        }

        _logService.Write(builder.Build());
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

    /// <summary>
    /// Creates the initial running state for a job.
    /// </summary>
    /// <param name="job">The job being executed.</param>
    /// <returns>A new state snapshot.</returns>
    private JobStateDto CreateInitialState(BackupJob job)
    {
        return new JobStateDto
        {
            JobId = job.Id,
            JobName = job.Name,
            Status = JobStatus.Running,
            LastActionTimestampUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Initializes total file and size counters.
    /// </summary>
    /// <param name="state">The state snapshot to update.</param>
    /// <param name="totalFiles">Total number of files.</param>
    /// <param name="totalSizeBytes">Total size in bytes.</param>
    private void InitializeTotals(JobExecutionControl control, JobStateDto state, int totalFiles, long totalSizeBytes)
    {
        lock (control.Sync)
        {
            state.TotalFiles = totalFiles;
            state.TotalSizeBytes = totalSizeBytes;
            state.RemainingFiles = totalFiles;
            state.RemainingSizeBytes = totalSizeBytes;
            state.ProgressPercentage = 0;
            state.LastActionTimestampUtc = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Updates the progress-related fields for the current file.
    /// </summary>
    /// <param name="state">The state snapshot to update.</param>
    /// <param name="sourcePath">Current source file path.</param>
    /// <param name="targetPath">Current target file path.</param>
    /// <param name="fileSize">Current file size in bytes.</param>
    /// <param name="incrementProcessed">Whether to increment processed counters.</param>
    private void UpdateProgressState(
        JobExecutionControl control,
        JobStateDto state,
        string sourcePath,
        string targetPath,
        long fileSize,
        bool incrementProcessed)
    {
        lock (control.Sync)
        {
            state.CurrentSourceFile = UncResolver.ResolveToUncForLog(sourcePath);
            state.CurrentTargetFile = UncResolver.ResolveToUncForLog(targetPath);

            if (incrementProcessed)
            {
                state.FilesProcessed++;
                state.SizeProcessedBytes += fileSize;
            }

            // Recalcule le reste et le pourcentage apres chaque fichier.
            state.RemainingFiles = Math.Max(0, state.TotalFiles - state.FilesProcessed);
            state.RemainingSizeBytes = Math.Max(0, state.TotalSizeBytes - state.SizeProcessedBytes);
            state.ProgressPercentage = CalculateProgress(state);
            state.LastActionTimestampUtc = DateTime.UtcNow;
            PublishState(state);
        }
    }

    /// <summary>
    /// Updates state to a terminal status and clears current file info.
    /// </summary>
    /// <param name="state">The state snapshot to update.</param>
    /// <param name="status">The terminal status.</param>
    /// <param name="errorMessage">Optional error message.</param>
    private void UpdateTerminalState(JobExecutionControl control, JobStateDto state, JobStatus status, string? errorMessage)
    {
        lock (control.Sync)
        {
            state.Status = status;
            state.CurrentSourceFile = null;
            state.CurrentTargetFile = null;
            state.ErrorMessage = errorMessage;
            state.ProgressPercentage = status == JobStatus.Completed ? 100 : state.ProgressPercentage;
            state.LastActionTimestampUtc = DateTime.UtcNow;
            PublishState(state);
        }
    }

    /// <summary>
    /// Raises the <see cref="StateChanged"/> event.
    /// </summary>
    /// <param name="state">The state snapshot to publish.</param>
    private void PublishState(JobStateDto state)
    {
        StateChanged?.Invoke(this, new JobStateChangedEventArgs(state));
    }

    private void WaitIfPaused(JobExecutionControl control, JobStateDto state)
    {
        if (!control.IsPaused)
            return;

        lock (control.Sync)
        {
            if (state.Status != JobStatus.Paused)
            {
                state.Status = JobStatus.Paused;
                state.LastActionTimestampUtc = DateTime.UtcNow;
                PublishState(state);
            }
        }

        try
        {
            control.WaitWhilePaused();
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (control.IsStopRequested)
            return;

        lock (control.Sync)
        {
            if (state.Status == JobStatus.Paused)
            {
                state.Status = JobStatus.Running;
                state.LastActionTimestampUtc = DateTime.UtcNow;
                PublishState(state);
            }
        }
    }

    private bool TrySetPaused(JobExecutionControl control)
    {
        lock (control.Sync)
        {
            if (control.State.Status is JobStatus.Completed or JobStatus.Error)
                return false;
            if (control.State.Status == JobStatus.Paused)
            {
                control.RequestPause();
                return true;
            }

            control.RequestPause();
            control.State.Status = JobStatus.Paused;
            control.State.LastActionTimestampUtc = DateTime.UtcNow;
            PublishState(control.State);
            return true;
        }
    }

    private bool TrySetRunning(JobExecutionControl control)
    {
        lock (control.Sync)
        {
            if (control.State.Status is JobStatus.Completed or JobStatus.Error)
                return false;
            if (control.State.Status != JobStatus.Paused)
                return true;

            control.State.Status = JobStatus.Running;
            control.State.LastActionTimestampUtc = DateTime.UtcNow;
            PublishState(control.State);
            return true;
        }
    }

    private bool TrySetStopped(JobExecutionControl control)
    {
        lock (control.Sync)
        {
            if (control.State.Status is not (JobStatus.Running or JobStatus.Paused))
                return false;

            control.State.Status = JobStatus.Error;
            control.State.ErrorMessage = Strings.Error_BackupStoppedByUser;
            control.State.LastActionTimestampUtc = DateTime.UtcNow;
            PublishState(control.State);
            return true;
        }
    }

    /// <summary>
    /// Calculates progress percentage based on size or file counts.
    /// </summary>
    /// <param name="state">The current job state.</param>
    /// <returns>Progress percentage from 0 to 100.</returns>
    private static int CalculateProgress(JobStateDto state)
    {
        if (state.TotalSizeBytes > 0)
        {
            // Priorite a la taille si elle est disponible.
            var percent = (int)(state.SizeProcessedBytes * 100 / state.TotalSizeBytes);
            return Math.Clamp(percent, 0, 100);
        }

        if (state.TotalFiles > 0)
        {
            // Sinon, fallback sur le nombre de fichiers.
            var percent = (int)(state.FilesProcessed * 100 / state.TotalFiles);
            return Math.Clamp(percent, 0, 100);
        }

        return 0;
    }

    private bool ShouldEncrypt(string sourcePath)
    {
        if (!_config.EncryptionEnabled)
            return false;
        if (string.IsNullOrWhiteSpace(_config.EncryptionKey))
            return false;

        var extension = Path.GetExtension(sourcePath);
        if (string.IsNullOrWhiteSpace(extension))
            return false;

        if (_config.ExtensionsToEncrypt.Count == 0)
            return false;

        return _config.ExtensionsToEncrypt
            .Any(ext => string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetDefaultCryptoSoftPath()
        => Path.Combine(AppContext.BaseDirectory, "CryptoSoft.exe");

    private static ICryptoService CreateDefaultCryptoService()
    {
        var path = GetDefaultCryptoSoftPath();
        if (File.Exists(path))
            return new CryptoSoftProcessService(path);
        return new NoEncryptionService();
    }
}
