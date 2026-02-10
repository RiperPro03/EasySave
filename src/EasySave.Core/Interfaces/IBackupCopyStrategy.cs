namespace EasySave.Core.Interfaces;

/// <summary>
/// Defines the strategy used to decide whether a file should be copied.
/// </summary>
public interface IBackupCopyStrategy
{
    /// <summary>
    /// Determines whether the source file should be copied to the target.
    /// </summary>
    /// <param name="sourcePath">The source file path.</param>
    /// <param name="targetPath">The target file path.</param>
    /// <returns><c>true</c> if the copy should occur; otherwise <c>false</c>.</returns>
    bool ShouldCopy(string sourcePath, string targetPath);
}
