using System;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.EasyLog.Options;

namespace EasySave.Core.Logging;

/// <summary>
/// Fluent builder for unified log entries.
/// </summary>
public sealed class LogEntryBuilder
{
    private readonly LogEntryDto _entry;

    private LogEntryBuilder(
        string eventName,
        LogEventCategory category,
        LogEventAction action,
        string message)
    {
        _entry = new LogEntryDto
        {
            TimestampUtc = DateTime.UtcNow,
            Level = LogLevel.Info,
            Message = message ?? string.Empty,
            Event = new LogEventDto
            {
                Name = eventName ?? string.Empty,
                Category = category,
                Action = action,
                Outcome = LogEventOutcome.Success
            }
        };
    }

    /// <summary>
    /// Creates a new log entry builder with the required event metadata.
    /// </summary>
    /// <param name="eventName">Event identifier (ex: job.created).</param>
    /// <param name="category">High level category.</param>
    /// <param name="action">Action performed.</param>
    /// <param name="message">Human readable message.</param>
    /// <returns>The configured builder.</returns>
    /// <example>
    /// var entry = LogEntryBuilder.Create("job.created", LogEventCategory.Job, LogEventAction.Create, "created")
    ///     .Build();
    /// </example>
    public static LogEntryBuilder Create(
        string eventName,
        LogEventCategory category,
        LogEventAction action,
        string message = "")
        => new(eventName, category, action, message);

    /// <summary>
    /// Adds a trace identifier when provided.
    /// </summary>
    /// <param name="traceId">Trace identifier.</param>
    /// <returns>The current builder.</returns>
    public LogEntryBuilder WithTraceIfPresent(string? traceId)
    {
        if (string.IsNullOrWhiteSpace(traceId))
            return this;

        _entry.Trace = new LogTraceDto { Id = traceId };
        return this;
    }

    /// <summary>
    /// Adds job metadata to the entry.
    /// </summary>
    /// <param name="id">Job id.</param>
    /// <param name="name">Job name.</param>
    /// <param name="type">Backup type.</param>
    /// <param name="sourcePath">Source path.</param>
    /// <param name="targetPath">Target path.</param>
    /// <param name="status">Optional job status.</param>
    /// <param name="isActive">Optional active flag.</param>
    /// <param name="runId">Optional run id.</param>
    /// <param name="strategy">Optional run strategy.</param>
    /// <returns>The current builder.</returns>
    public LogEntryBuilder WithJob(
        string id,
        string name,
        BackupType type,
        string sourcePath,
        string targetPath,
        JobStatus? status = null,
        bool? isActive = null,
        string? runId = null,
        string? strategy = null)
    {
        _entry.Job = new LogJobDto
        {
            Id = id,
            Name = name,
            Type = type,
            Status = status,
            IsActive = isActive,
            RunId = runId,
            Strategy = strategy,
            SourcePath = sourcePath,
            TargetPath = targetPath
        };
        return this;
    }

