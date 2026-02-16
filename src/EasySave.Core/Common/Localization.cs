using System.Globalization;
using EasySave.Core.Enums;

namespace EasySave.Core.Common;

/// <summary>
/// Provides culture resolution for the application.
/// This class bridges the gap between EasySave's own ‘Language’ enumeration and .NET's standard ‘CultureInfo’ objects.
/// </summary>
public static class Localization
{
    /// <summary>
    /// Maps a language enum to a concrete culture.
    /// </summary>
    /// <param name="language">The language to resolve.</param>
    /// <returns>The matching <see cref="CultureInfo"/> instance.</returns>
    public static CultureInfo GetCulture(Language language)
    {
        return language switch
        {
            Language.French => new CultureInfo("fr-FR"),
            _ => new CultureInfo("en-US"),
        };
    }
}
