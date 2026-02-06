using EasySave.Core.DTO;
using EasySave.Core.Models;

namespace EasySave.Core.Interfaces;

public interface IBackupService
{
    BackupResultDto Run(BackupJob job);
}
