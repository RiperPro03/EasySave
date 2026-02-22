using System;
using EasySave.Core.Enums;
using EasySave.EasyLog.Options;

namespace EasySave.Core.DTO;

/// <summary>
/// Represents a unified log entry (JSON/XML).
/// </summary>
public sealed class LogEntryDto
{
    /// <summary>
    /// UTC timestamp for the log entry.
    /// </summary>
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Severity level.
    /// </summary>
    public LogLevel Level { get; set; } = LogLevel.Info;

    /// <summary>
    /// Human readable message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Schema version for backward compatibility.
    /// </summary>
    public int SchemaVersion { get; set; } = 2;

    /// <summary>
    /// Event metadata (name/category/action/outcome).
    /// </summary>
    public LogEventDto Event { get; set; } = new();

    /// <summary>
    /// Optional application metadata.
    /// </summary>
    public LogAppDto? App { get; set; }

    /// <summary>
    /// Optional trace identifiers.
    /// </summary>
    public LogTraceDto? Trace { get; set; }

    /// <summary>
    /// Optional host metadata.
    /// </summary>
    public LogHostDto? Host { get; set; }


    /// <summary>
    /// Optional job metadata.
    /// </summary>
    public LogJobDto? Job { get; set; }

    /// <summary>
    /// Optional file metadata.
    /// </summary>
    public LogFileDto? File { get; set; }

    /// <summary>
    /// Optional crypto metadata.
    /// </summary>
    public LogCryptoDto? Crypto { get; set; }

    /// <summary>
    /// Optional settings metadata.
    /// </summary>
    public LogSettingsDto? Settings { get; set; }

    /// <summary>
    /// Optional execution summary.
    /// </summary>
    public LogSummaryDto? Summary { get; set; }

    /// <summary>
    /// Optional error details.
    /// </summary>
    public LogErrorDto? Error { get; set; }
}

/// <summary>
/// Event metadata for a log entry.
/// </summary>
public sealed class LogEventDto
{
    /// <summary>
    /// Event identifier (ex: job.created).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// High level category.
    /// </summary>
    public LogEventCategory Category { get; set; } = LogEventCategory.System;

    /// <summary>
    /// Action performed.
    /// </summary>
    public LogEventAction Action { get; set; } = LogEventAction.Unknown;

    /// <summary>
    /// Outcome of the action.
    /// </summary>
    public LogEventOutcome Outcome { get; set; } = LogEventOutcome.Success;
}

/// <summary>
/// Application metadata.
/// </summary>
public sealed class LogAppDto
{
    /// <summary>
    /// Application name.
    /// </summary>
    public string Name { get; set; } = "EasySave";

    /// <summary>
    /// Application version.
    /// </summary>
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Trace correlation identifiers.
/// </summary>
public sealed class LogTraceDto
{
    /// <summary>
    /// Trace identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Host machine metadata.
/// </summary>
public sealed class LogHostDto
{
    /// <summary>
    /// Host machine name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User name.
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Optional process id.
    /// </summary>
    public int? Pid { get; set; }
}

/// <summary>
/// Run context metadata.
/// </summary>
/// <summary>
/// Job metadata.
/// </summary>
public sealed class LogJobDto
{
    /// <summary>
    /// Job identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Job name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Backup type.
    /// </summary>
    public BackupType? Type { get; set; }

    /// <summary>
    /// Current job status.
    /// </summary>
    public JobStatus? Status { get; set; }

    /// <summary>
    /// Whether the job is active.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Job source path.
    /// </summary>
    public string? SourcePath { get; set; }

    /// <summary>
    /// Job target path.
    /// </summary>
    public string? TargetPath { get; set; }
}

/// <summary>
/// File metadata.
/// </summary>
public sealed class LogFileDto
{
    /// <summary>
    /// File source path.
    /// </summary>
    public string? SourcePath { get; set; }

    /// <summary>
    /// File target path.
    /// </summary>
    public string? TargetPath { get; set; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long? SizeBytes { get; set; }

    /// <summary>
    /// Transfer duration in milliseconds.
    /// </summary>
    public double? TransferTimeMs { get; set; }

    /// <summary>
    /// Indicates whether the entry is a directory.
    /// </summary>
    public bool? IsDirectory { get; set; }
    
}

/// <summary>
/// Crypto metadata.
/// </summary>
public sealed class LogCryptoDto
{
    /// <summary>
    /// Crypto tool name.
    /// </summary>
    public string Tool { get; set; } = "CryptoSoft";

    /// <summary>
    /// Whether the file matched crypto extensions.
    /// </summary>
    public bool ExtensionMatched { get; set; }

    /// <summary>
    /// Required by the subject:
    /// 0  => no encryption,
    /// >0 => encryption duration in milliseconds,
    /// <0 => error code.
    /// </summary>
    public long EncryptionTimeMs { get; set; }

    /// <summary>
    /// Optional: evaluated file extension (ex: ".pdf").
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// Optional: CryptoSoft mono-instance status (acquired | waiting | failed).
    /// </summary>
    public string? InstanceLock { get; set; }
}

/// <summary>
/// Settings metadata.
/// </summary>
public sealed class LogSettingsDto
{
    /// <summary>
    /// Selected language.
    /// </summary>
    public Language? Language { get; set; }

    /// <summary>
    /// Selected log format.
    /// </summary>
    public LogFormat? LogFormat { get; set; }

    /// <summary>
    /// Log directory path.
    /// </summary>
    public string? LogDirectory { get; set; }

    /// <summary>
    /// Configuration file path.
    /// </summary>
    public string? ConfigPath { get; set; }

    /// <summary>
    /// Whether encryption is enabled.
    /// </summary>
    public bool? EncryptionEnabled { get; set; }

    /// <summary>
    /// Extensions to encrypt (ex: ".pdf").
    /// </summary>
    public List<string>? ExtensionsToEncrypt { get; set; }

    /// <summary>
    /// Business software process name.
    /// </summary>
    public string? BusinessSoftwareProcessName { get; set; }
}

/// <summary>
/// Execution summary counters.
/// </summary>
public sealed class LogSummaryDto
{
    /// <summary>
    /// Number of copied files.
    /// </summary>
    public int CopiedCount { get; set; }

    /// <summary>
    /// Number of skipped files.
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// Number of errors.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Total processed bytes.
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// Total duration in milliseconds.
    /// </summary>
    public double DurationMs { get; set; }

    /// <summary>
    /// Optional error details.
    /// </summary>
    public string? Details { get; set; }
}

/// <summary>
/// Error metadata.
/// </summary>
public sealed class LogErrorDto
{
    /// <summary>
    /// Error type identifier.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Optional error code.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Error message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Optional error stack trace.
    /// </summary>
    public string? Stack { get; set; }
}
