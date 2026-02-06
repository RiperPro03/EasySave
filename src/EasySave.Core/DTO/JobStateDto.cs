using EasySave.Core.Enums;

namespace EasySave.Core.DTO;

/// <summary>
/// Représente l'état temps réel d'un job (state.json).
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
    /// Pourcentage déjà calculé côté moteur (ou service)
    /// </summary>
    public int ProgressPercentage { get; set; }

    public long RemainingFiles { get; set; }
    public long RemainingSizeBytes { get; set; }

    public DateTime LastActionTimestampUtc { get; set; }

    public string? ErrorMessage { get; set; }

    // Constructeur vide requis pour certains sérialiseurs (XML)
    public JobStateDto() { }
}
