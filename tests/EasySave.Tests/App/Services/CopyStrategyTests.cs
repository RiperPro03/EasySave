using EasySave.App.Services;

namespace EasySave.Tests.App.Services;

public class CopyStrategyTests : IDisposable
{
    private readonly string _basePath;

    public CopyStrategyTests()
    {
        _basePath = Path.Combine(Path.GetTempPath(), "EasySave.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_basePath);
    }

    [Fact]
    public void FullCopyStrategy_ShouldAlwaysCopy()
    {
        var strategy = new FullCopyStrategy();

        var result = strategy.ShouldCopy("source.txt", "target.txt");

        Assert.True(result);
    }

    [Fact]
    public void DifferentialCopyStrategy_ShouldCopy_WhenTargetMissing()
    {
        var source = Path.Combine(_basePath, "source.txt");
        File.WriteAllText(source, "content");

        var target = Path.Combine(_basePath, "missing.txt");
        var strategy = new DifferentialCopyStrategy();

        Assert.True(strategy.ShouldCopy(source, target));
    }

    [Fact]
    public void DifferentialCopyStrategy_ShouldSkip_WhenSameSizeAndTimestamp()
    {
        var source = Path.Combine(_basePath, "source.txt");
        var target = Path.Combine(_basePath, "target.txt");
        File.WriteAllText(source, "content");
        File.Copy(source, target, true);

        var time = DateTime.UtcNow;
        File.SetLastWriteTimeUtc(source, time);
        File.SetLastWriteTimeUtc(target, time);

        var strategy = new DifferentialCopyStrategy();

        Assert.False(strategy.ShouldCopy(source, target));
    }

    [Fact]
    public void DifferentialCopyStrategy_ShouldCopy_WhenSizeDiffers()
    {
        var source = Path.Combine(_basePath, "source.txt");
        var target = Path.Combine(_basePath, "target.txt");
        File.WriteAllText(source, "content");
        File.WriteAllText(target, "different");

        var strategy = new DifferentialCopyStrategy();

        Assert.True(strategy.ShouldCopy(source, target));
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
            Directory.Delete(_basePath, true);
    }
}
