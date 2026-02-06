using EasySave.Core.DTO;
using EasySave.Core.Enums;

namespace EasySave.Tests.Core.DTO;

public class JobStateDtoTests
{
    [Fact]
    public void Defaults_ShouldBeInitialized()
    {
        var dto = new JobStateDto();

        Assert.Equal(string.Empty, dto.JobId);
        Assert.Equal(string.Empty, dto.JobName);
        Assert.Equal(JobStatus.Idle, dto.Status);
        Assert.Null(dto.CurrentSourceFile);
        Assert.Null(dto.CurrentTargetFile);

        Assert.Equal(0, dto.TotalFiles);
        Assert.Equal(0, dto.FilesProcessed);

        Assert.Equal(0, dto.TotalSizeBytes);
        Assert.Equal(0, dto.SizeProcessedBytes);

        Assert.Equal(0, dto.ProgressPercentage);
        Assert.Equal(0, dto.RemainingFiles);
        Assert.Equal(0, dto.RemainingSizeBytes);
        Assert.Equal(default, dto.LastActionTimestampUtc);
        Assert.Null(dto.ErrorMessage);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var lastAction = DateTime.UtcNow;

        var dto = new JobStateDto
        {
            JobId = "1",
            JobName = "Job1",
            Status = JobStatus.Running,
            CurrentSourceFile = "C:\\a.txt",
            CurrentTargetFile = "D:\\a.txt",
            TotalFiles = 10,
            FilesProcessed = 3,
            TotalSizeBytes = 1000,
            SizeProcessedBytes = 300,
            ProgressPercentage = 30,
            RemainingFiles = 7,
            RemainingSizeBytes = 700,
            LastActionTimestampUtc = lastAction,
            ErrorMessage = "none"
        };

        Assert.Equal("1", dto.JobId);
        Assert.Equal("Job1", dto.JobName);
        Assert.Equal(JobStatus.Running, dto.Status);
        Assert.Equal("C:\\a.txt", dto.CurrentSourceFile);
        Assert.Equal("D:\\a.txt", dto.CurrentTargetFile);
        Assert.Equal(10, dto.TotalFiles);
        Assert.Equal(3, dto.FilesProcessed);
        Assert.Equal(1000, dto.TotalSizeBytes);
        Assert.Equal(300, dto.SizeProcessedBytes);
        Assert.Equal(30, dto.ProgressPercentage);
        Assert.Equal(7, dto.RemainingFiles);
        Assert.Equal(700, dto.RemainingSizeBytes);
        Assert.Equal(lastAction, dto.LastActionTimestampUtc);
        Assert.Equal("none", dto.ErrorMessage);
    }
}
