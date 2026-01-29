using EasySave.Core.DTO;

namespace EasySave.Core.Interfaces;

/// <summary>
/// Service responsable de l'écriture de l'état temps réel d'un job.
/// </summary>
public interface IStateWriter
{
    void Write(JobStateDto state);
}