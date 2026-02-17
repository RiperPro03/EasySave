using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;

namespace EasySave.App.Gui.Converters;

public sealed class L10nFormatConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count == 0)
            return string.Empty;

        var resolvedValues = values
            .Select(UnwrapUnsetValue)
            .ToArray();

        if (resolvedValues.Length == 1)
            return resolvedValues[0]?.ToString() ?? string.Empty;

        var format = resolvedValues[^1] as string;
        var args = resolvedValues.Take(resolvedValues.Length - 1).ToArray();

        if (args.Length == 1 && args[0] == null)
            return string.Empty;

        if (string.IsNullOrWhiteSpace(format))
            return args.Length == 1 ? args[0]?.ToString() ?? string.Empty : string.Join(" ", args);

        try
        {
            return string.Format(culture, format, args);
        }
        catch (FormatException)
        {
            return string.Empty;
        }
    }

    private static object? UnwrapUnsetValue(object? value)
    {
        return value is Avalonia.UnsetValueType ? null : value;
    }
}
