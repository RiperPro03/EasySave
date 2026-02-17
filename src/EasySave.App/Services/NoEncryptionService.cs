using EasySave.Core.Interfaces;

namespace EasySave.App.Services;

public class NoEncryptionService : ICryptoService
{
    public Task<int> EncryptFileAsync(string filePath, string key)
    {
        // Pas de chiffrement → retourne 0ms
        return Task.FromResult(0);
    }
}
