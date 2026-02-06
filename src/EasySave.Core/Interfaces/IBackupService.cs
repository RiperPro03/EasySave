namespace EasySave.Core.Interfaces;



public interface IBackupService
{
    void FullBackup(string sourcePath, string targetPath);
}

