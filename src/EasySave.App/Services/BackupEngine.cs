using System.Diagnostics;
using System.Linq;
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

        ExecuteBackup(files, job.SourcePath, job.TargetPath, strategy, result, state);

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
            UpdateProgressState(state, sourcePath, targetPath, fileSize, incrementProcessed: false);

            result.FilesProcessed++;

            try
            {
                if (!strategy.ShouldCopy(sourcePath, targetPath))
                {
                    result.SkippedCount++;
                    continue;
                }

                CopyFile(sourcePath, targetPath);
                result.CopiedCount++;
                result.TotalBytesProcessed += fileSize;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{sourcePath} -> {targetPath}: {ex.Message}");
                result.ErrorCount++;
            }

            UpdateProgressState(state, sourcePath, targetPath, fileSize, incrementProcessed: true);
        }
    }

    private static void CopyFile(string sourcePath, string targetPath)
    {
        var directory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.Copy(sourcePath, targetPath, true);
    }

    private void WriteSummaryLog(BackupJob job, BackupResultDto result)
    {
        if (_logger is null)
            return;

        var summary = $"Copied={result.CopiedCount}; Skipped={result.SkippedCount}; Errors={result.ErrorCount}";
        if (result.Errors.Count > 0)
            summary = $"{summary}; Details={string.Join(" | ", result.Errors)}";

        var entry = new LogEntryDto
        {
            TimestampUtc = DateTime.UtcNow,
            JobName = job.Name,
            SourcePath = job.SourcePath,
            TargetPath = job.TargetPath,
            FileSizeBytes = result.TotalBytesProcessed,
            TransferTimeMs = result.Duration.TotalMilliseconds,
            Status = result.ErrorCount == 0 ? "OK" : "ERROR",
            ErrorMessage = summary
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
        state.CurrentSourceFile = sourcePath;
        state.CurrentTargetFile = targetPath;

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
