using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.EasyLog.Options;

namespace EasySave.Tests.Core.DTO;

public class LogEntryDtoTests
{
    [Fact]
    public void Defaults_ShouldBeInitialized()
    {
        var dto = new LogEntryDto();

        Assert.NotEqual(default, dto.TimestampUtc);
        Assert.Equal(LogLevel.Info, dto.Level);
        Assert.Equal(string.Empty, dto.Message);
        Assert.Equal(1, dto.SchemaVersion);

        Assert.NotNull(dto.Event);
        Assert.Equal(string.Empty, dto.Event.Name);
        Assert.Equal(LogEventCategory.System, dto.Event.Category);
        Assert.Equal(LogEventAction.Unknown, dto.Event.Action);
        Assert.Equal(LogEventOutcome.Success, dto.Event.Outcome);

        Assert.Null(dto.App);
        Assert.Null(dto.Trace);
        Assert.Null(dto.Host);
        Assert.Null(dto.Job);
        Assert.Null(dto.File);
        Assert.Null(dto.Crypto);
        Assert.Null(dto.Settings);
        Assert.Null(dto.Summary);
        Assert.Null(dto.Error);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var t = DateTime.UtcNow;

        var dto = new LogEntryDto
        {
            TimestampUtc = t,
            Level = LogLevel.Error,
            Message = "failed",
            SchemaVersion = 2,
            Event = new LogEventDto
            {
                Name = "job.created",
                Category = LogEventCategory.Job,
                Action = LogEventAction.Create,
                Outcome = LogEventOutcome.Success
            },
            App = new LogAppDto { Name = "EasySave", Version = "1.2.3" },
            Trace = new LogTraceDto { Id = "trace-1", SpanId = "span-1" },
            Host = new LogHostDto { Name = "host", User = "user", Pid = 123 },
            Job = new LogJobDto
            {
                Id = "job-1",
                Name = "Job",
                Type = BackupType.Full,
                Status = JobStatus.Running,
                IsActive = true,
                RunId = "run-1",
                Strategy = "sequential",
                SourcePath = "C:\\Src",
                TargetPath = "D:\\Dst"
            },
            File = new LogFileDto
            {
                SourcePath = "C:\\Src\\a.txt",
                TargetPath = "D:\\Dst\\a.txt",
                SizeBytes = 123,
                TransferTimeMs = 45.6,
                IsDirectory = false,
                Priority = true
            },
            Crypto = new LogCryptoDto
            {
                Tool = "CryptoSoft",
                ExtensionMatched = true,
                EncryptionTimeMs = 10,
                Extension = ".txt",
                InstanceLock = "acquired"
            },
            Settings = new LogSettingsDto
            {
                Language = Language.English,
                LogFormat = LogFormat.Json,
                LogDirectory = "C:\\Logs",
                ConfigPath = "C:\\Config\\setting.json"
            },
            Summary = new LogSummaryDto
            {
                CopiedCount = 1,
                SkippedCount = 2,
                ErrorCount = 3,
                TotalBytes = 100,
                DurationMs = 2000,
                Details = "details"
            },
            Error = new LogErrorDto
            {
                Type = "IO",
                Code = "E_IO",
                Message = "boom",
                Stack = "stack"
            }
        };

        Assert.Equal(t, dto.TimestampUtc);
        Assert.Equal(LogLevel.Error, dto.Level);
        Assert.Equal("failed", dto.Message);
        Assert.Equal(2, dto.SchemaVersion);

        Assert.Equal("job.created", dto.Event.Name);
        Assert.Equal(LogEventCategory.Job, dto.Event.Category);
        Assert.Equal(LogEventAction.Create, dto.Event.Action);
        Assert.Equal(LogEventOutcome.Success, dto.Event.Outcome);

        Assert.Equal("EasySave", dto.App?.Name);
        Assert.Equal("1.2.3", dto.App?.Version);
        Assert.Equal("trace-1", dto.Trace?.Id);
        Assert.Equal("span-1", dto.Trace?.SpanId);
        Assert.Equal("host", dto.Host?.Name);
        Assert.Equal("user", dto.Host?.User);
        Assert.Equal(123, dto.Host?.Pid);

        Assert.Equal("job-1", dto.Job?.Id);
        Assert.Equal("Job", dto.Job?.Name);
        Assert.Equal(BackupType.Full, dto.Job?.Type);
        Assert.Equal(JobStatus.Running, dto.Job?.Status);
        Assert.True(dto.Job?.IsActive);
        Assert.Equal("run-1", dto.Job?.RunId);
        Assert.Equal("sequential", dto.Job?.Strategy);
        Assert.Equal("C:\\Src", dto.Job?.SourcePath);
        Assert.Equal("D:\\Dst", dto.Job?.TargetPath);

        Assert.Equal("C:\\Src\\a.txt", dto.File?.SourcePath);
        Assert.Equal("D:\\Dst\\a.txt", dto.File?.TargetPath);
        Assert.Equal(123, dto.File?.SizeBytes);
        Assert.Equal(45.6, dto.File?.TransferTimeMs);
        Assert.False(dto.File?.IsDirectory);
        Assert.True(dto.File?.Priority);

        Assert.Equal("CryptoSoft", dto.Crypto?.Tool);
        Assert.True(dto.Crypto?.ExtensionMatched);
        Assert.Equal(10, dto.Crypto?.EncryptionTimeMs);
        Assert.Equal(".txt", dto.Crypto?.Extension);
        Assert.Equal("acquired", dto.Crypto?.InstanceLock);

        Assert.Equal(Language.English, dto.Settings?.Language);
        Assert.Equal(LogFormat.Json, dto.Settings?.LogFormat);
        Assert.Equal("C:\\Logs", dto.Settings?.LogDirectory);
        Assert.Equal("C:\\Config\\setting.json", dto.Settings?.ConfigPath);

        Assert.Equal(1, dto.Summary?.CopiedCount);
        Assert.Equal(2, dto.Summary?.SkippedCount);
        Assert.Equal(3, dto.Summary?.ErrorCount);
        Assert.Equal(100, dto.Summary?.TotalBytes);
        Assert.Equal(2000, dto.Summary?.DurationMs);
        Assert.Equal("details", dto.Summary?.Details);

        Assert.Equal("IO", dto.Error?.Type);
        Assert.Equal("E_IO", dto.Error?.Code);
        Assert.Equal("boom", dto.Error?.Message);
        Assert.Equal("stack", dto.Error?.Stack);
    }
}
