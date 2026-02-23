using System;
using System.Threading;

namespace EasySave.App.Services;

public sealed class ProcessSemaphoreLock : IDisposable
{
    private readonly Semaphore _semaphore;
    private bool _hasHandle;

    /// <summary>
    /// Creates a global lock to limit the number of simultaneous processes.
    /// </summary>
    /// <param name="semaphoreName">Unique name of the system semaphore</param>
    /// <param name="timeoutMs">Timeout in milliseconds to acquire the lock</param>
    /// <param name="maxCount">Maximum number of simultaneous processes</param>
    public ProcessSemaphoreLock(string semaphoreName, int timeoutMs = 10000, int maxCount = 1)
    {
        // Creates or opens a global named semaphore
        bool createdNew;
        _semaphore = new Semaphore(maxCount, maxCount, semaphoreName, out createdNew);

        try
        {
            _hasHandle = _semaphore.WaitOne(timeoutMs);
            if (!_hasHandle)
            {
                throw new InvalidOperationException("Another instance of CryptoSoft is already running.");
            }
        }
        catch (AbandonedMutexException)
        {
            // Even if the previous process died, we can continue
            _hasHandle = true;
        }
    }

    public void Dispose()
    {
        if (_hasHandle)
        {
            _semaphore.Release();
            _hasHandle = false;
        }

        _semaphore.Dispose();
    }
}