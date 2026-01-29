using EasySave.Core.Common;
using EasySave.Core.Enums;

namespace EasySave.Core.Models;

/// <summary>
/// Représente un job de sauvegarde (métier).
/// </summary>
public sealed class BackupJob
{
    public string Id { get; }
    public string Name { get; private set; }
    public string SourcePath { get; private set; }
    public string TargetPath { get; private set; }
    public BackupType Type { get; private set; }

    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? LastRun { get; private set; }

    public BackupJob(
        string id,
        string name,
        string sourcePath,
        string targetPath,
        BackupType type,
        bool isActive = true,
        DateTime? createdAtUtc = null,
        DateTime? lastRunUtc = null)
    {
        Id = Guard.NotNullOrWhiteSpace(id, nameof(id));
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        SourcePath = Guard.NotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        TargetPath = Guard.NotNullOrWhiteSpace(targetPath, nameof(targetPath));

        Type = type;

        IsActive = isActive;
        CreatedAt = (createdAtUtc ?? DateTime.UtcNow).ToUniversalTime();
        LastRun = lastRunUtc?.ToUniversalTime();
    }

    /// <summary>Active le job.</summary>
    public void Enable() => IsActive = true;

    /// <summary>Désactive le job.</summary>
    public void Disable() => IsActive = false;

    /// <summary>Met à jour l'horodatage du dernier run.</summary>
    public void MarkExecuted(DateTime? nowUtc = null)
        => LastRun = (nowUtc ?? DateTime.UtcNow).ToUniversalTime();

    /// <summary>Met à jour les infos principales du job (optionnel si tu as un écran "éditer job").</summary>
    public void UpdateDefinition(string name, string sourcePath, string targetPath, BackupType type)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        SourcePath = Guard.NotNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        TargetPath = Guard.NotNullOrWhiteSpace(targetPath, nameof(targetPath));
        Type = type;
    }
    
    public override string ToString()
        => $"{Name} ({Type}) | {SourcePath} -> {TargetPath} | Active={IsActive}";
}