namespace EasySave.Core.Interfaces;

public interface IBackupCopyStrategy
{
    bool ShouldCopy(string sourcePath, string targetPath);
}
