using EasySave.Core.DTO;
using EasySave.Core.Enums;
using EasySave.Core.Models;

namespace EasySave.Tests.Core.DTO;

public class BackupJobDtoTests
{
    [Theory]
    [InlineData(null, "Job1", "C:\\Src", "D:\\Dst", "Full", false)]
    [InlineData("", "Job1", "C:\\Src", "D:\\Dst", "Full", false)]
    [InlineData("1", null, "C:\\Src", "D:\\Dst", "Full", false)]
    [InlineData("1", "   ", "C:\\Src", "D:\\Dst", "Full", false)]
    [InlineData("1", "Job1", null, "D:\\Dst", "Full", false)]
    [InlineData("1", "Job1", "C:\\Src", null, "Full", false)]
    [InlineData("1", "Job1", "C:\\Src", "D:\\Dst", null, false)]
    [InlineData("1", "Job1", "C:\\Src", "D:\\Dst", "", false)]
    [InlineData("1", "Job1", "C:\\Src", "D:\\Dst", "NotAType", false)]
    [InlineData("1", "Job1", "C:\\Src", "D:\\Dst", "Full", true)]
    public void IsValid_ShouldMatchExpected(
        string? id,
        string? name,
        string? src,
        string? dst,
        string? type,
        bool expected)
    {
        var dto = new BackupJobDto
        {
            Id = id,
            Name = name,
            SourcePath = src,
            TargetPath = dst,
            Type = type,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastRun = null
        };

        Assert.Equal(expected, dto.IsValid());
    }

    [Fact]
    public void ToModel_ShouldThrow_WhenDtoInvalid()
    {
        var dto = new BackupJobDto
        {
            Id = null,
            Name = "Job1",
            SourcePath = "C:\\Src",
            TargetPath = "D:\\Dst",
            Type = "Full",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        Assert.Throws<ArgumentException>(() => dto.ToModel());
    }

    [Fact]
    public void ToModel_ShouldCreateBackupJob_WhenDtoValid()
    {
        var createdAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var lastRun = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc);

        var dto = new BackupJobDto
        {
            Id = "job-1",
            Name = "MyJob",
            SourcePath = "C:\\Src",
            TargetPath = "D:\\Dst",
            Type = "Full",
            IsActive = false,
            CreatedAt = createdAt,
            LastRun = lastRun
        };

        var model = dto.ToModel();

        Assert.Equal("job-1", model.Id);
        Assert.Equal("MyJob", model.Name);
        Assert.Equal("C:\\Src", model.SourcePath);
        Assert.Equal("D:\\Dst", model.TargetPath);
        Assert.Equal(BackupType.Full, model.Type);
        Assert.False(model.IsActive);

        // le modèle force en UTC via ToUniversalTime()
        Assert.Equal(createdAt.ToUniversalTime(), model.CreatedAt);
        Assert.Equal(lastRun.ToUniversalTime(), model.LastRun);
    }

    [Fact]
    public void FromModel_ShouldMapAllProperties()
    {
        var createdAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var lastRun = new DateTime(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc);

        var job = new BackupJob(
            id: "id-123",
            name: "JobName",
            sourcePath: "C:\\A",
            targetPath: "D:\\B",
            type: BackupType.Differential,
            isActive: true,
            createdAtUtc: createdAt,
            lastRunUtc: lastRun
        );

        var dto = BackupJobDto.FromModel(job);

        Assert.Equal("id-123", dto.Id);
        Assert.Equal("JobName", dto.Name);
        Assert.Equal("C:\\A", dto.SourcePath);
        Assert.Equal("D:\\B", dto.TargetPath);
        Assert.Equal("Differential", dto.Type);
        Assert.True(dto.IsActive);
        Assert.Equal(createdAt, dto.CreatedAt);
        Assert.Equal(lastRun, dto.LastRun);
    }
}