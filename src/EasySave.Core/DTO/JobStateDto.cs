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

    public string? CurrentFile { get; set; }

    public long TotalFilesToCopy { get; set; }
    public long TotalFilesCopied { get; set; }

    public long TotalSizeBytes { get; set; }
    public long SizeCopiedBytes { get; set; }

    /// <summary>
    /// Pourcentage déjà calculé côté moteur (ou service)
    /// </summary>
    public int ProgressPercentage { get; set; }
    
    public DateTime StartTimeUtc { get; set; }

    /// <summary>
    /// Null tant que le job n'est pas terminé.
    /// </summary>
    public DateTime? EndTimeUtc { get; set; }

    public string? ErrorMessage { get; set; }

    // Constructeur vide requis pour certains sérialiseurs (XML)
    public JobStateDto() { }
}