using EasySave.App.Services;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
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
        var processName = Process.GetCurrentProcess().ProcessName;
        config.ChangeBussinessSoftware(processName);
        var engine = new BackupEngine(config);
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
        var job = new BackupJob("4", "Crypto job", source, target, BackupType.Full);

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
        var job = new BackupJob("5", "Crypto fail job", source, target, BackupType.Full);

        var result = engine.Run(job);

        Assert.False(result.Success);
        Assert.True(result.ErrorCount > 0);
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
}

