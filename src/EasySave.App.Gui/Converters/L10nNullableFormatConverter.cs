using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace EasySave.App.Gui.Converters;

public sealed class L10nNullableFormatConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count < 3)
            return string.Empty;

        var rawValue = UnwrapUnsetValue(values[0]);
        var format = UnwrapUnsetValue(values[1]) as string;
        var fallback = UnwrapUnsetValue(values[2]) as string ?? string.Empty;

        if (rawValue == null || (rawValue is string text && string.IsNullOrWhiteSpace(text)))
            return fallback;

        if (string.IsNullOrWhiteSpace(format))
            return rawValue.ToString() ?? fallback;

        try
        {
            return string.Format(culture, format, rawValue);
        }
        catch (FormatException)
        {
            return fallback;
        }
    }

    private static object? UnwrapUnsetValue(object? value)
    {
        return value is Avalonia.UnsetValueType ? null : value;
    }
}
