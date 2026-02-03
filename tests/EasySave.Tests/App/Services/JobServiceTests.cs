using EasySave.App.Repositories;
using EasySave.App.Services;
using EasySave.Core.Enums;
using EasySave.tests.Helpers.Assertions;

namespace EasySave.Tests.App.Services;

public class JobServiceTests
{
    [Fact]
    public void Create_ShouldAddJob()
    {
        var repository = new JobRepository();
        var service = new JobService(repository);

        service.Create("1", "Job1", @"C:\Src", @"D:\Dst", BackupType.Full);

        var job = service.GetById("1");
        Assert.NotNull(job);
        Assert.Equal("Job1", job!.Name);
    }

    [Fact]
    public void Create_ShouldThrow_WhenIdInvalid()
    {
        var repository = new JobRepository();
        var service = new JobService(repository);

        ExceptionAssert.ThrowsArgumentException(
            () => service.Create("   ", "Job1", @"C:\Src", @"D:\Dst", BackupType.Full),
            "id");
    }

    [Fact]
    public void Update_ShouldUpdateJob_WhenExists()
    {
        var repository = new JobRepository();
        var service = new JobService(repository);

        service.Create("1", "Job1", @"C:\Src", @"D:\Dst", BackupType.Full);

        service.Update("1", "Job1-Updated", @"C:\NewSrc", @"D:\NewDst", BackupType.Differential, isActive: false);

        var job = service.GetById("1");
        Assert.NotNull(job);
        Assert.Equal("Job1-Updated", job!.Name);
        Assert.Equal(@"C:\NewSrc", job.SourcePath);
        Assert.Equal(@"D:\NewDst", job.TargetPath);
        Assert.Equal(BackupType.Differential, job.Type);
        Assert.False(job.IsActive);
    }

    [Fact]
    public void Update_ShouldThrow_WhenMissing()
    {
        var repository = new JobRepository();
        var service = new JobService(repository);

        Assert.Throws<KeyNotFoundException>(
            () => service.Update("missing", "Job", @"C:\Src", @"D:\Dst", BackupType.Full, isActive: true));
    }

    [Fact]
    public void Delete_ShouldRemoveJob_WhenExists()
    {
        var repository = new JobRepository();
        var service = new JobService(repository);

        service.Create("1", "Job1", @"C:\Src", @"D:\Dst", BackupType.Full);

        service.Delete("1");

        Assert.Null(service.GetById("1"));
    }

    [Fact]
    public void GetAll_ShouldReturnAllJobs()
    {
        var repository = new JobRepository();
        var service = new JobService(repository);

        service.Create("1", "Job1", @"C:\Src", @"D:\Dst", BackupType.Full);
        service.Create("2", "Job2", @"C:\Src", @"D:\Dst", BackupType.Differential);

        var jobs = service.GetAll();

        Assert.Equal(2, jobs.Count);
    }
}
