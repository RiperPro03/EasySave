using System.Globalization;
using EasySave.Core.Enums;

namespace EasySave.Core.Common;

// Gestionnaire de localisation pour l'application.
// Cette classe permet de faire le pont entre l'énumération 'Language' propre à EasySave et les objets 'CultureInfo' standards de .NET.
public static class Localization
{
    public static CultureInfo GetCulture(Language language)
    {
        return language switch
        {
            Language.French => new CultureInfo("fr-FR"),
            _ => new CultureInfo("en-US"),
        };
    }
}