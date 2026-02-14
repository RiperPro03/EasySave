namespace EasySave.Core.Enums;

/// <summary>
/// Defines the backup strategy.
/// </summary>
public enum BackupType
{
    /// <summary>
    /// Copies all files.
    /// </summary>
    Full,
    /// <summary>
    /// Copies only files changed since the last backup.
    /// </summary>
    Differential
}
