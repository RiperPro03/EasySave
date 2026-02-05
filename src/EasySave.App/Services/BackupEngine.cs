/*using EasySave.Core.DTO;
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
}*/

using EasySave.Core.DTO;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;

namespace EasySave.App.Services;

public sealed class BackupEngine : IBackupEngine
{
    private readonly IBackupService _backupService;

    // Constructeur utilisé par le CompositionRoot
    public BackupEngine()
        : this(new BackupService())
    {
    }

    // Constructeur "propre" pour DI / tests / futur
    public BackupEngine(IBackupService backupService)
    {
        _backupService = backupService;
    }

    public BackupResultDto Run(BackupJob job)
    {
        var start = DateTime.Now;

        try
        {
            _backupService.FullBackup(job.SourcePath, job.TargetPath);

            return new BackupResultDto
            {
                Success = true,
                Message = "Sauvegarde terminée avec succès.",
                Duration = DateTime.Now - start
            };
        }
        catch (Exception ex)
        {
            return new BackupResultDto
            {
                Success = false,
                Message = ex.Message,
                Duration = DateTime.Now - start
            };
        }
    }
}

