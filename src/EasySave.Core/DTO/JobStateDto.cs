using EasySave.Core.Enums;

namespace EasySave.Core.DTO;

/// <summary>
/// Represents the real-time state of a job (state.json).
/// </summary>
public class JobStateDto
{
    public string JobId { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;

    public JobStatus Status { get; set; } = JobStatus.Idle;

    public string? CurrentSourceFile { get; set; }
    public string? CurrentTargetFile { get; set; }

    public long TotalFiles { get; set; }
    public long FilesProcessed { get; set; }

    public long TotalSizeBytes { get; set; }
    public long SizeProcessedBytes { get; set; }

    /// <summary>
    /// Gets or sets the progress percentage computed by the engine/service.
    /// </summary>
    public int ProgressPercentage { get; set; }

    public long RemainingFiles { get; set; }
    public long RemainingSizeBytes { get; set; }

    public DateTime LastActionTimestampUtc { get; set; }

    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JobStateDto"/> class.
    /// </summary>
    /// <remarks>Required by some XML serializers.</remarks>
    public JobStateDto() { }
}
