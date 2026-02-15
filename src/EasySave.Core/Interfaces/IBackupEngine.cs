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

    /// <summary>
    /// Requests a pause for a running job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns><c>true</c> if the pause was requested; otherwise <c>false</c>.</returns>
    bool Pause(string jobId);

    /// <summary>
    /// Requests a resume for a paused job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns><c>true</c> if the resume was requested; otherwise <c>false</c>.</returns>
    bool Resume(string jobId);

    /// <summary>
    /// Requests a stop for a running job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns><c>true</c> if the stop was requested; otherwise <c>false</c>.</returns>
    bool Stop(string jobId);
}
