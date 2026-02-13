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

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"CryptoSoft error: {error}");
        }

        return int.TryParse(output.Trim(), out int result)
            ? result
            : -1;
    }
}