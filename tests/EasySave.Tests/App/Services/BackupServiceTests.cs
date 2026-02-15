using EasySave.App.Services;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;

namespace EasySave.Tests.App.Services;

public class BackupServiceTests
{
    [Fact]
    public void Run_ShouldCopyFiles_ForFullBackup()
    {
        var source = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var target = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "test.txt"), "hello");

        var jobService = new FakeJobService();
        var service = new BackupService(jobService, stateWriter: new NoOpStateWriter());
        var job = new BackupJob("1", "Job", source, target, BackupType.Full);

        var result = service.Run(job);

        Assert.True(result.Success);
        Assert.True(File.Exists(Path.Combine(target, "test.txt")));
        Assert.True(jobService.MarkExecutedCalled);

        Directory.Delete(source, true);
        Directory.Delete(target, true);
    }

    [Fact]
    public void Run_ShouldSkipInactiveJob_AndNotMarkExecuted()
    {
        var jobService = new FakeJobService();
        var service = new BackupService(jobService, stateWriter: new NoOpStateWriter());
        var job = new BackupJob("1", "Job", @"C:\source", @"C:\target", BackupType.Full, isActive: false);

        var result = service.Run(job);

        Assert.False(result.Success);
        Assert.Equal("job desactivé run skipper", result.Message);
        Assert.False(jobService.MarkExecutedCalled);
    }

    private sealed class FakeJobService : IJobService
    {
        public bool MarkExecutedCalled { get; private set; }

        public IReadOnlyList<BackupJob> GetAll() => Array.Empty<BackupJob>();
        public BackupJob? GetById(string id) => null;
        public void Create(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive = true) { }
        public void Update(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive) { }
        public void MarkExecuted(string id, DateTime? nowUtc = null) => MarkExecutedCalled = true;
        public void Delete(string id) { }
    }

    private sealed class NoOpStateWriter : IStateWriter
    {
        public void Write(AppStateDto state) { }
    }
}


