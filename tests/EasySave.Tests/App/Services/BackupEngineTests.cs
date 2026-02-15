using EasySave.App.Services;
using EasySave.Core.Enums;
using EasySave.Core.Models;
using EasySave.Core.Resources;
using System.Diagnostics;

namespace EasySave.Tests.App.Services;

public class BackupEngineTests : IDisposable
{
    private readonly string _basePath;

    public BackupEngineTests()
    {
        _basePath = Path.Combine(Path.GetTempPath(), "EasySave.Tests", Guid.NewGuid().ToString("N"));
    }

    [Fact]
    public void Run_ShouldReturnFailure_WhenSourceMissing()
    {
        var engine = new BackupEngine(AppConfig.LoadDefaults());
        var job = new BackupJob(
            "1",
            "Missing source",
            Path.Combine(_basePath, "Missing"),
            Path.Combine(_basePath, "Target"),
            BackupType.Full
        );

        var result = engine.Run(job);

        Assert.False(result.Success);
        Assert.Contains(string.Format(Strings.Error_SourceFolderMissing, job.SourcePath), result.Message);
    }

    [Fact]
    public void Run_ShouldCopyFile_WhenDifferentialAndTargetMissing()
    {
        var source = Path.Combine(_basePath, "Source");
        var target = Path.Combine(_basePath, "Target");
        Directory.CreateDirectory(source);

        var sourceFile = Path.Combine(source, "file.txt");
        File.WriteAllText(sourceFile, "content");

        var engine = new BackupEngine(AppConfig.LoadDefaults());
        var job = new BackupJob("2", "Diff job", source, target, BackupType.Differential);

        var result = engine.Run(job);

        Assert.True(result.Success);
        Assert.Equal(1, result.CopiedCount);
        Assert.True(File.Exists(Path.Combine(target, "file.txt")));
    }

    [Fact]
    public void Run_ShouldReturnFailure_WhenBusinessSoftwareRunning()
    {
        var config = AppConfig.LoadDefaults();
        var engine = new BackupEngine(config);
        var processName = Process.GetCurrentProcess().ProcessName;
        config.ChangeBussinessSoftware(processName);
        var job = new BackupJob(
            "3",
            "Business software running",
            Path.Combine(_basePath, "Source"),
            Path.Combine(_basePath, "Target"),
            BackupType.Full);

        var result = engine.Run(job);

        Assert.False(result.Success);
        Assert.Contains(processName, result.Message);
        Assert.Equal(1, result.ErrorCount);
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
            Directory.Delete(_basePath, true);
    }
}