    /// <summary>
    /// Adds file metadata to the entry.
    /// </summary>
    /// <param name="sourcePath">File source path.</param>
    /// <param name="targetPath">File target path.</param>
    /// <param name="sizeBytes">File size in bytes.</param>
    /// <param name="transferTimeMs">Transfer duration in milliseconds.</param>
    /// <param name="isDirectory">Whether the entry is a directory.</param>
    /// <param name="priority">Optional priority flag.</param>
    /// <returns>The current builder.</returns>
    public LogEntryBuilder WithFile(
        string sourcePath,
        string targetPath,
        long sizeBytes,
        double transferTimeMs,
        bool isDirectory = false,
        bool? priority = null)
    {
        _entry.File = new LogFileDto
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            SizeBytes = sizeBytes,
            TransferTimeMs = transferTimeMs,
            IsDirectory = isDirectory,
            Priority = priority
        };
        return this;
    }

    /// <summary>
    /// Adds crypto metadata to the entry.
    /// </summary>
    /// <param name="tool">Crypto tool name.</param>
    /// <param name="extensionMatched">Whether the file matched crypto extensions.</param>
    /// <param name="encryptionTimeMs">0=no encryption, &gt;0 duration ms, &lt;0 error code.</param>
    /// <param name="extension">Optional evaluated extension.</param>
    /// <param name="instanceLock">Optional mono-instance status.</param>
    /// <returns>The current builder.</returns>
    public LogEntryBuilder WithCrypto(
        string tool,
        bool extensionMatched,
        long encryptionTimeMs,
        string? extension = null,
        string? instanceLock = null)
    {
        _entry.Crypto = new LogCryptoDto
        {
            Tool = tool,
            ExtensionMatched = extensionMatched,
            EncryptionTimeMs = encryptionTimeMs,
            Extension = extension,
            InstanceLock = instanceLock
        };
        return this;
    }

    /// <summary>
    /// Adds settings metadata to the entry.
    /// </summary>
    /// <param name="language">Selected language.</param>
    /// <param name="logFormat">Selected log format.</param>
    /// <param name="logDirectory">Optional log directory.</param>
    /// <param name="configPath">Optional config path.</param>
    /// <returns>The current builder.</returns>
    public LogEntryBuilder WithSettings(
        Language language,
        LogFormat logFormat,
        string? logDirectory = null,
        string? configPath = null)
    {
        _entry.Settings = new LogSettingsDto
        {
            Language = language,
            LogFormat = logFormat,
            LogDirectory = logDirectory,
            ConfigPath = configPath
        };
        return this;
    }

    /// <summary>
    /// Adds execution summary counters to the entry.
    /// </summary>
    /// <param name="copiedCount">Number of copied files.</param>
    /// <param name="skippedCount">Number of skipped files.</param>
    /// <param name="errorCount">Number of errors.</param>
    /// <param name="totalBytes">Total bytes processed.</param>
    /// <param name="durationMs">Total duration in milliseconds.</param>
    /// <param name="details">Optional details string.</param>
    /// <returns>The current builder.</returns>
    public LogEntryBuilder WithSummary(
        int copiedCount,
        int skippedCount,
        int errorCount,
        long totalBytes,
        double durationMs,
        string? details = null)
    {
        _entry.Summary = new LogSummaryDto
        {
            CopiedCount = copiedCount,
            SkippedCount = skippedCount,
            ErrorCount = errorCount,
            TotalBytes = totalBytes,
            DurationMs = durationMs,
            Details = details
        };
        return this;
    }

    /// <summary>
    /// Overrides the event outcome.
    /// </summary>
    /// <param name="outcome">Outcome to set.</param>
    /// <returns>The current builder.</returns>
    public LogEntryBuilder WithOutcome(LogEventOutcome outcome)
    {
        _entry.Event.Outcome = outcome;
        return this;
    }

    /// <summary>
    /// Overrides the severity level.
    /// </summary>
    /// <param name="level">Severity level.</param>
    /// <returns>The current builder.</returns>
    public LogEntryBuilder WithLevel(LogLevel level)
    {
        _entry.Level = level;
        return this;
    }

    /// <summary>
    /// Marks the entry as failed and attaches error details.
    /// </summary>
    /// <param name="errorType">Error type identifier.</param>
    /// <param name="errorMessage">Error message.</param>
    /// <param name="errorCode">Optional error code.</param>
    /// <param name="errorStack">Optional error stack.</param>
    /// <returns>The current builder.</returns>
    public LogEntryBuilder Fail(
        string errorType,
        string errorMessage,
        string? errorCode = null,
        string? errorStack = null)
    {
        _entry.Event.Outcome = LogEventOutcome.Failure;
        _entry.Level = LogLevel.Error;
        _entry.Error = new LogErrorDto
        {
            Type = errorType,
            Code = errorCode,
            Message = errorMessage,
            Stack = errorStack
        };
        return this;
    }

    /// <summary>
    /// Builds the final log entry.
    /// </summary>
    /// <returns>The built log entry.</returns>
    public LogEntryDto Build() => _entry;
}
