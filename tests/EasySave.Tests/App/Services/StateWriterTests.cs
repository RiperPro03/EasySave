using System.Text.Json;
using EasySave.App.Services;
using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;

namespace EasySave.Tests.App.Services;

public class StateWriterTests : IDisposable
{
    private readonly string _basePath;
    private readonly TestPathProvider _pathProvider;

    public StateWriterTests()
    {
        _basePath = Path.Combine(Path.GetTempPath(), "EasySave.Tests", Guid.NewGuid().ToString("N"));
        _pathProvider = new TestPathProvider(_basePath);
    }

    [Fact]
    public void Write_ShouldCreateReadableJson()
    {
        var writer = new StateWriter(_pathProvider);
        var jobState = new JobStateDto
        {
            JobId = "1",
            JobName = "Job",
            Status = JobStatus.Running,
            CurrentSourceFile = "C:\\source\\file.txt",
            CurrentTargetFile = "D:\\target\\file.txt",
            TotalFiles = 2,
            FilesProcessed = 1,
            TotalSizeBytes = 100,
            SizeProcessedBytes = 50,
            ProgressPercentage = 50,
            RemainingFiles = 1,
            RemainingSizeBytes = 50,
            LastActionTimestampUtc = DateTime.UtcNow
        };

        var snapshot = new AppStateDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            TotalJobs = 1,
            GlobalStatus = JobStatus.Running,
            ActiveJobIds = new List<string> { "1" },
            Jobs = new List<JobStateDto> { jobState }
        };

        writer.Write(snapshot);

        var path = Path.Combine(_pathProvider.StatePath, "state.json");
        Assert.True(File.Exists(path));

        var json = File.ReadAllText(path);
        Assert.Contains(Environment.NewLine, json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("GeneratedAtUtc", out _));
        Assert.Equal(1, root.GetProperty("TotalJobs").GetInt32());
        Assert.Equal("Running", root.GetProperty("GlobalStatus").GetString());
        Assert.Equal("1", root.GetProperty("ActiveJobIds")[0].GetString());

        var job = root.GetProperty("Jobs")[0];
        Assert.Equal("1", job.GetProperty("JobId").GetString());
        Assert.Equal("Job", job.GetProperty("JobName").GetString());
        Assert.Equal("Running", job.GetProperty("Status").GetString());
        Assert.Equal("C:\\source\\file.txt", job.GetProperty("CurrentSourceFile").GetString());
        Assert.Equal("D:\\target\\file.txt", job.GetProperty("CurrentTargetFile").GetString());
        Assert.Equal(2, job.GetProperty("TotalFiles").GetInt64());
        Assert.Equal(1, job.GetProperty("FilesProcessed").GetInt64());
        Assert.Equal(100, job.GetProperty("TotalSizeBytes").GetInt64());
        Assert.Equal(50, job.GetProperty("SizeProcessedBytes").GetInt64());
        Assert.Equal(50, job.GetProperty("ProgressPercentage").GetInt32());
        Assert.Equal(1, job.GetProperty("RemainingFiles").GetInt64());
        Assert.Equal(50, job.GetProperty("RemainingSizeBytes").GetInt64());
        Assert.True(job.TryGetProperty("LastActionTimestampUtc", out _));
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
            Directory.Delete(_basePath, true);
    }

    private sealed class TestPathProvider : IPathProvider
    {
        private readonly string _basePath;

        public TestPathProvider(string basePath)
        {
            _basePath = basePath;
        }

        public string LogsPath => Path.Combine(_basePath, "Logs");
        public string StatePath => Path.Combine(_basePath, "State");
        public string ConfigPath => Path.Combine(_basePath, "Config");

        public void EnsureDirectoriesCreated()
        {
            Directory.CreateDirectory(LogsPath);
            Directory.CreateDirectory(StatePath);
            Directory.CreateDirectory(ConfigPath);
        }
    }
}
