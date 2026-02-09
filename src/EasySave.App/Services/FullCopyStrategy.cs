using EasySave.Core.Interfaces;

namespace EasySave.App.Services;

// Cette stratégie implémente la sauvegarde complète
internal sealed class FullCopyStrategy : IBackupCopyStrategy
{
    public bool ShouldCopy(string sourcePath, string targetPath)
    {
        return true;
    }
}
