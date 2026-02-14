using System.Diagnostics;
using EasySave.App.Utils;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Events;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using EasySave.Core.Resources;
using EasySave.EasyLog.Factories;
using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Options;

namespace EasySave.App.Services;

/// <summary>
/// Executes backup jobs and produces state updates and logs.
/// </summary>
internal sealed class BackupEngine : IBackupEngine
{
    private readonly ILogger<LogEntryDto>? _logger;

    /// <summary>
    /// Raised when job state changes during execution. (update state.json)
    /// </summary>
    public event EventHandler<JobStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupEngine"/> class.
    /// </summary>
    /// <param name="logDirectory">Optional log directory; when null, logging is disabled.</param>
    /// <param name="logFormat">Log serialization format.</param>
    public BackupEngine(string? logDirectory = null, LogFormat logFormat = LogFormat.Json)
    {
        if (!string.IsNullOrWhiteSpace(logDirectory))
        {
            var options = new LogOptions
            {
                LogDirectory = logDirectory,
                Format = logFormat
            };

            _logger = LoggerFactory.Create<LogEntryDto>(options);
        }
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

        // Resultat cumule pour l'appelant (CLI/GUI/tests).
        var result = new BackupResultDto();
        // Chronometre la duree totale de la sauvegarde.
        var stopwatch = Stopwatch.StartNew();
        // Etat initial publie des le debut de l'execution.
        var state = CreateInitialState(job);

        if (!Directory.Exists(job.SourcePath))
        {
            // Dossier source manquant : on termine avec une erreur explicite.
            result.Success = false;
            result.Message = string.Format(Strings.Error_SourceFolderMissing, job.SourcePath);
            result.Errors.Add(result.Message);
            result.ErrorCount = result.Errors.Count;
            result.Duration = stopwatch.Elapsed;
            UpdateTerminalState(state, JobStatus.Error, result.Message);
            WriteSummaryLog(job, result);
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
            UpdateTerminalState(state, JobStatus.Error, result.Message);
            WriteSummaryLog(job, result);
            return result;
        }

        // Charge la liste des fichiers pour calculer les totaux avant execution.
        var files = Directory.EnumerateFiles(job.SourcePath, "*", SearchOption.AllDirectories).ToList();
        var totalSizeBytes = files.Sum(file => new FileInfo(file).Length);
        InitializeTotals(state, files.Count, totalSizeBytes);
        PublishState(state);

        // Execute la copie fichier par fichier.
        ExecuteBackup(job, files, job.SourcePath, job.TargetPath, strategy, result, state);

        // Le succes est determine par l'absence d'erreurs.
        result.Success = result.ErrorCount == 0;
        result.Message = result.Success ? Strings.Backup_Success : Strings.Info_BackupCompletedWithErrors;
        result.Duration = stopwatch.Elapsed;
        
        // Statut final dependant du succes global.
        var finalStatus = result.Success ? JobStatus.Completed : JobStatus.Error;
        
        // Concatene les erreurs pour l'etat final (utile pour la GUI).
        var finalError = result.Success ? null : string.Join(" | ", result.Errors);
        UpdateTerminalState(state, finalStatus, finalError);
        WriteSummaryLog(job, result);
        return result;
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
    private void ExecuteBackup(
        BackupJob job,
        IReadOnlyList<string> files,
        string sourceRoot,
        string targetRoot,
        IBackupCopyStrategy strategy,
        BackupResultDto result,
        JobStateDto state)
    {
        foreach (var sourcePath in files)
        {
            // Construit le chemin cible en preservant la structure relative.
            var relativePath = Path.GetRelativePath(sourceRoot, sourcePath);
            var targetPath = Path.Combine(targetRoot, relativePath);
            var fileSize = new FileInfo(sourcePath).Length;
            var transferStopwatch = new Stopwatch();
            
            // Publie l'etat du fichier courant sans incrementer les compteurs.
            UpdateProgressState(state, sourcePath, targetPath, fileSize, incrementProcessed: false);

            // Compte le fichier comme traite (meme s'il est saute ensuite).
            result.FilesProcessed++;

            try
            {
                if (!strategy.ShouldCopy(sourcePath, targetPath))
                {
                    // Cas "skip": on journalise et on avance.
                    result.SkippedCount++;
                    WriteLogEntry(job, sourcePath, targetPath, fileSize, 0, "SKIPPED");
                    UpdateProgressState(state, sourcePath, targetPath, fileSize, incrementProcessed: true);
                    continue;
                }

                // Cree le dossier cible si necessaire.
                EnsureTargetDirectory(job, sourcePath, targetPath);
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
                    "OK");
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
                    "ERROR",
                    ex.Message);
                // Stocke l'erreur detaillee pour le resume final.
                result.Errors.Add(
                    $"{sourceUnc} -> {targetUnc}: {ex.Message}; SizeBytes={fileSize}; TransferMs={transferMs:0.###}");
                result.ErrorCount++;
            }

            // Publie l'etat apres traitement du fichier.
            UpdateProgressState(state, sourcePath, targetPath, fileSize, incrementProcessed: true);
        }
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
    private void WriteSummaryLog(BackupJob job, BackupResultDto result)
    {
        if (_logger is null)
            return;

        // Resume lisible par l'utilisateur final.
        var summary = $"Copied={result.CopiedCount}; Skipped={result.SkippedCount}; Errors={result.ErrorCount}";
        if (result.Errors.Count > 0)
            summary = $"{summary}; Details={string.Join(" | ", result.Errors)}";

        WriteLogEntry(
            job,
            job.SourcePath,
            job.TargetPath,
            result.TotalBytesProcessed,
            result.Duration.TotalMilliseconds,
            result.ErrorCount == 0 ? "OK" : "ERROR",
            summary);
    }

