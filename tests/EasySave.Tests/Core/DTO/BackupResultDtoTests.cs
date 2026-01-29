using EasySave.Core.DTO;

namespace EasySave.Tests.Core.DTO;

public class BackupResultDtoTests
{
    [Fact]
    public void Defaults_ShouldBeInitialized()
    {
        var dto = new BackupResultDto();

        Assert.False(dto.Success);
        Assert.Equal(string.Empty, dto.Message);
        Assert.Equal(0, dto.FilesProcessed);
        Assert.Equal(0, dto.TotalBytesProcessed);
        Assert.Equal(default, dto.Duration);
        Assert.NotNull(dto.Errors);
        Assert.Empty(dto.Errors);
    }

    [Fact]
    public void Errors_ShouldBeMutableList()
    {
        var dto = new BackupResultDto();
        dto.Errors.Add("err1");
        dto.Errors.Add("err2");

        Assert.Equal(2, dto.Errors.Count);
        Assert.Contains("err1", dto.Errors);
        Assert.Contains("err2", dto.Errors);
    }
}