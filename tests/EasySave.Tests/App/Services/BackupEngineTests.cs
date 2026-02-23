using EasySave.App.Services;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;
using EasySave.Core.Resources;
using EasySave.tests.Helpers.Builders;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

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
        var job = BackupJobBuilder.Valid()
            .WithId("1")
            .WithName("Missing source")
            .WithSource(Path.Combine(_basePath, "Missing"))
            .WithTarget(Path.Combine(_basePath, "Target"))
            .WithType(BackupType.Full)
            .Build();

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
        var job = BackupJobBuilder.Valid()
            .WithId("2")
            .WithName("Diff job")
            .WithSource(source)
            .WithTarget(target)
            .WithType(BackupType.Differential)
            .Build();

        var result = engine.Run(job);

        Assert.True(result.Success);
        Assert.Equal(1, result.CopiedCount);
        Assert.True(File.Exists(Path.Combine(target, "file.txt")));
    }

    [Fact]
    public void Run_ShouldReturnFailure_WhenBusinessSoftwareRunning()
    {
        var config = AppConfig.LoadDefaults();
        var processName = Process.GetCurrentProcess().ProcessName;
        config.ChangeBussinessSoftware(processName);
        var engine = new BackupEngine(config);
        var job = BackupJobBuilder.Valid()
            .WithId("3")
            .WithName("Business software running")
            .WithSource(Path.Combine(_basePath, "Source"))
            .WithTarget(Path.Combine(_basePath, "Target"))
            .WithType(BackupType.Full)
            .Build();

        var result = engine.Run(job);

        Assert.False(result.Success);
        Assert.Contains(processName, result.Message);
        Assert.Equal(1, result.ErrorCount);
    }

    [Fact]
    public void Run_ShouldCallCrypto_WhenGlobalEncryptionEnabledAndExtensionMatches()
    {
        var source = Path.Combine(_basePath, "CryptoSource");
        var target = Path.Combine(_basePath, "CryptoTarget");
        Directory.CreateDirectory(source);

        var sourceFile = Path.Combine(source, "file.txt");
        File.WriteAllText(sourceFile, "content");

        var config = AppConfig.LoadDefaults();
        config.SetEncryptionEnabled(true);
        config.UpdateEncryptionKey("secret");
        config.UpdateExtensionsToEncrypt(new[] { ".txt" });

        var crypto = new FakeCryptoService(12);
        var engine = new BackupEngine(config, cryptoService: crypto);
        var job = BackupJobBuilder.Valid()
            .WithId("4")
            .WithName("Crypto job")
            .WithSource(source)
            .WithTarget(target)
            .WithType(BackupType.Full)
            .Build();

        var result = engine.Run(job);

        Assert.True(result.Success);
        Assert.Equal(1, crypto.CallCount);
        Assert.Equal(Path.Combine(target, "file.txt"), crypto.LastFilePath);
        Assert.Equal("secret", crypto.LastKey);
    }

    [Fact]
    public void Run_ShouldReturnFailure_WhenCryptoReturnsNegative()
    {
        var source = Path.Combine(_basePath, "CryptoFailSource");
        var target = Path.Combine(_basePath, "CryptoFailTarget");
        Directory.CreateDirectory(source);

        var sourceFile = Path.Combine(source, "file.txt");
        File.WriteAllText(sourceFile, "content");

        var config = AppConfig.LoadDefaults();
        config.SetEncryptionEnabled(true);
        config.UpdateEncryptionKey("secret");
        config.UpdateExtensionsToEncrypt(new[] { ".txt" });

        var crypto = new FakeCryptoService(-2);
        var engine = new BackupEngine(config, cryptoService: crypto);
        var job = BackupJobBuilder.Valid()
            .WithId("5")
            .WithName("Crypto fail job")
            .WithSource(source)
            .WithTarget(target)
            .WithType(BackupType.Full)
            .Build();

        var result = engine.Run(job);

        Assert.False(result.Success);
        Assert.True(result.ErrorCount > 0);
    }

    [Fact]
    public async Task PauseResume_ShouldPublishStateTransitions_AndComplete()
    {
        var source = Path.Combine(_basePath, "PauseResumeSource");
        var target = Path.Combine(_basePath, "PauseResumeTarget");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(source, "file2.txt"), "content2");

        var config = AppConfig.LoadDefaults();
        config.SetEncryptionEnabled(true);
        config.UpdateEncryptionKey("secret");
        config.UpdateExtensionsToEncrypt(new[] { ".txt" });

        using var crypto = new BlockingFirstCallCryptoService();
        var engine = new BackupEngine(config, cryptoService: crypto);
        var job = BackupJobBuilder.Valid()
            .WithId("pause-1")
            .WithName("Pause resume job")
            .WithSource(source)
            .WithTarget(target)
            .WithType(BackupType.Full)
            .Build();
        var states = new ConcurrentQueue<JobStateDto>();
        engine.StateChanged += (_, e) => states.Enqueue(CloneState(e.State));

        var runTask = Task.Run(() => engine.Run(job));

        Assert.True(crypto.WaitForFirstCall(TimeSpan.FromSeconds(3)));
        Assert.True(engine.Pause(job.Id));
        Assert.True(WaitForState(states, JobStatus.Paused, TimeSpan.FromSeconds(2)));

        crypto.ReleaseFirstCall();

        Assert.True(engine.Resume(job.Id));
        Assert.True(WaitForState(states, JobStatus.Running, TimeSpan.FromSeconds(2), minimumOccurrences: 2));

        var result = await runTask;

        Assert.True(result.Success);
        Assert.True(WaitForState(states, JobStatus.Completed, TimeSpan.FromSeconds(2)));
        Assert.True(File.Exists(Path.Combine(target, "file1.txt")));
        Assert.True(File.Exists(Path.Combine(target, "file2.txt")));
    }

    [Fact]
    public async Task Stop_ShouldPublishErrorState_AndReturnStoppedResult()
    {
        var source = Path.Combine(_basePath, "StopSource");
        var target = Path.Combine(_basePath, "StopTarget");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(source, "file2.txt"), "content2");

        var config = AppConfig.LoadDefaults();
        config.SetEncryptionEnabled(true);
        config.UpdateEncryptionKey("secret");
        config.UpdateExtensionsToEncrypt(new[] { ".txt" });

        using var crypto = new BlockingFirstCallCryptoService();
        var engine = new BackupEngine(config, cryptoService: crypto);
        var job = BackupJobBuilder.Valid()
            .WithId("stop-1")
            .WithName("Stop job")
            .WithSource(source)
            .WithTarget(target)
            .WithType(BackupType.Full)
            .Build();
        var states = new ConcurrentQueue<JobStateDto>();
        engine.StateChanged += (_, e) => states.Enqueue(CloneState(e.State));

        var runTask = Task.Run(() => engine.Run(job));

        Assert.True(crypto.WaitForFirstCall(TimeSpan.FromSeconds(3)));
        Assert.True(engine.Stop(job.Id));
        Assert.True(WaitForState(states, JobStatus.Error, TimeSpan.FromSeconds(2)));

        crypto.ReleaseFirstCall();
        var result = await runTask;

        Assert.False(result.Success);
        Assert.Contains(Strings.Error_BackupStoppedByUser, result.Message);
        Assert.Contains(states, s => s.Status == JobStatus.Error &&
                                     (s.ErrorMessage?.Contains(Strings.Error_BackupStoppedByUser, StringComparison.Ordinal) ?? false));
    }

    [Fact]
    public async Task Run_ShouldAutoPauseAndResume_WhenBusinessSoftwareDetectedDuringExecution()
    {
        var source = Path.Combine(_basePath, "AutoPauseSource");
        var target = Path.Combine(_basePath, "AutoPauseTarget");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(source, "file2.txt"), "content2");

        var config = AppConfig.LoadDefaults();
        config.SetEncryptionEnabled(true);
        config.UpdateEncryptionKey("secret");
        config.UpdateExtensionsToEncrypt(new[] { ".txt" });

        using var crypto = new BlockingFirstCallCryptoService();
        var engine = new BackupEngine(config, cryptoService: crypto);
        var job = BackupJobBuilder.Valid()
            .WithId("auto-pause-1")
            .WithName("Auto pause job")
            .WithSource(source)
            .WithTarget(target)
            .WithType(BackupType.Full)
            .Build();

        var states = new ConcurrentQueue<JobStateDto>();
        engine.StateChanged += (_, e) => states.Enqueue(CloneState(e.State));

        var runTask = Task.Run(() => engine.Run(job));

        Assert.True(crypto.WaitForFirstCall(TimeSpan.FromSeconds(3)));

        // Simule l'apparition du logiciel metier pendant le traitement du premier fichier.
        var businessProcessName = Process.GetCurrentProcess().ProcessName;
        config.ChangeBussinessSoftware(businessProcessName);

        crypto.ReleaseFirstCall();

        Assert.True(WaitForState(states, JobStatus.Paused, TimeSpan.FromSeconds(3)));

        var runningCountBeforeResume = states.Count(s => s.Status == JobStatus.Running);
        // Simule la fermeture (ou la desactivation de la surveillance) pour reprendre automatiquement.
        config.ChangeBussinessSoftware(null);

        Assert.True(WaitForState(states, JobStatus.Running, TimeSpan.FromSeconds(3), runningCountBeforeResume + 1));

        var result = await runTask;

        Assert.True(result.Success);
        Assert.True(WaitForState(states, JobStatus.Completed, TimeSpan.FromSeconds(2)));
        Assert.True(File.Exists(Path.Combine(target, "file1.txt")));
        Assert.True(File.Exists(Path.Combine(target, "file2.txt")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
            Directory.Delete(_basePath, true);
    }

    private sealed class FakeCryptoService : ICryptoService
    {
        private readonly int _result;

        public FakeCryptoService(int result)
        {
            _result = result;
        }

        public int CallCount { get; private set; }
        public string? LastFilePath { get; private set; }
        public string? LastKey { get; private set; }

        public Task<int> EncryptFileAsync(string filePath, string key)
        {
            CallCount++;
            LastFilePath = filePath;
            LastKey = key;
            return Task.FromResult(_result);
        }
    }

    private sealed class BlockingFirstCallCryptoService : ICryptoService, IDisposable
    {
        private readonly ManualResetEventSlim _firstCallStarted = new(false);
        private readonly ManualResetEventSlim _releaseFirstCall = new(false);
        private int _callCount;

        public Task<int> EncryptFileAsync(string filePath, string key)
        {
            var callIndex = Interlocked.Increment(ref _callCount);
            if (callIndex == 1)
            {
                _firstCallStarted.Set();
                _releaseFirstCall.Wait(TimeSpan.FromSeconds(5));
            }

            return Task.FromResult(1);
        }

        public bool WaitForFirstCall(TimeSpan timeout) => _firstCallStarted.Wait(timeout);

        public void ReleaseFirstCall() => _releaseFirstCall.Set();

        public void Dispose()
        {
            _releaseFirstCall.Set();
            _firstCallStarted.Dispose();
            _releaseFirstCall.Dispose();
        }
    }

    private static JobStateDto CloneState(JobStateDto state)
    {
        return new JobStateDto
        {
            JobId = state.JobId,
            JobName = state.JobName,
            Status = state.Status,
            CurrentSourceFile = state.CurrentSourceFile,
            CurrentTargetFile = state.CurrentTargetFile,
            TotalFiles = state.TotalFiles,
            FilesProcessed = state.FilesProcessed,
            TotalSizeBytes = state.TotalSizeBytes,
            SizeProcessedBytes = state.SizeProcessedBytes,
            ProgressPercentage = state.ProgressPercentage,
            RemainingFiles = state.RemainingFiles,
            RemainingSizeBytes = state.RemainingSizeBytes,
            LastActionTimestampUtc = state.LastActionTimestampUtc,
            ErrorMessage = state.ErrorMessage
        };
    }

    private static bool WaitForState(
        ConcurrentQueue<JobStateDto> states,
        JobStatus expectedStatus,
        TimeSpan timeout,
        int minimumOccurrences = 1)
    {
        var deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            if (states.Count(s => s.Status == expectedStatus) >= minimumOccurrences)
                return true;

            Thread.Sleep(20);
        }

        return states.Count(s => s.Status == expectedStatus) >= minimumOccurrences;
    }
}
