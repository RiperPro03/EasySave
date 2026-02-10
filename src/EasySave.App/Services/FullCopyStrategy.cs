using EasySave.Core.Interfaces;

namespace EasySave.App.Services;

/// <summary>
/// Always copies files regardless of target state.
/// </summary>
internal sealed class FullCopyStrategy : IBackupCopyStrategy
{
    /// <summary>
    /// Always returns <c>true</c> to force a copy.
    /// </summary>
    /// <param name="sourcePath">Source file path.</param>
    /// <param name="targetPath">Target file path.</param>
    /// <returns><c>true</c>.</returns>
    public bool ShouldCopy(string sourcePath, string targetPath)
    {
        return true;
    }
}
