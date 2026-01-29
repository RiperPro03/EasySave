using EasySave.Core.DTO;

namespace EasySave.Tests.Core.DTO;

public class LogEntryDtoTests
{
    [Fact]
    public void Defaults_ShouldBeInitialized()
    {
        var dto = new LogEntryDto();

        Assert.Equal(default, dto.TimestampUtc);
        Assert.Equal(string.Empty, dto.JobName);
        Assert.Equal(string.Empty, dto.SourcePath);
        Assert.Equal(string.Empty, dto.TargetPath);
        Assert.Equal(0, dto.FileSizeBytes);
        Assert.Equal(0d, dto.TransferTimeMs);
        Assert.Equal("OK", dto.Status);
        Assert.Null(dto.ErrorMessage);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var t = DateTime.UtcNow;

        var dto = new LogEntryDto
        {
            TimestampUtc = t,
            JobName = "Job",
            SourcePath = "C:\\Src\\a.txt",
            TargetPath = "D:\\Dst\\a.txt",
            FileSizeBytes = 123,
            TransferTimeMs = 42.5,
            Status = "ERROR",
            ErrorMessage = "boom"
        };

        Assert.Equal(t, dto.TimestampUtc);
        Assert.Equal("Job", dto.JobName);
        Assert.Equal("C:\\Src\\a.txt", dto.SourcePath);
        Assert.Equal("D:\\Dst\\a.txt", dto.TargetPath);
        Assert.Equal(123, dto.FileSizeBytes);
        Assert.Equal(42.5, dto.TransferTimeMs);
        Assert.Equal("ERROR", dto.Status);
        Assert.Equal("boom", dto.ErrorMessage);
    }
}