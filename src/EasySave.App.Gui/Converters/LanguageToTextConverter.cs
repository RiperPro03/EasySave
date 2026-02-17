using System;
using System.Globalization;
using Avalonia.Data.Converters;
using EasySave.Core.Enums;
using EasySave.Core.Resources;

namespace EasySave.App.Gui.Converters;

public sealed class LanguageToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Language language)
        {
            return language switch
            {
                Language.English => Strings.Lang_English,
                Language.French => Strings.Lang_French,
                _ => Strings.Gui_Common_Unknown
            };
        }

        return Strings.Gui_Common_Unknown;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            if (string.Equals(text, Strings.Lang_English, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "English", StringComparison.OrdinalIgnoreCase))
                return Language.English;
            if (string.Equals(text, Strings.Lang_French, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "French", StringComparison.OrdinalIgnoreCase))
                return Language.French;
        }

        return Language.English;
    }
}
