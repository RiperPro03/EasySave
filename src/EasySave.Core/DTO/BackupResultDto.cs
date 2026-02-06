namespace EasySave.Core.DTO;

/// <summary>
/// Résultat d'une exécution de job (utile CLI/GUI/tests).
/// </summary>
public class BackupResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public int FilesProcessed { get; set; }
    public long TotalBytesProcessed { get; set; }
    public int CopiedCount { get; set; }
    public int SkippedCount { get; set; }
    public int ErrorCount { get; set; }

    /// <summary>Durée totale (en ticks .NET). Alternative : DurationMs (long) si tu préfères.</summary>
    public TimeSpan Duration { get; set; }

    public List<string> Errors { get; set; } = new();

    public BackupResultDto() { }
}
