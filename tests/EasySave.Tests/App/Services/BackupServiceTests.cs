using EasySave.App.Services;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using EasySave.tests.Helpers.Builders;
using System.Diagnostics;
using System.Collections.Concurrent;

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
        var job = BackupJobBuilder.Valid()
            .WithId("1")
            .WithName("Job")
            .WithSource(source)
            .WithTarget(target)
            .WithType(BackupType.Full)
            .Build();

        var result = service.Run(job);

        await Task.Delay(200);

        Assert.True(File.Exists(Path.Combine(target, "test.txt")));
        Assert.True(jobService.MarkExecutedCalled);

        Directory.Delete(source, true);
        Directory.Delete(target, true);
    }

    [Fact]
    public void Run_ShouldRejectDuplicateLaunch_ForSameJob()
    {
        var source = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "hold.txt"), new string('a', 1024 * 1024));
        var target = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            var job = BackupJobBuilder.Valid()
                .WithId("10")
                .WithName("Job")
                .WithSource(source)
                .WithTarget(target)
                .WithType(BackupType.Full)
                .Build();
            var jobService = new FakeJobService(new[] { job });
            var service = new BackupService(jobService, AppConfig.LoadDefaults(), stateWriter: new NoOpStateWriter());

            var result1 = service.Run(job);
            var result2 = service.Run(job);

            Assert.True(result1.Success);
            Assert.False(result2.Success);
            Assert.Contains("already running", result2.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            TryDeleteDirectory(source);
            TryDeleteDirectory(target);
        }
    }

    [Fact]
    public async Task Run_ShouldAllowSimultaneousDifferentJobs()
    {
        var source1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var target1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var source2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var target2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(source1);
        Directory.CreateDirectory(source2);
        File.WriteAllText(Path.Combine(source1, "a.txt"), "hello");
        File.WriteAllText(Path.Combine(source2, "b.txt"), "world");

        try
        {
            var job1 = BackupJobBuilder.Valid()
                .WithId("10")
                .WithName("Job1")
                .WithSource(source1)
                .WithTarget(target1)
                .WithType(BackupType.Full)
                .Build();
            var job2 = BackupJobBuilder.Valid()
                .WithId("11")
                .WithName("Job2")
                .WithSource(source2)
                .WithTarget(target2)
                .WithType(BackupType.Full)
                .Build();
            var jobService = new FakeJobService(new[] { job1, job2 });
            var service = new BackupService(jobService, AppConfig.LoadDefaults(), stateWriter: new NoOpStateWriter());

            var result1 = service.Run(job1);
            var result2 = service.Run(job2);

            Assert.True(result1.Success);
            Assert.True(result2.Success);

            await WaitUntilAsync(() =>
                    File.Exists(Path.Combine(target1, "a.txt")) &&
                    File.Exists(Path.Combine(target2, "b.txt")) &&
                    jobService.MarkExecutedIds.Count >= 2,
                TimeSpan.FromSeconds(5));

            Assert.Contains("10", jobService.MarkExecutedIds);
            Assert.Contains("11", jobService.MarkExecutedIds);
        }
        finally
        {
            TryDeleteDirectory(source1);
            TryDeleteDirectory(target1);
            TryDeleteDirectory(source2);
            TryDeleteDirectory(target2);
        }
    }

    [Fact]
    public void Run_ShouldWriteRunningSnapshot_Immediately()
    {
        var source = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var target = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var job = BackupJobBuilder.Valid()
            .WithId("42")
            .WithName("Job")
            .WithSource(source)
            .WithTarget(target)
            .WithType(BackupType.Full)
            .Build();
        var jobService = new FakeJobService(new[] { job });
        var writer = new CapturingStateWriter();
        var service = new BackupService(jobService, AppConfig.LoadDefaults(), stateWriter: writer);

        service.Run(job);

        var snapshots = writer.GetSnapshots();
        Assert.Contains(snapshots, snapshot =>
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
        public ConcurrentBag<string> MarkExecutedIds { get; } = new();
        private readonly List<BackupJob> _jobs;

        public FakeJobService(IEnumerable<BackupJob>? jobs = null)
        {
            _jobs = jobs?.ToList() ?? new List<BackupJob>();
        }

        public IReadOnlyList<BackupJob> GetAll() => _jobs;
        public BackupJob? GetById(string id) => _jobs.FirstOrDefault(job => job.Id == id);
        public void Create(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive = true, List<string>? priorityExtensions = null) { }
        public void Update(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive, List<string>? priorityExtensions = null) { }
        public void MarkExecuted(string id, DateTime? nowUtc = null)
        {
            MarkExecutedCalled = true;
            MarkExecutedIds.Add(id);
        }
        public void Delete(string id) { }
    }

    private sealed class NoOpStateWriter : IStateWriter
    {
        public void Write(AppStateDto state) { }
    }

    private sealed class CapturingStateWriter : IStateWriter
    {
        private readonly object _sync = new();
        private readonly List<AppStateDto> _snapshots = new();

        public void Write(AppStateDto state)
        {
            lock (_sync)
            {
                _snapshots.Add(state);
            }
        }

        public IReadOnlyList<AppStateDto> GetSnapshots()
        {
            lock (_sync)
            {
                return _snapshots.ToList();
            }
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
                return;

            await Task.Delay(25);
        }

        Assert.True(condition(), "Condition not reached within timeout.");
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        catch
        {
            // Best effort cleanup for temp test folders.
        }
    }
}


