using EasySave.Core.DTO;

namespace EasySave.Core.Interfaces;

/// <summary>
/// Service responsable de l'écriture des logs journaliers.
/// </summary>
public interface ILogWriter
{
    void Write(LogEntryDto entry);
}