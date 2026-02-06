using System;
using EasySave.App.Services;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using Xunit;

namespace EasySave.App.Tests.App.Services;

// Fake qui simule un backup qui fonctionne
internal class FakeBackupService : IBackupService
{
    public bool WasCalled { get; private set; }

    public void FullBackup(string sourcePath, string targetPath)
    {
        WasCalled = true;
    }
}

// Fake qui simule une erreur
internal class FailingBackupService : IBackupService
{
    public void FullBackup(string sourcePath, string targetPath)
    {
        throw new Exception("Erreur test");
    }
}

public class BackupEngineTests
{
    [Fact]
    public void Run_ShouldReturnSuccess_WhenBackupSucceeds()
    {
        // Arrange
        var fakeService = new FakeBackupService();
        var engine = new BackupEngine(fakeService);

        var job = new BackupJob(
            "1",
            "Test job",
            "C:\\Source",
            "C:\\Target",
            BackupType.Full
        );

        // Act
        var result = engine.Run(job);

        // Assert
        Assert.True(result.Success);
        Assert.True(fakeService.WasCalled);
        Assert.NotEqual(TimeSpan.Zero, result.Duration);
    }

    [Fact]
    public void Run_ShouldReturnFailure_WhenBackupThrows()
    {
        // Arrange
        var failingService = new FailingBackupService();
        var engine = new BackupEngine(failingService);

        var job = new BackupJob(
            "2",
            "Fail job",
            "src",
            "dst",
            BackupType.Full
        );

        // Act
        var result = engine.Run(job);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Erreur test", result.Message);
    }
}