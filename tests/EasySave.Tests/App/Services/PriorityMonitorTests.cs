using EasySave.Core.Models;

namespace EasySave.Tests.App.Services;

public sealed class PriorityMonitorTests
{
    [Fact]
    public void IsPriorityWorkActive_ShouldBeFalse_ByDefault()
    {
        var monitor = new PriorityMonitor();

        Assert.False(monitor.IsPriorityWorkActive);
    }

    [Fact]
    public void EnterAndExitPriorityZone_ShouldTrackActiveState()
    {
        var monitor = new PriorityMonitor();

        monitor.EnterPriorityZone();
        Assert.True(monitor.IsPriorityWorkActive);

        monitor.ExitPriorityZone();
        Assert.False(monitor.IsPriorityWorkActive);
    }

    [Fact]
    public void ExitPriorityZone_ShouldNotGoBelowZero()
    {
        var monitor = new PriorityMonitor();

        monitor.ExitPriorityZone();
        monitor.ExitPriorityZone();

        Assert.False(monitor.IsPriorityWorkActive);
    }
}
