using System.Globalization;
using EasySave.Core.Enums;

namespace EasySave.Core.Common;

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