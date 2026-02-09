namespace EasySave.Core.Common;

// Classe utilitaire de validation .
// Elle sert de barrière de sécurité pour empêcher la création d'objets avec des données invalides.
internal static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null, empty or whitespace.", paramName);

        return value;
    }
}