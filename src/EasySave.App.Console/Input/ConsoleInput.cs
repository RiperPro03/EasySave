using EasySave.Core.Resources;

namespace EasySave.App.Console.Input;

/// <summary>
/// Provides helper methods to read validated console input.
/// </summary>
public sealed class ConsoleInput
{
    /// <summary>
    /// Reads an integer from the console.
    /// </summary>
    /// <param name="prompt">Prompt displayed to the user.</param>
    /// <returns>The parsed integer.</returns>
    public int ReadInt(string prompt)
    {
        while (true)
        {
            System.Console.Write(prompt);
            var input = System.Console.ReadLine();
            if (int.TryParse(input, out var value))
                return value;

            System.Console.WriteLine(Strings.Error_InvalidNumber);
        }
    }

    /// <summary>
    /// Reads a non-empty string from the console.
    /// </summary>
    /// <param name="prompt">Prompt displayed to the user.</param>
    /// <returns>The trimmed non-empty string.</returns>
    public string ReadNonEmptyString(string prompt)
    {
        while (true)
        {
            System.Console.Write(prompt);
            var input = System.Console.ReadLine();
            
            // Vérifie que le texte n'est pas vide ou rempli d'espaces
            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();

            System.Console.WriteLine(Strings.Error_EmptyValue);
        }
    }

    /// <summary>
    /// Reads an integer that must be one of the valid choices.
    /// </summary>
    /// <param name="prompt">Prompt displayed to the user.</param>
    /// <param name="validChoices">Allowed values.</param>
    /// <returns>The selected choice.</returns>
    public int ReadChoice(string prompt, IEnumerable<int> validChoices)
    {
        var validSet = new HashSet<int>(validChoices);
        while (true)
        {
            // Reutilise la validation numerique.
            var value = ReadInt(prompt);
            if (validSet.Contains(value))
                return value;

            System.Console.WriteLine(Strings.Error_InvalidChoice);
        }
    }
}
