using System.Diagnostics;
using EasySave.Core.Interfaces;

namespace EasySave.App.Services;

public class CryptoSoftProcessService : ICryptoService
{
    private readonly string _exePath;
    private readonly string _semaphoreName;

    public CryptoSoftProcessService(string exePath, string _semaphoreName = "Global\\CryptoSoftSemaphore")
    {
        _exePath = exePath;
        _semaphoreName = _semaphoreName;
    }

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

            // Lecture asynchrone des flux pour éviter blocage
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(outputTask, errorTask);
            await process.WaitForExitAsync();

            return process.ExitCode; // exitCode = temps d'encryption en ms
        }
    }
}