namespace EasySave.Core.Enums;

/// <summary>
/// Represents the status of a backup job.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// The job exists but has not started yet.
    /// </summary>
    Idle,
    /// <summary>
    /// Backup is currently running.
    /// </summary>
    Running,
    /// <summary>
    /// Backup is paused manually or automatically.
    /// </summary>
    Paused,
    /// <summary>
    /// Backup completed successfully.
    /// </summary>
    Completed,
    /// <summary>
    /// A blocking error occurred.
    /// </summary>
    Error
}
