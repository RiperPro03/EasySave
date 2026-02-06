using System;
using EasySave.Core.DTO;
using EasySave.Core.Events;
using EasySave.Core.Models;

namespace EasySave.Core.Interfaces;

public interface IBackupService
{
    event EventHandler<JobStateChangedEventArgs>? StateChanged;

    BackupResultDto Run(BackupJob job);
}
