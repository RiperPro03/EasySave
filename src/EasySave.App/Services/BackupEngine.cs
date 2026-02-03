using EasySave.Core.DTO;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;

namespace EasySave.App.Services;

public sealed class BackupEngine : IBackupEngine
{
    public BackupResultDto Run(BackupJob job)
    {
        return new BackupResultDto
        {
            Success = false,
            Message = "//TODO: backup engine not implemented.",
            Duration = TimeSpan.Zero
        };
    }
}
