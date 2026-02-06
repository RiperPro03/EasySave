using System;
using EasySave.Core.DTO;
using EasySave.Core.Events;
using EasySave.Core.Models;

namespace EasySave.Core.Interfaces;

/// <summary>
/// Moteur métier responsable de l'exécution d'un job de sauvegarde.
/// </summary>
public interface IBackupEngine
{
    event EventHandler<JobStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Exécute un job de sauvegarde.
    /// </summary>
    BackupResultDto Run(BackupJob job);
}
