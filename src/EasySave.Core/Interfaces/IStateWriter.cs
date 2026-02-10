using EasySave.Core.DTO;

namespace EasySave.Core.Interfaces;

/// <summary>
/// Writes global state snapshots.
/// </summary>
public interface IStateWriter
{
    /// <summary>
    /// Writes the provided application state.
    /// </summary>
    /// <param name="state">The state snapshot to write.</param>
    void Write(AppStateDto state);
}
