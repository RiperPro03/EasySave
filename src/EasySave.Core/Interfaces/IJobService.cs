using EasySave.Core.Enums;
using EasySave.Core.Models;

namespace EasySave.Core.Interfaces;

public interface IJobService
{
    IReadOnlyList<BackupJob> GetAll();
    BackupJob? GetById(string id);

    void Create(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive = true);
    void Update(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive);
    void MarkExecuted(string id, DateTime? nowUtc = null);
    void Delete(string id);
}
