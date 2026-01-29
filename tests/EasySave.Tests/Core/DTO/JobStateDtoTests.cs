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
        Assert.Null(dto.CurrentFile);

        Assert.Equal(0, dto.TotalFilesToCopy);
        Assert.Equal(0, dto.TotalFilesCopied);

        Assert.Equal(0, dto.TotalSizeBytes);
        Assert.Equal(0, dto.SizeCopiedBytes);

        Assert.Equal(0, dto.ProgressPercentage);
        Assert.Equal(default, dto.StartTimeUtc);

        Assert.Null(dto.EndTimeUtc);
        Assert.Null(dto.ErrorMessage);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var start = DateTime.UtcNow;
        var end = start.AddSeconds(3);

        var dto = new JobStateDto
        {
            JobId = "1",
            JobName = "Job1",
            Status = JobStatus.Running,
            CurrentFile = "C:\\a.txt",
            TotalFilesToCopy = 10,
            TotalFilesCopied = 3,
            TotalSizeBytes = 1000,
            SizeCopiedBytes = 300,
            ProgressPercentage = 30,
            StartTimeUtc = start,
            EndTimeUtc = end,
            ErrorMessage = "none"
        };

        Assert.Equal("1", dto.JobId);
        Assert.Equal("Job1", dto.JobName);
        Assert.Equal(JobStatus.Running, dto.Status);
        Assert.Equal("C:\\a.txt", dto.CurrentFile);
        Assert.Equal(10, dto.TotalFilesToCopy);
        Assert.Equal(3, dto.TotalFilesCopied);
        Assert.Equal(1000, dto.TotalSizeBytes);
        Assert.Equal(300, dto.SizeCopiedBytes);
        Assert.Equal(30, dto.ProgressPercentage);
        Assert.Equal(start, dto.StartTimeUtc);
        Assert.Equal(end, dto.EndTimeUtc);
        Assert.Equal("none", dto.ErrorMessage);
    }
}