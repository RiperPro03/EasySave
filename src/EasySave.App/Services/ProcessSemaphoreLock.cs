using System;
using System.Threading;

namespace EasySave.App.Services;

public sealed class ProcessSemaphoreLock : IDisposable
{
    private readonly Semaphore _semaphore;
    private bool _hasHandle;

    /// <summary>
    /// Crée un verrou global pour limiter le nombre de processus simultanés.
    /// </summary>
    /// <param name="semaphoreName">Nom unique du sémaphore système</param>
    /// <param name="timeoutMs">Timeout en millisecondes pour obtenir le verrou</param>
    /// <param name="maxCount">Nombre maximum de processus simultanés</param>
    public ProcessSemaphoreLock(string semaphoreName, int timeoutMs = 10000, int maxCount = 1)
    {
        // Crée ou ouvre un sémaphore nommé global
        bool createdNew;
        _semaphore = new Semaphore(maxCount, maxCount, semaphoreName, out createdNew);

        try
        {
            _hasHandle = _semaphore.WaitOne(timeoutMs);
            if (!_hasHandle)
            {
                throw new InvalidOperationException("Une autre instance de CryptoSoft est déjà en cours d'exécution.");
            }
        }
        catch (AbandonedMutexException)
        {
            // Même si le précédent processus est mort, on peut continuer
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