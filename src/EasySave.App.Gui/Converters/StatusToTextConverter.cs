using System;
using System.Globalization;
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
