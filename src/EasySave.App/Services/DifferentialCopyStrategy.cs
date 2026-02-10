using System.Security.Cryptography;
using EasySave.Core.Interfaces;

namespace EasySave.App.Services;

/// <summary>
/// Copies files only when content differs from the target.
/// </summary>
internal sealed class DifferentialCopyStrategy : IBackupCopyStrategy
{
    /// <summary>
    /// Determines whether a source file should be copied to the target.
    /// </summary>
    /// <param name="sourcePath">Source file path.</param>
    /// <param name="targetPath">Target file path.</param>
    /// <returns><c>true</c> if the file should be copied; otherwise <c>false</c>.</returns>
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

    /// <summary>
    /// Computes a SHA-256 hash for a file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The hash bytes.</returns>
    private static byte[] ComputeHash(string path)
    {
        using var stream = File.OpenRead(path);
        using var sha = SHA256.Create();
        return sha.ComputeHash(stream);
    }
}
