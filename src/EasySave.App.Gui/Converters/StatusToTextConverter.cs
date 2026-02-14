using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EasySave.App.Gui.Converters;

public sealed class StatusToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
            return isActive ? "Active" : "Inactive";

        return "Unknown";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            if (string.Equals(text, "Active", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(text, "Inactive", StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return false;
    }
}
