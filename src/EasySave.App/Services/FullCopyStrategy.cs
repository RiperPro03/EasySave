using EasySave.Core.Interfaces;

namespace EasySave.App.Services;

/// <summary>
/// Cette stratégie implémente la sauvegarde complète
/// </summary>
internal sealed class FullCopyStrategy : IBackupCopyStrategy
{
    public bool ShouldCopy(string sourcePath, string targetPath)
    {
        return true;
    }
}
