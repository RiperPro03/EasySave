using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.App.Services
{
    public sealed class LargeFileTransferLimiter
    {
        private readonly SemaphoreSlim _largeFileSemaphore = new(1,1);

        /// <summary> 
        /// Acquires permission to transfer a file. 
        /// Large files wait for the semaphore; small files pass immediately. 
        /// </summary>
        public async Task<IDisposable> AcquireAsync(long filesizebytes, long thresholdbytes)
        {
            bool isLarge = filesizebytes >= thresholdbytes;
            if (isLarge)
                await _largeFileSemaphore.WaitAsync().ConfigureAwait(false);
            return new Releaser(_largeFileSemaphore, isLarge);
        }

        /// <summary>
        /// Releases the semaphore when the transfer is finished
        /// </summary>
        private sealed class Releaser : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;
            private readonly bool _isLarge;

            public Releaser(SemaphoreSlim semaphore, bool isLarge)
            {
                _semaphore = semaphore;
                _isLarge = isLarge;
            }
            public void Dispose()
            {
                if (_isLarge)
                    _semaphore.Release();
            }
        }
    }
}
