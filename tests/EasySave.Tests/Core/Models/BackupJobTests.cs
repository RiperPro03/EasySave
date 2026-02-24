using EasySave.Core.Enums;
using EasySave.Core.Models;
using EasySave.tests.Helpers.Assertions;
using EasySave.tests.Helpers.Builders;

namespace EasySave.Tests.Core.Models;

public class BackupJobTests
{
    [Fact]
    public void Ctor_ShouldSetProperties_ForValidInput()
    {
        var job = BackupJobBuilder.Valid()
            .WithId("job-123")
            .WithName("JobName")
            .WithSource(@"C:\Src")
            .WithTarget(@"C:\Dst")
            .WithType(BackupType.Differential)
            .Build();

        Assert.Equal("job-123", job.Id);
        Assert.Equal("JobName", job.Name);
        Assert.Equal(@"C:\Src", job.SourcePath);
        Assert.Equal(@"C:\Dst", job.TargetPath);
        Assert.Equal(BackupType.Differential, job.Type);

        Assert.True(job.IsActive);
        Assert.True(job.CreatedAt != default);
        Assert.Null(job.LastRun);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_ShouldThrow_IfIdInvalid(string? id)
    {
        ExceptionAssert.ThrowsArgumentException(
            () => BackupJobBuilder.Valid().WithId(id!).Build(),
            "id");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_ShouldThrow_IfNameInvalid(string? name)
    {
        ExceptionAssert.ThrowsArgumentException(
            () => BackupJobBuilder.Valid().WithName(name!).Build(),
            "name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_ShouldThrow_IfSourceInvalid(string? source)
    {
        ExceptionAssert.ThrowsArgumentException(
            () => BackupJobBuilder.Valid().WithSource(source!).Build(),
            "sourcePath");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_ShouldThrow_IfTargetInvalid(string? target)
    {
        ExceptionAssert.ThrowsArgumentException(
            () => BackupJobBuilder.Valid().WithTarget(target!).Build(),
            "targetPath");
    }

    [Fact]
    public void Disable_ShouldSetIsActiveFalse()
    {
        var job = BackupJobBuilder.Valid().Build();

        job.Disable();

        Assert.False(job.IsActive);
    }

    [Fact]
    public void Enable_ShouldSetIsActiveTrue()
    {
        var job = BackupJobBuilder.Valid().Build();
        job.Disable();

        job.Enable();

        Assert.True(job.IsActive);
    }

    [Fact]
    public void MarkExecuted_ShouldSetLastRun()
    {
        var job = BackupJobBuilder.Valid().Build();

        job.MarkExecuted();

        Assert.NotNull(job.LastRun);
    }

    [Fact]
    public void UpdateDefinition_ShouldUpdateCoreFields()
    {
        var job = BackupJobBuilder.Valid().Build();

        job.UpdateDefinition("NewName", @"D:\Src2", @"D:\Dst2", BackupType.Differential, new List<string>());

        Assert.Equal("NewName", job.Name);
        Assert.Equal(@"D:\Src2", job.SourcePath);
        Assert.Equal(@"D:\Dst2", job.TargetPath);
        Assert.Equal(BackupType.Differential, job.Type);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDefinition_ShouldThrow_IfNameInvalid(string? name)
    {
        var job = BackupJobBuilder.Valid().Build();

        ExceptionAssert.ThrowsArgumentException(
            () => job.UpdateDefinition(name!, @"C:\Src", @"C:\Dst", BackupType.Full, new List<string>()),
            "name");
    }

    [Fact]
    public void ToString_ShouldReturnExpectedFormat()
    {
        var job = new BackupJob("1", "Backup1", "C:\\Src", "D:\\Dst", BackupType.Full, isActive: true);

        Assert.Equal("Backup1 (Full) | C:\\Src -> D:\\Dst | Active=True", job.ToString());
    }
}