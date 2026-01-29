namespace EasySave.Core.DTO;

/// <summary>
/// Entrée de log (daily log).
/// Données sérialisables (JSON/XML).
/// </summary>
public class LogEntryDto
{
    public DateTime TimestampUtc { get; set; }

    public string JobName { get; set; } = string.Empty;

    /// <summary>Chemin source (fichier ou dossier concerné selon ton implémentation).</summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>Chemin destination.</summary>
    public string TargetPath { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    /// <summary>Durée de transfert en millisecondes.</summary>
    public double TransferTimeMs { get; set; }

    /// <summary>
    /// Statut textuel pour compatibilité simple (ex: "OK", "ERROR", "SKIPPED").
    /// Si tu préfères : remplace par un enum LogStatus.
    /// </summary>
    public string Status { get; set; } = "OK";

    /// <summary>
    /// message d'erreur si Status=ERROR.
    /// </summary>
    public string? ErrorMessage { get; set; }

    public LogEntryDto() { }

}