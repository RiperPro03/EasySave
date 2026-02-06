using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Core.DTO;
using EasySave.Core.Interfaces;

namespace EasySave.App.Services;

public sealed class StateWriter : IStateWriter
{
    private readonly IPathProvider _pathProvider;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public StateWriter(IPathProvider pathProvider)
    {
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
    }

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
