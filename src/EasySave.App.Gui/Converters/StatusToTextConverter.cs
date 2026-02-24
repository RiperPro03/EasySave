using System;
using System.Globalization;
using Avalonia.Data.Converters;
using EasySave.Core.Resources;

namespace EasySave.App.Gui.Converters;

public sealed class StatusToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
            return isActive ? Strings.Gui_Common_Active : Strings.Gui_Common_Inactive;

        return Strings.Gui_Common_Unknown;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            if (string.Equals(text, Strings.Gui_Common_Active, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "Active", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(text, Strings.Gui_Common_Inactive, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "Inactive", StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return false;
    }
}
