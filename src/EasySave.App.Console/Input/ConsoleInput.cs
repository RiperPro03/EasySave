using EasySave.Core.Resources;

namespace EasySave.App.Console.Input;

// Cette classe sert à forcer l'utilisateur à donner une réponse correcte avant de continuer
public sealed class ConsoleInput
{
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

    // Empêche l'utilisateur de laisser un champ vide
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

    // Vérifie que le chiffre tapé fait bien partie des options proposées
    public int ReadChoice(string prompt, IEnumerable<int> validChoices)
    {
        var validSet = new HashSet<int>(validChoices);
        while (true)
        {
            var value = ReadInt(prompt);
            if (validSet.Contains(value))
                return value;

            System.Console.WriteLine(Strings.Error_InvalidChoice);
        }
    }
}
