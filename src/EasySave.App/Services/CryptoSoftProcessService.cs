using System.Diagnostics;
using EasySave.Core.Interfaces;

namespace EasySave.App.Services;

public class CryptoSoftProcessService : ICryptoService
{
    private readonly string _exePath;
    private readonly string _semaphoreName;

    /// <summary>
    /// Initializes a service responsible for launching the CryptoSoft encryption process.
    /// </summary>
    /// <param name="exePath">Full path to the CryptoSoft executable</param>
    /// <param name="_semaphoreName">Name of the global semaphore used to limit concurrent executions</param>
    public CryptoSoftProcessService(string exePath, string _semaphoreName = "Global\\CryptoSoftSemaphore")
    {
        _exePath = exePath;
        _semaphoreName = _semaphoreName;
    }

    /// <summary>
    /// Encrypts a file by launching the external CryptoSoft process.
    /// </summary>
    /// <param name="filePath">Path of the file to encrypt</param>
    /// <param name="key">Encryption key passed to the CryptoSoft executable</param>
    /// <returns>Exit code of the process (encryption time in milliseconds)</returns>
    public async Task<int> EncryptFileAsync(string filePath, string key)
    {
        using (var semaphore = new ProcessSemaphoreLock(_semaphoreName))
        {
            var startInfo = new ProcessStartInfo(_exePath)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add(filePath);
            startInfo.ArgumentList.Add(key);

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            // Asynchronously read output streams to avoid blocking
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(outputTask, errorTask);
            await process.WaitForExitAsync();

            return process.ExitCode; // exitCode = encryption time in ms
        }
    }
}
