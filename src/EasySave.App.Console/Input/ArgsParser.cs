namespace EasySave.App.Console.Input;

/// <summary>
/// Parses command-line job identifiers.
/// </summary>
public sealed class ArgsParser
{
    private const int MinId = 1;
    private const int MaxId = 5;

    /// <summary>
    /// Parses a raw argument string into job IDs.
    /// </summary>
    /// <param name="rawArgs">Raw argument string.</param>
    /// <returns>A list of unique job IDs.</returns>
    /// <exception cref="ArgumentException">Thrown when input is empty.</exception>
    /// <exception cref="FormatException">Thrown when the format is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an ID is out of range.</exception>
    public IReadOnlyList<int> Parse(string rawArgs)
    {
        if (string.IsNullOrWhiteSpace(rawArgs))
            throw new ArgumentException("No job ids were provided.", nameof(rawArgs));
        
        // Nettoie les espaces inutiles pour éviter les erreurs de parsing
        var sanitized = rawArgs.Replace(" ", string.Empty);

        var hasRange = sanitized.Contains('-');
        var hasList = sanitized.Contains(';');
        
        // Empêche de mélanger les tirets et les points-virgules
        if (hasRange && hasList)
            throw new FormatException("Mixed range and list syntax is not supported.");

        if (hasRange)
        {
            // Format attendu: "startIdJob-endIdJob".
            var parts = sanitized.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
                throw new FormatException("Invalid range format.");

            if (!int.TryParse(parts[0], out var start) || !int.TryParse(parts[1], out var end))
                throw new FormatException("Invalid range numbers.");

            if (start > end)
                throw new FormatException("Range start must be less than or equal to range end.");

            var result = new List<int>();
            for (var i = start; i <= end; i++)
            {
                // Valide chaque id du range.
                ValidateId(i);
                result.Add(i);
            }

            return result;
        }

        // Sinon, format liste: "1;2;3".
        var tokens = sanitized.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
            throw new FormatException("No ids found.");

        var ids = new List<int>();
        var seen = new HashSet<int>();
        foreach (var token in tokens)
        {
            if (!int.TryParse(token, out var id))
                throw new FormatException("Invalid job id.");

            ValidateId(id);
            if (seen.Add(id))
                // Evite les doublons en conservant l'ordre.
                ids.Add(id);
        }

        return ids;
    }

    private static void ValidateId(int id)
    {
        // Les IDs sont limites au range configure.
        if (id < MinId || id > MaxId)
            throw new ArgumentOutOfRangeException(nameof(id), $"Job id must be between {MinId} and {MaxId}.");
    }
}
