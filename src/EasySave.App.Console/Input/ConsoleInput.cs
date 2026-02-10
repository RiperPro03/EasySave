using EasySave.Core.Resources;

namespace EasySave.App.Console.Input;

///<summary>
/// Cette classe sert à forcer l'utilisateur à donner une réponse correcte avant de continuer
/// </summary> 
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

    /// <summary>
    /// Empêche l'utilisateur de laisser un champ vide
    /// </summary>
    public string ReadNonEmptyString(string prompt)
    {
        while (true)
        {
            System.Console.Write(prompt);
            var input = System.Console.ReadLine();

            ///<summary>
            /// Vérifie que le texte n'est pas vide ou rempli d'espaces
            /// </summary> 
            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();

            System.Console.WriteLine(Strings.Error_EmptyValue);
        }
    }

    /// <summary>
    /// Vérifie que le chiffre tapé fait bien partie des options proposées
    /// </summary>
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
