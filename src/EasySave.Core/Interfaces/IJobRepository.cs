using EasySave.Core.Models;

namespace EasySave.Core.Interfaces;

/// <summary>
/// Contrat de persistance des jobs de sauvegarde.
/// </summary>
public interface IJobRepository
{
    IReadOnlyList<BackupJob> GetAll();
    BackupJob? GetById(string id);

    void Add(BackupJob job);
    void Update(BackupJob job);
    void Remove(string id);
}