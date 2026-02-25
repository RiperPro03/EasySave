using EasySave.App.Services;

namespace EasySave.Tests.App.Services;

public sealed class LargeFileTransferLimiterTests
{
    [Fact]
    public async Task AcquireAsync_ShouldNotBlockSmallFiles()
    {
        var limiter = new LargeFileTransferLimiter();

        using var lease = await limiter.AcquireAsync(filesizebytes: 100, thresholdbytes: 1_000);

        Assert.NotNull(lease);
    }

    [Fact]
    public async Task AcquireAsync_ShouldSerializeLargeFiles()
    {
        var limiter = new LargeFileTransferLimiter();
        var firstLease = await limiter.AcquireAsync(filesizebytes: 10_000, thresholdbytes: 1_000);

        var secondAcquired = false;
        var secondTask = Task.Run(async () =>
        {
            using var secondLease = await limiter.AcquireAsync(filesizebytes: 20_000, thresholdbytes: 1_000);
            secondAcquired = true;
        });

        await Task.Delay(150);
        Assert.False(secondAcquired);

        try
        {
            firstLease.Dispose();

            await secondTask.WaitAsync(TimeSpan.FromSeconds(2));
            Assert.True(secondAcquired);
        }
        finally
        {
            // Safety if the assertion above fails before task completion.
            await secondTask;
        }
    }
}
