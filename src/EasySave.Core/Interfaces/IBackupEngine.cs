using System;
using EasySave.Core.DTO;
using EasySave.Core.Events;
using EasySave.Core.Models;

namespace EasySave.Core.Interfaces;

/// <summary>
/// Domain engine responsible for executing a backup job.
/// </summary>
public interface IBackupEngine
{
    /// <summary>
    /// Raised when job state changes during execution.
    /// </summary>
    event EventHandler<JobStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Executes a backup job.
    /// </summary>
    /// <param name="job">The job to run.</param>
    /// <returns>The execution result.</returns>
    BackupResultDto Run(BackupJob job);
}
