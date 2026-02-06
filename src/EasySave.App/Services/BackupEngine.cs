using System.Diagnostics;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
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

    public BackupEngine(string? logDirectory = null)
    {
        if (!string.IsNullOrWhiteSpace(logDirectory))
        {
            var options = new LogOptions
            {
                LogDirectory = logDirectory
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

        if (!Directory.Exists(job.SourcePath))
        {
            result.Success = false;
            result.Message = string.Format(Strings.Error_SourceFolderMissing, job.SourcePath);
            result.Errors.Add(result.Message);
            result.ErrorCount = result.Errors.Count;
            result.Duration = stopwatch.Elapsed;
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
            WriteSummaryLog(job, result);
            return result;
        }

        ExecuteBackup(job.SourcePath, job.TargetPath, strategy, result);

        result.Success = result.ErrorCount == 0;
        result.Message = result.Success ? Strings.Backup_Success : Strings.Info_BackupCompletedWithErrors;
        result.Duration = stopwatch.Elapsed;
        WriteSummaryLog(job, result);
        return result;
    }

    private static void ExecuteBackup(
        string sourceRoot,
        string targetRoot,
        IBackupCopyStrategy strategy,
        BackupResultDto result)
    {
        foreach (var sourcePath in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            result.FilesProcessed++;

            var relativePath = Path.GetRelativePath(sourceRoot, sourcePath);
            var targetPath = Path.Combine(targetRoot, relativePath);

            try
            {
                if (!strategy.ShouldCopy(sourcePath, targetPath))
                {
                    result.SkippedCount++;
                    continue;
                }

                CopyFile(sourcePath, targetPath);
                result.CopiedCount++;
                result.TotalBytesProcessed += new FileInfo(sourcePath).Length;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{sourcePath} -> {targetPath}: {ex.Message}");
                result.ErrorCount++;
            }
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
}
