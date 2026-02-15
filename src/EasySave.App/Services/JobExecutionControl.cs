using System.Threading;
using EasySave.Core.DTO;

namespace EasySave.App.Services;

/// <summary>
/// Encapsulates synchronization primitives for a running job.
/// </summary>
internal sealed class JobExecutionControl : IDisposable
{
    private readonly ManualResetEventSlim _pauseGate = new(true);
    private readonly CancellationTokenSource _cancellation = new();
    private readonly object _sync = new();

    internal JobExecutionControl(JobStateDto state)
    {
        State = state;
    }

    internal JobStateDto State { get; }
    internal object Sync => _sync;
    internal bool IsPaused => !_pauseGate.IsSet;
    internal bool IsStopRequested => _cancellation.IsCancellationRequested;

    internal void RequestPause()
    {
        _pauseGate.Reset();
    }

    internal void RequestResume()
    {
        _pauseGate.Set();
    }

    internal void RequestStop()
    {
        _cancellation.Cancel();
        _pauseGate.Set();
    }

    internal void WaitWhilePaused()
    {
        if (_pauseGate.IsSet)
            return;

        _pauseGate.Wait(_cancellation.Token);
    }

    public void Dispose()
    {
        _pauseGate.Dispose();
        _cancellation.Dispose();
    }
}
