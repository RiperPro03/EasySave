using EasySave.App.Services;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using System.Diagnostics;

namespace EasySave.Tests.App.Services;

public class BackupServiceTests
{
    [Fact]
    public async Task Run_ShouldCopyFiles_ForFullBackup()
    {
        var source = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var target = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "test.txt"), "hello");

        var jobService = new FakeJobService();
        var service = new BackupService(jobService, AppConfig.LoadDefaults(), stateWriter: new NoOpStateWriter());
        var job = new BackupJob("1", "Job", source, target, BackupType.Full);

        var result = service.Run(job);

        await Task.Delay(200);

        Assert.True(File.Exists(Path.Combine(target, "test.txt")));
        Assert.True(jobService.MarkExecutedCalled);

        Directory.Delete(source, true);
        Directory.Delete(target, true);
    }

    [Fact]
    public void Run_ShouldAllowParallelJobs()
    {
        var source = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(source);
        var target = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var job = new BackupJob("10", "Job", source, target, BackupType.Full);
        var jobService = new FakeJobService(new[] { job });
        var service = new BackupService(jobService, AppConfig.LoadDefaults(), stateWriter: new NoOpStateWriter());

        var result1 = service.Run(job);
        var result2 = service.Run(job);

        Assert.True(result1.Success);
        Assert.True(result2.Success);

        Directory.Delete(source, true);
    }

    [Fact]
    public void Run_ShouldWriteRunningSnapshot_Immediately()
    {
        var source = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var target = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var job = new BackupJob("42", "Job", source, target, BackupType.Full);
        var jobService = new FakeJobService(new[] { job });
        var writer = new CapturingStateWriter();
        var service = new BackupService(jobService, AppConfig.LoadDefaults(), stateWriter: writer);

        service.Run(job);

        Assert.Contains(writer.Snapshots, snapshot =>
            snapshot.Jobs.Any(state => state.JobId == job.Id && state.Status == JobStatus.Running));
    }

    [Fact]
    public void CanStartSequence_ShouldReturnFalse_WhenBusinessSoftwareRunning()
    {
        var jobService = new FakeJobService();
        var config = AppConfig.LoadDefaults();
        var processName = Process.GetCurrentProcess().ProcessName;
        config.ChangeBussinessSoftware(processName);
        var service = new BackupService(jobService, config, stateWriter: new NoOpStateWriter());

        var canStart = service.CanStartSequence(out var reason);

        Assert.False(canStart);
        Assert.NotNull(reason);
        Assert.Contains(processName, reason);
    }

    [Fact]
    public void CanStartSequence_ShouldReturnTrue_WhenBusinessSoftwareNotConfigured()
    {
        var jobService = new FakeJobService();
        var config = AppConfig.LoadDefaults();
        var service = new BackupService(jobService, config, stateWriter: new NoOpStateWriter());

        var canStart = service.CanStartSequence(out var reason);

        Assert.True(canStart);
        Assert.Null(reason);
    }

    private sealed class FakeJobService : IJobService
    {
        public bool MarkExecutedCalled { get; private set; }
        private readonly List<BackupJob> _jobs;

        public FakeJobService(IEnumerable<BackupJob>? jobs = null)
        {
            _jobs = jobs?.ToList() ?? new List<BackupJob>();
        }

        public IReadOnlyList<BackupJob> GetAll() => _jobs;
        public BackupJob? GetById(string id) => _jobs.FirstOrDefault(job => job.Id == id);
        public void Create(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive = true) { }
        public void Update(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive) { }
        public void MarkExecuted(string id, DateTime? nowUtc = null) => MarkExecutedCalled = true;
        public void Delete(string id) { }
    }

    private sealed class NoOpStateWriter : IStateWriter
    {
        public void Write(AppStateDto state) { }
    }

    private sealed class CapturingStateWriter : IStateWriter
    {
        public List<AppStateDto> Snapshots { get; } = new();

        public void Write(AppStateDto state)
        {
            Snapshots.Add(state);
        }
    }
}


