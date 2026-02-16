using System.Diagnostics;
using EasySave.Core.Interfaces;

namespace EasySave.App.Services;

public class CryptoSoftProcessService : ICryptoService
{
    private readonly string _exePath;

    public CryptoSoftProcessService(string exePath)
    {
        _exePath = exePath;
    }

    public async Task<int> EncryptFileAsync(string filePath, string key)
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

        _ = await process.StandardOutput.ReadToEndAsync();
        _ = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        // CryptoSoft returns the encryption time in ms as the process exit code.
        var exitCode = process.ExitCode;
        return exitCode;
    }
}