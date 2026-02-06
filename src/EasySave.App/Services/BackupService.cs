using EasySave.Core.DTO;
using EasySave.Core.Interfaces;
using EasySave.Core.Models;

namespace EasySave.App.Services;

public sealed class BackupService : IBackupService
{
    private readonly IBackupEngine _backupEngine;
    private readonly IJobService _jobService;

    public BackupService(IJobService jobService, string? logDirectory = null)
    {
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _backupEngine = new BackupEngine(logDirectory);
    }

    public BackupResultDto Run(BackupJob job)
    {
        var result = _backupEngine.Run(job);
        _jobService.MarkExecuted(job.Id);
        return result;
    }
}
