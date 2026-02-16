using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;
using EasySave.Core.Resources;
using EasySave.Core.Enums;
using EasySave.EasyLog.Options;

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

public sealed class LogFormatToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LogFormat format)
        {
            return format switch
            {
                LogFormat.Json => Strings.Gui_LogFormat_Json,
                LogFormat.Xml => Strings.Gui_LogFormat_Xml,
                _ => Strings.Gui_Common_Unknown
            };
        }

        return Strings.Gui_Common_Unknown;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            if (string.Equals(text, Strings.Gui_LogFormat_Json, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "JSON", StringComparison.OrdinalIgnoreCase))
                return LogFormat.Json;
            if (string.Equals(text, Strings.Gui_LogFormat_Xml, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "XML", StringComparison.OrdinalIgnoreCase))
                return LogFormat.Xml;
        }

        return LogFormat.Json;
    }
}

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

