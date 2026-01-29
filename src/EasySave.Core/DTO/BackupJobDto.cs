using EasySave.Core.Enums;
using EasySave.Core.Models;

namespace EasySave.Core.DTO;

/// <summary>
/// DTO de persistence pour un job de sauvegarde (JSON / disque).
/// Représente des données externes potentiellement invalides.
/// </summary>
public sealed class BackupJobDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? SourcePath { get; set; }
    public string? TargetPath { get; set; }

    /// <summary>
    /// Type stocké sous forme de string pour compatibilité JSON.
    /// </summary>
    public string? Type { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastRun { get; set; }

    /// <summary>
    /// Constructeur vide requis pour certains sérialiseurs (JSON/XML).
    /// </summary>
    public BackupJobDto() { }

    /// <summary>
    /// Vérifie la cohérence minimale des données lues depuis la persistence.
    /// </summary>
    public bool IsValid()
        => !string.IsNullOrWhiteSpace(Id)
        && !string.IsNullOrWhiteSpace(Name)
        && !string.IsNullOrWhiteSpace(SourcePath)
        && !string.IsNullOrWhiteSpace(TargetPath)
        && !string.IsNullOrWhiteSpace(Type)
        && Enum.TryParse<BackupType>(Type, out _);

    /// <summary>
    /// Transforme le DTO en modèle métier.
    /// Lève une exception si les données sont invalides.
    /// </summary>
    public BackupJob ToModel()
    {
        if (!IsValid())
            throw new ArgumentException("Invalid BackupJobDto. Cannot convert to BackupJob.");

        if (!Enum.TryParse<BackupType>(Type, out var backupType))
            throw new ArgumentException($"Invalid BackupType value: {Type}");

        return new BackupJob(
            id: Id!,
            name: Name!,
            sourcePath: SourcePath!,
            targetPath: TargetPath!,
            type: backupType,
            isActive: IsActive,
            createdAtUtc: CreatedAt,
            lastRunUtc: LastRun
        );
    }

    /// <summary>
    /// Crée un DTO à partir d'un modèle métier valide.
    /// </summary>
    public static BackupJobDto FromModel(BackupJob job)
    {
        return new BackupJobDto
        {
            Id = job.Id,
            Name = job.Name,
            SourcePath = job.SourcePath,
            TargetPath = job.TargetPath,
            Type = job.Type.ToString(),
            IsActive = job.IsActive,
            CreatedAt = job.CreatedAt,
            LastRun = job.LastRun
        };
    }
}