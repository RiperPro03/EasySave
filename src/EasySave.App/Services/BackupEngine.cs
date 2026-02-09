using System.Diagnostics;
using System.Linq;
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

internal sealed class BackupEngine : IBackupEngine
{
    private readonly ILogger<LogEntryDto>? _logger;

    public event EventHandler<JobStateChangedEventArgs>? StateChanged;

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

    public BackupResultDto Run(BackupJob job)
    {
        if (job is null)
            throw new ArgumentNullException(nameof(job));

        var result = new BackupResultDto();
        var stopwatch = Stopwatch.StartNew();
        var state = CreateInitialState(job);

        if (!Directory.Exists(job.SourcePath))
        {
            result.Success = false;
            result.Message = string.Format(Strings.Error_SourceFolderMissing, job.SourcePath);
            result.Errors.Add(result.Message);
            result.ErrorCount = result.Errors.Count;
            result.Duration = stopwatch.Elapsed;
            UpdateTerminalState(state, JobStatus.Error, result.Message);
            WriteSummaryLog(job, result);
            return result;
        }

        IBackupCopyStrategy? strategy = job.Type switch
        {
            BackupType.Differential => new DifferentialCopyStrategy(),
            BackupType.Full => new FullCopyStrategy(),
            _ => null
        };

        if (strategy is null)
        {
            result.Success = false;
            result.Message = string.Format(Strings.Error_BackupTypeNotSupported, job.Type);
            result.Errors.Add(result.Message);
            result.ErrorCount = result.Errors.Count;
            result.Duration = stopwatch.Elapsed;
            UpdateTerminalState(state, JobStatus.Error, result.Message);
            WriteSummaryLog(job, result);
            return result;
        }

        var files = Directory.EnumerateFiles(job.SourcePath, "*", SearchOption.AllDirectories).ToList();
        var totalSizeBytes = files.Sum(file => new FileInfo(file).Length);
        InitializeTotals(state, files.Count, totalSizeBytes);
        PublishState(state);

        ExecuteBackup(job, files, job.SourcePath, job.TargetPath, strategy, result, state);

        result.Success = result.ErrorCount == 0;
        result.Message = result.Success ? Strings.Backup_Success : Strings.Info_BackupCompletedWithErrors;
        result.Duration = stopwatch.Elapsed;
        var finalStatus = result.Success ? JobStatus.Completed : JobStatus.Error;
        var finalError = result.Success ? null : string.Join(" | ", result.Errors);
        UpdateTerminalState(state, finalStatus, finalError);
        WriteSummaryLog(job, result);
        return result;
    }

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
            var relativePath = Path.GetRelativePath(sourceRoot, sourcePath);
            var targetPath = Path.Combine(targetRoot, relativePath);
            var fileSize = new FileInfo(sourcePath).Length;
            var transferStopwatch = new Stopwatch();
            UpdateProgressState(state, sourcePath, targetPath, fileSize, incrementProcessed: false);

            result.FilesProcessed++;

            try
            {
                if (!strategy.ShouldCopy(sourcePath, targetPath))
                {
                    result.SkippedCount++;
                    WriteLogEntry(job, sourcePath, targetPath, fileSize, 0, "SKIPPED");
                    UpdateProgressState(state, sourcePath, targetPath, fileSize, incrementProcessed: true);
                    continue;
                }

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

                var sourceUnc = UncResolver.ResolveToUncForLog(sourcePath);
                var targetUnc = UncResolver.ResolveToUncForLog(targetPath);
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
                result.Errors.Add(
                    $"{sourceUnc} -> {targetUnc}: {ex.Message}; SizeBytes={fileSize}; TransferMs={transferMs:0.###}");
                result.ErrorCount++;
            }

            UpdateProgressState(state, sourcePath, targetPath, fileSize, incrementProcessed: true);
        }
    }

    private static void CopyFile(string sourcePath, string targetPath)
        => File.Copy(sourcePath, targetPath, true);

    private void WriteSummaryLog(BackupJob job, BackupResultDto result)
    {
        if (_logger is null)
            return;

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

    private void EnsureTargetDirectory(BackupJob job, string sourcePath, string targetPath)
    {
        var directory = Path.GetDirectoryName(targetPath);
        if (string.IsNullOrWhiteSpace(directory))
            return;

        if (Directory.Exists(directory))
            return;

        Directory.CreateDirectory(directory);

        var sourceDirectory = Path.GetDirectoryName(sourcePath);
        if (string.IsNullOrWhiteSpace(sourceDirectory))
            sourceDirectory = job.SourcePath;

        WriteLogEntry(job, sourceDirectory, directory, 0, 0, "DIR_CREATED");
    }

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

    private void InitializeTotals(JobStateDto state, int totalFiles, long totalSizeBytes)
    {
        state.TotalFiles = totalFiles;
        state.TotalSizeBytes = totalSizeBytes;
        state.RemainingFiles = totalFiles;
        state.RemainingSizeBytes = totalSizeBytes;
        state.ProgressPercentage = 0;
        state.LastActionTimestampUtc = DateTime.UtcNow;
    }

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

        state.RemainingFiles = Math.Max(0, state.TotalFiles - state.FilesProcessed);
        state.RemainingSizeBytes = Math.Max(0, state.TotalSizeBytes - state.SizeProcessedBytes);
        state.ProgressPercentage = CalculateProgress(state);
        state.LastActionTimestampUtc = DateTime.UtcNow;
        PublishState(state);
    }

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

    private void PublishState(JobStateDto state)
    {
        StateChanged?.Invoke(this, new JobStateChangedEventArgs(state));
    }

    private static int CalculateProgress(JobStateDto state)
    {
        if (state.TotalSizeBytes > 0)
        {
            var percent = (int)(state.SizeProcessedBytes * 100 / state.TotalSizeBytes);
            return Math.Clamp(percent, 0, 100);
        }

        if (state.TotalFiles > 0)
        {
            var percent = (int)(state.FilesProcessed * 100 / state.TotalFiles);
            return Math.Clamp(percent, 0, 100);
        }

        return 0;
    }
}
