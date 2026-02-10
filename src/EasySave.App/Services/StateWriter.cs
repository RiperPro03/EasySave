using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Core.DTO;
using EasySave.Core.Interfaces;

namespace EasySave.App.Services;

/// <summary>
/// Writes application state snapshots to disk.
/// </summary>
public sealed class StateWriter : IStateWriter
{
    private readonly IPathProvider _pathProvider;

    /// <summary>
    /// Options to make the JSON file readable (indentation) and convert enumerations to text
    /// </summary>
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="StateWriter"/> class.
    /// </summary>
    /// <param name="pathProvider">Provides state file paths.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pathProvider"/> is null.</exception>
    public StateWriter(IPathProvider pathProvider)
    {
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
    }

    /// <summary>
    /// Writes the provided application state to disk.
    /// </summary>
    /// <param name="state">The state snapshot to persist.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
    public void Write(AppStateDto state)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        _pathProvider.EnsureDirectoriesCreated();
        var statePath = Path.Combine(_pathProvider.StatePath, "state.json");
        var json = JsonSerializer.Serialize(state, _options);
        File.WriteAllText(statePath, json);
    }
}
