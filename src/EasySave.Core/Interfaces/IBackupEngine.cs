using EasySave.Core.DTO;
using EasySave.Core.Models;

namespace EasySave.Core.Interfaces;

/// <summary>
/// Moteur métier responsable de l'exécution d'un job de sauvegarde.
/// </summary>
public interface IBackupEngine
{
    /// <summary>
    /// Exécute un job de sauvegarde.
    /// </summary>
    BackupResultDto Run(BackupJob job);
}