    /// <summary>
    /// Ensures the target directory exists and logs directory creation.
    /// </summary>
    /// <param name="job">The job being executed.</param>
    /// <param name="sourcePath">The source file path.</param>
    /// <param name="targetPath">The target file path.</param>
    private void EnsureTargetDirectory(BackupJob job, string sourcePath, string targetPath)
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

        WriteLogEntry(job, sourceDirectory, directory, 0, 0, "DIR_CREATED");
    }

    /// <summary>
    /// Writes a single log entry for a file operation.
    /// </summary>
    /// <param name="job">The job being executed.</param>
    /// <param name="sourcePath">The source file path.</param>
    /// <param name="targetPath">The target file path.</param>
    /// <param name="fileSizeBytes">The file size in bytes.</param>
    /// <param name="transferTimeMs">The transfer time in milliseconds.</param>
    /// <param name="status">The status text.</param>
    /// <param name="errorMessage">Optional error message.</param>
    private void WriteLogEntry(
        BackupJob job,
        string? sourcePath,
        string? targetPath,
        long fileSizeBytes,
        double transferTimeMs,
        string status,
        string? errorMessage = null)
    {
        if (_logger is null)
            return;

        var entry = new LogEntryDto
        {
            TimestampUtc = DateTime.UtcNow,
            JobName = job.Name,
            // Conversion en UNC pour une lecture coherente sur le reseau.
            SourcePath = string.IsNullOrWhiteSpace(sourcePath)
                ? string.Empty
                : UncResolver.ResolveToUncForLog(sourcePath),
            TargetPath = string.IsNullOrWhiteSpace(targetPath)
                ? string.Empty
                : UncResolver.ResolveToUncForLog(targetPath),
            FileSizeBytes = fileSizeBytes,
            TransferTimeMs = transferTimeMs,
            Status = status,
            ErrorMessage = errorMessage
        };

        _logger.Write(entry);
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
    private void InitializeTotals(JobStateDto state, int totalFiles, long totalSizeBytes)
    {
        state.TotalFiles = totalFiles;
        state.TotalSizeBytes = totalSizeBytes;
        state.RemainingFiles = totalFiles;
        state.RemainingSizeBytes = totalSizeBytes;
        state.ProgressPercentage = 0;
        state.LastActionTimestampUtc = DateTime.UtcNow;
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
        JobStateDto state,
        string sourcePath,
        string targetPath,
        long fileSize,
        bool incrementProcessed)
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

    /// <summary>
    /// Updates state to a terminal status and clears current file info.
    /// </summary>
    /// <param name="state">The state snapshot to update.</param>
    /// <param name="status">The terminal status.</param>
    /// <param name="errorMessage">Optional error message.</param>
    private void UpdateTerminalState(JobStateDto state, JobStatus status, string? errorMessage)
    {
        state.Status = status;
        state.CurrentSourceFile = null;
        state.CurrentTargetFile = null;
        state.ErrorMessage = errorMessage;
        state.ProgressPercentage = status == JobStatus.Completed ? 100 : state.ProgressPercentage;
        state.LastActionTimestampUtc = DateTime.UtcNow;
        PublishState(state);
    }

    /// <summary>
    /// Raises the <see cref="StateChanged"/> event.
    /// </summary>
    /// <param name="state">The state snapshot to publish.</param>
    private void PublishState(JobStateDto state)
    {
        StateChanged?.Invoke(this, new JobStateChangedEventArgs(state));
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
}
