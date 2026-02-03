namespace EasySave.App.Console.Input;

public sealed class ArgsParser
{
    private const int MinId = 1;
    private const int MaxId = 5;

    public IReadOnlyList<int> Parse(string rawArgs)
    {
        if (string.IsNullOrWhiteSpace(rawArgs))
            throw new ArgumentException("No job ids were provided.", nameof(rawArgs));

        var sanitized = rawArgs.Replace(" ", string.Empty);

        var hasRange = sanitized.Contains('-');
        var hasList = sanitized.Contains(';');

        if (hasRange && hasList)
            throw new FormatException("Mixed range and list syntax is not supported.");

        if (hasRange)
        {
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
                ValidateId(i);
                result.Add(i);
            }

            return result;
        }

        var tokens = sanitized.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
            throw new FormatException("No ids found.");

        var ids = new List<int>();
        foreach (var token in tokens)
        {
            if (!int.TryParse(token, out var id))
                throw new FormatException("Invalid job id.");

            ValidateId(id);
            ids.Add(id);
        }

        return ids;
    }

    private static void ValidateId(int id)
    {
        if (id < MinId || id > MaxId)
            throw new ArgumentOutOfRangeException(nameof(id), $"Job id must be between {MinId} and {MaxId}.");
    }
}
