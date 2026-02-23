namespace EasySave.App.Services;

public sealed class ProcessMutexLock : IDisposable
{
    private readonly Mutex _mutex;
    private bool _hasHandle;
    
    public ProcessMutexLock(string mutexName)
    {
        _mutex = new Mutex(false, mutexName);
        try
        {
            // Attend jusqu'à 10 secondes pour obtenir le mutex (configurable)
            _hasHandle = _mutex.WaitOne(TimeSpan.FromSeconds(10), false);
            if (!_hasHandle)
            {
                throw new InvalidOperationException("Une autre instance de CryptoSoft est déjà en cours d'exécution.");
            }
        }
        catch (AbandonedMutexException)
        {
            _hasHandle = true; // Le mutex a été abandonné, on peut continuer
        }
    }

    public void Dispose()
    {
        if (_hasHandle)
        {
            _mutex.ReleaseMutex();
            _hasHandle = false;
        }

        _mutex.Dispose();
    }
}