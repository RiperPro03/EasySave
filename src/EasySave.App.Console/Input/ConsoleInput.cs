using EasySave.Core.Resources;

namespace EasySave.App.Console.Input;

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

    public string ReadNonEmptyString(string prompt)
    {
        while (true)
        {
            System.Console.Write(prompt);
            var input = System.Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();

            System.Console.WriteLine(Strings.Error_EmptyValue);
        }
    }

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
