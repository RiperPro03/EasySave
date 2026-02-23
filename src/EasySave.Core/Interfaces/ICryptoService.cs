namespace EasySave.Core.Interfaces;

public interface ICryptoService
{
    Task<int> EncryptFileAsync(string filePath, string key);
}




