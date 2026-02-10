namespace EasySave.Core.Enums;

/// <summary>
/// Represents the status of a backup job.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// The job exists but has not started yet.
    /// </summary>
    Idle = 0,
    /// <summary>
    /// Backup is currently running.
    /// </summary>
    Running = 1,
    /// <summary>
    /// Backup is paused manually or automatically.
    /// </summary>
    Paused = 2,
    /// <summary>
    /// Backup completed successfully.
    /// </summary>
    Completed = 3,
    /// <summary>
    /// A blocking error occurred.
    /// </summary>
    Error = 4
}
