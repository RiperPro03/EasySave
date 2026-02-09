using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Core.DTO;
using EasySave.Core.Interfaces;

namespace EasySave.App.Services;

// Cette classe écrit l'état global de l'application dans un fichier JSON
public sealed class StateWriter : IStateWriter
{
    private readonly IPathProvider _pathProvider;

    // Options pour rendre le fichier JSON lisible (indentation) et transformer les énumérations en texte
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public StateWriter(IPathProvider pathProvider)
    {
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
    }

    // Méthode appelée par le BackupService pour mettre à jour le fichier state.json
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
