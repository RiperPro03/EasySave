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

    /// <summary>
    /// Checks whether the configured business software is running.
    /// </summary>
    /// <param name="processName">The configured process name, if any.</param>
    /// <returns><c>true</c> when the process is running.</returns>
    bool IsBusinessSoftwareRunning(out string? processName);

    /// <summary>
    /// Validates whether a batch/sequence can start (global business software rule).
    /// Logs the block when it cannot start.
    /// </summary>
    /// <param name="reason">Reason when the sequence cannot start.</param>
    /// <returns><c>true</c> if the sequence can start.</returns>
    bool CanStartSequence(out string? reason);
}
