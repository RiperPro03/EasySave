using EasySave.Core.Enums;
using EasySave.Core.Logging;
using EasySave.EasyLog.Options;

namespace EasySave.Tests.Core.Logging;

public class LogEntryBuilderTests
{
    [Fact]
    public void Create_ShouldInitializeCoreFields()
    {
        var entry = LogEntryBuilder.Create(
                "job.created",
                LogEventCategory.Job,
                LogEventAction.Create,
                "created")
            .Build();

        Assert.NotEqual(default, entry.TimestampUtc);
        Assert.Equal(LogLevel.Info, entry.Level);
        Assert.Equal("created", entry.Message);
        Assert.Equal("job.created", entry.Event.Name);
        Assert.Equal(LogEventCategory.Job, entry.Event.Category);
        Assert.Equal(LogEventAction.Create, entry.Event.Action);
        Assert.Equal(LogEventOutcome.Success, entry.Event.Outcome);
    }

    [Fact]
    public void WithTraceIfPresent_ShouldIgnoreNullOrWhitespace()
    {
        var entryWithoutTrace = LogEntryBuilder.Create(
                "job.updated",
                LogEventCategory.Job,
                LogEventAction.Update)
            .WithTraceIfPresent("   ")
            .Build();

        Assert.Null(entryWithoutTrace.Trace);

        var entryWithTrace = LogEntryBuilder.Create(
                "job.updated",
                LogEventCategory.Job,
                LogEventAction.Update)
            .WithTraceIfPresent("trace-1")
            .Build();

        Assert.NotNull(entryWithTrace.Trace);
        Assert.Equal("trace-1", entryWithTrace.Trace?.Id);
    }

    [Fact]
    public void WithJobFileSummary_ShouldSetFields()
    {
        var entry = LogEntryBuilder.Create(
                "file.transferred",
                LogEventCategory.File,
                LogEventAction.Transfer)
            .WithJob(
                "job-1",
                "Job",
                BackupType.Full,
                "C:\\Src",
                "D:\\Dst",
                JobStatus.Running,
                true)
            .WithFile(
                "C:\\Src\\a.txt",
                "D:\\Dst\\a.txt",
                123,
                45.6)
            .WithSummary(1, 2, 3, 100, 2000, "details")
            .Build();

        Assert.Equal("job-1", entry.Job?.Id);
        Assert.Equal("Job", entry.Job?.Name);
        Assert.Equal(BackupType.Full, entry.Job?.Type);
        Assert.Equal(JobStatus.Running, entry.Job?.Status);
        Assert.True(entry.Job?.IsActive);
        Assert.Equal("C:\\Src", entry.Job?.SourcePath);
        Assert.Equal("D:\\Dst", entry.Job?.TargetPath);

        Assert.Equal("C:\\Src\\a.txt", entry.File?.SourcePath);
        Assert.Equal("D:\\Dst\\a.txt", entry.File?.TargetPath);
        Assert.Equal(123, entry.File?.SizeBytes);
        Assert.Equal(45.6, entry.File?.TransferTimeMs);
        Assert.False(entry.File?.IsDirectory);

        Assert.Equal(1, entry.Summary?.CopiedCount);
        Assert.Equal(2, entry.Summary?.SkippedCount);
        Assert.Equal(3, entry.Summary?.ErrorCount);
        Assert.Equal(100, entry.Summary?.TotalBytes);
        Assert.Equal(2000, entry.Summary?.DurationMs);
        Assert.Equal("details", entry.Summary?.Details);
    }

    [Fact]
    public void WithCrypto_ShouldSetFields()
    {
        var entry = LogEntryBuilder.Create(
                "file.encrypted",
                LogEventCategory.File,
                LogEventAction.Transfer)
            .WithCrypto(
                tool: "CryptoSoft",
                extensionMatched: true,
                encryptionTimeMs: 120,
                extension: ".pdf",
                instanceLock: "acquired")
            .Build();

        Assert.NotNull(entry.Crypto);
        Assert.Equal("CryptoSoft", entry.Crypto?.Tool);
        Assert.True(entry.Crypto?.ExtensionMatched);
        Assert.Equal(120, entry.Crypto?.EncryptionTimeMs);
        Assert.Equal(".pdf", entry.Crypto?.Extension);
        Assert.Equal("acquired", entry.Crypto?.InstanceLock);
    }

    [Fact]
    public void WithSettings_ShouldSetFields()
    {
        var entry = LogEntryBuilder.Create(
                "settings.updated",
                LogEventCategory.Settings,
                LogEventAction.Update)
            .WithSettings(
                language: Language.English,
                logFormat: LogFormat.Json,
                logDirectory: "C:\\Logs",
                configPath: "C:\\Config\\settings.json")
            .Build();

        Assert.NotNull(entry.Settings);
        Assert.Equal(Language.English, entry.Settings?.Language);
        Assert.Equal(LogFormat.Json, entry.Settings?.LogFormat);
        Assert.Equal("C:\\Logs", entry.Settings?.LogDirectory);
        Assert.Equal("C:\\Config\\settings.json", entry.Settings?.ConfigPath);
    }

    [Fact]
    public void WithOutcome_ShouldOverrideEventOutcome()
    {
        var entry = LogEntryBuilder.Create(
                "job.updated",
                LogEventCategory.Job,
                LogEventAction.Update)
            .WithOutcome(LogEventOutcome.Failure)
            .Build();

        Assert.Equal(LogEventOutcome.Failure, entry.Event.Outcome);
        Assert.Null(entry.Error);
    }

    [Fact]
    public void WithLevel_ShouldOverrideSeverity()
    {
        var entry = LogEntryBuilder.Create(
                "job.updated",
                LogEventCategory.Job,
                LogEventAction.Update)
            .WithLevel(LogLevel.Warning)
            .Build();

        Assert.Equal(LogLevel.Warning, entry.Level);
    }

    [Fact]
    public void Fail_ShouldSetOutcomeAndError()
    {
        var entry = LogEntryBuilder.Create(
                "job.created",
                LogEventCategory.Job,
                LogEventAction.Create)
            .Fail("IO", "boom", "E_IO", "stack")
            .Build();

        Assert.Equal(LogEventOutcome.Failure, entry.Event.Outcome);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.NotNull(entry.Error);
        Assert.Equal("IO", entry.Error?.Type);
        Assert.Equal("E_IO", entry.Error?.Code);
        Assert.Equal("boom", entry.Error?.Message);
        Assert.Equal("stack", entry.Error?.Stack);
    }
}
