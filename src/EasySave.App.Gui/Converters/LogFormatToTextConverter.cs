using System;
using System.Globalization;
using Avalonia.Data.Converters;
using EasySave.Core.Resources;
using EasySave.EasyLog.Options;

namespace EasySave.App.Gui.Converters;

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
