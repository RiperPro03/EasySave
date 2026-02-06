using EasySave.Core.DTO;

namespace EasySave.Core.Interfaces;

/// <summary>
/// Service responsable de l'écriture du snapshot d'état global.
/// </summary>
public interface IStateWriter
{
    void Write(AppStateDto state);
}
