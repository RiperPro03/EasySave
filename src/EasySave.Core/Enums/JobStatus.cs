namespace EasySave.Core.Enums;

/// <summary>
///  Représente le statut d'un job de sauvegarde.
/// - Idle: Le job existe mais n'a pas encore été lancé.
/// - Running: Sauvegarde en cours.
/// - Paused: Pause manuelle ou automatique.
/// - Completed: Sauvegarde terminée avec succès.
/// - Error: Erreur bloquante rencontrée.
/// </summary>
public enum JobStatus
{
    Idle = 0,
    Running = 1,
    Paused = 2,
    Completed = 3,
    Error = 4
}