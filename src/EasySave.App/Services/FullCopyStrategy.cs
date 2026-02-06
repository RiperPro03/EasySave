using EasySave.Core.Interfaces;

namespace EasySave.App.Services;

internal sealed class FullCopyStrategy : IBackupCopyStrategy
{
    public bool ShouldCopy(string sourcePath, string targetPath)
    {
        return true;
    }
}
