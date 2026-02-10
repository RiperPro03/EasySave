using EasySave.Core.Enums;

namespace EasySave.Core.DTO;

/// <summary>
/// Represents a snapshot of the application state for persistence or transport.
/// </summary>
public sealed class AppStateDto
{
    /// <summary>
    /// Gets or sets the UTC time when this snapshot was generated.
    /// </summary>
    public DateTime GeneratedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the total number of known jobs.
    /// </summary>
    public int TotalJobs { get; set; }

    /// <summary>
    /// Gets or sets the aggregate status across all jobs.
    /// </summary>
    public JobStatus GlobalStatus { get; set; } = JobStatus.Idle;

    /// <summary>
    /// Gets or sets the identifiers of currently active jobs.
    /// </summary>
    public List<string> ActiveJobIds { get; set; } = new();

    /// <summary>
    /// Gets or sets per-job state snapshots.
    /// </summary>
    public List<JobStateDto> Jobs { get; set; } = new();
}
