using EasySave.Core.Enums;

namespace EasySave.Core.DTO;

public sealed class AppStateDto
{
    public DateTime GeneratedAtUtc { get; set; }
    public int TotalJobs { get; set; }
    public JobStatus GlobalStatus { get; set; } = JobStatus.Idle;
    public List<string> ActiveJobIds { get; set; } = new();
    public List<JobStateDto> Jobs { get; set; } = new();
}
