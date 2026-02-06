using System;
using System.Collections.Generic;
using System.IO;
using EasySave.App.Repositories;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;

namespace EasySave.Tests.App.Repositories;

public class JobRepositoryTests : IDisposable
{
    private readonly List<string> _pathsToClean = new();

    [Fact]
    public void Add_ShouldAddJob_WhenUnderLimit()
    {
        var repository = CreateRepository();
        var job = new BackupJob("1", "Job1", @"C:\Src", @"D:\Dst", BackupType.Full);

        repository.Add(job);

        var allJobs = repository.GetAll();
        Assert.Single(allJobs);
        Assert.Equal("Job1", allJobs[0].Name);
    }

    [Fact]
    public void Add_ShouldThrow_WhenMaxJobsReached()
    {
        var repository = CreateRepository();
        for (int i = 0; i < 5; i++)
        {
            repository.Add(new BackupJob(
                i.ToString(),
                $"Job{i}",
                @"C:\Src",
                @"D:\Dst",
                BackupType.Full
            ));
        }
        var newJob = new BackupJob(
            "6",
            "Job6",
            @"C:\Src",
            @"D:\Dst",
            BackupType.Full
        );

        Assert.Throws<InvalidOperationException>(() => repository.Add(newJob));
    }

    [Fact]
    public void Remove_ShouldRemoveJob_WhenJobExists()
    {
        var repository = CreateRepository();
        var job = new BackupJob(
            "1",
            "Job1",
            @"C:\\Src",
            @"D:\\Dst",
            BackupType.Full
        );
        repository.Add(job);

        repository.Remove("1");

        var allJobs = repository.GetAll();
        Assert.Empty(allJobs);
    }

    [Fact]
    public void Remove_ShouldThrow_WhenJobNotFound()
    {
        var repository = CreateRepository();
        Assert.Throws<KeyNotFoundException>(() => repository.Remove("nonexistent-id"));
    }

    [Fact]
    public void Update_ShouldUpdateJob_WhenJobExists()
    {
        var repository = CreateRepository();
        var job = new BackupJob(
            "2",
            "Job2",
            @"C:\Src",
            @"D:\Dst",
            BackupType.Full,
            isActive: true
        );
        repository.Add(job);

        var updatedJob = new BackupJob(
            "2",
            "UpdatedJob2",
            @"C:\NewSrc",
            @"D:\NewDst",
            BackupType.Differential,
            isActive: false
        );
        repository.Update(updatedJob);
        var retrievedJob = repository.GetById("2");
        Assert.NotNull(retrievedJob);
        Assert.Equal("UpdatedJob2", retrievedJob!.Name);
        Assert.Equal(@"C:\NewSrc", retrievedJob.SourcePath);
        Assert.Equal(@"D:\NewDst", retrievedJob.TargetPath);
        Assert.Equal(BackupType.Differential, retrievedJob.Type);
        Assert.False(retrievedJob.IsActive);
    }

    [Fact]
    public void GetById_ShouldReturnCorrectJob()
    {
        var repository = CreateRepository();
        var job = new BackupJob("3", "Job3", @"C:\Src", @"D:\Dst", BackupType.Full);
        repository.Add(job);

        var retrievedJob = repository.GetById("3");
        Assert.NotNull(retrievedJob);
        Assert.Equal("Job3", retrievedJob!.Name);
    }

    [Fact]
    public void GetById_ShouldReturnNull_WhenMissing()
    {
        var repository = CreateRepository();

        var retrievedJob = repository.GetById("missing-id");

        Assert.Null(retrievedJob);
    }

    [Fact]
    public void GetAll_ShouldReturnAllJobs()
    {
        var repository = CreateRepository();
        repository.Add(new BackupJob("1", "Job1", @"C:\Src", @"D:\Dst", BackupType.Full));
        repository.Add(new BackupJob("2", "Job2", @"C:\Src", @"D:\Dst", BackupType.Differential));

        var allJobs = repository.GetAll();
        Assert.Equal(2, allJobs.Count);
        Assert.Contains(allJobs, j => j.Name == "Job1");
        Assert.Contains(allJobs, j => j.Name == "Job2");
    }

    public void Dispose()
    {
        foreach (var path in _pathsToClean)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }

    private JobRepository CreateRepository()
    {
        var basePath = Path.Combine(Path.GetTempPath(), "EasySave.Tests", Guid.NewGuid().ToString("N"));
        _pathsToClean.Add(basePath);
        var pathProvider = new TestPathProvider(basePath);
        return new JobRepository(pathProvider);
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
