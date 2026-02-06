using EasySave.App.Services;


namespace EasySave.Tests.App.Services;

public class BackupServiceTests
{
    [Fact]
    public void FullBackup_ShouldCopyFiles()
    {
        // Arrange
        var source = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var target = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "test.txt"), "hello");

        var service = new BackupService();

        // Act
        service.FullBackup(source, target);

        // Assert
        Assert.True(File.Exists(Path.Combine(target, "test.txt")));

        // Cleanup
        Directory.Delete(source, true);
        Directory.Delete(target, true);
    }
}