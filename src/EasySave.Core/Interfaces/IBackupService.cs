using System;
using EasySave.Core.DTO;
using EasySave.Core.Events;
using EasySave.Core.Models;

namespace EasySave.Core.Interfaces;

/// <summary>
/// Application service that coordinates backup execution.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Raised when job state changes during execution.
    /// </summary>
    event EventHandler<JobStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Runs a backup job.
    /// </summary>
    /// <param name="job">The job to run.</param>
    /// <returns>The execution result.</returns>
    BackupResultDto Run(BackupJob job);
}
