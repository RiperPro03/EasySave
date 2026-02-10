using System.Security.Cryptography;
using EasySave.Core.Interfaces;

namespace EasySave.App.Services;

/// <summary>
/// Cette stratégie permet de ne copier que les fichiers modifiés depuis la dernière sauvegarde
/// </summary>
internal sealed class DifferentialCopyStrategy : IBackupCopyStrategy
{
    public bool ShouldCopy(string sourcePath, string targetPath)
    {
        if (!File.Exists(targetPath))
            return true;

        var sourceInfo = new FileInfo(sourcePath);
        var targetInfo = new FileInfo(targetPath);

        if (sourceInfo.Length != targetInfo.Length)
            return true;

        if (sourceInfo.LastWriteTimeUtc == targetInfo.LastWriteTimeUtc)
            return false;

        var sourceHash = ComputeHash(sourcePath);
        var targetHash = ComputeHash(targetPath);
        return !CryptographicOperations.FixedTimeEquals(sourceHash, targetHash);
    }

    private static byte[] ComputeHash(string path)
    {
        using var stream = File.OpenRead(path);
        using var sha = SHA256.Create();
        return sha.ComputeHash(stream);
    }
}
