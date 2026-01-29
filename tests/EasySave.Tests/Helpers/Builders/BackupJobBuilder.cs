using EasySave.Core.Enums;
using EasySave.Core.Models;

namespace EasySave.tests.Helpers.Builders;

/// <summary>
/// Builder de test pour créer rapidement un BackupJob valide.
/// Permet de ne pas dupliquer les paramètres dans tous les tests.
/// </summary>
internal sealed class BackupJobBuilder
{
    private string _id = "job-001";
    private string _name = "MyJob";
    private string _sourcePath = "/source";
    private string _targetPath = "/target";
    private BackupType _type = BackupType.Full;

    public static BackupJobBuilder Valid() => new BackupJobBuilder();

    public BackupJobBuilder WithId(string id) { _id = id; return this; }
    public BackupJobBuilder WithName(string name) { _name = name; return this; }
    public BackupJobBuilder WithSource(string sourcePath) { _sourcePath = sourcePath; return this; }
    public BackupJobBuilder WithTarget(string targetPath) { _targetPath = targetPath; return this; }
    public BackupJobBuilder WithType(BackupType type) { _type = type; return this; }

    public BackupJob Build()
        => new BackupJob(_id, _name, _sourcePath, _targetPath, _type);
}