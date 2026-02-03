using EasySave.Core.Enums;
using EasySave.Core.Models;

namespace EasySave.App.Services;

public interface IJobService
{
    IReadOnlyList<BackupJob> GetAll();
    BackupJob? GetById(string id);

    void Create(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive = true);
    void Update(string id, string name, string sourcePath, string targetPath, BackupType type, bool isActive);
    void Delete(string id);
}
