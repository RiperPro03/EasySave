using System;
using System.Globalization;
using Avalonia.Data.Converters;
using EasySave.Core.Enums;
using EasySave.Core.Resources;

namespace EasySave.App.Gui.Converters;

public sealed class TypeToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BackupType backupType)
        {
            return backupType switch
            {
                BackupType.Full => Strings.Gui_Common_Full,
                BackupType.Differential => Strings.Gui_Common_Differential,
                _ => Strings.Gui_Common_Unknown
            };
        }

        return Strings.Gui_Common_Unknown;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            if (string.Equals(text, Strings.Gui_Common_Full, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "Full", StringComparison.OrdinalIgnoreCase))
                return BackupType.Full;
            if (string.Equals(text, Strings.Gui_Common_Differential, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "Differential", StringComparison.OrdinalIgnoreCase))
                return BackupType.Differential;
        }

        return BackupType.Full;
    }
}

public sealed class TypeToEmojiConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BackupType backupType)
        {
            return backupType switch
            {
                BackupType.Full => "F",
                BackupType.Differential => "D",
                _ => "?"
            };
        }

        return "?";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            if (string.Equals(text, "F", StringComparison.OrdinalIgnoreCase))
                return BackupType.Full;
            if (string.Equals(text, "D", StringComparison.OrdinalIgnoreCase))
                return BackupType.Differential;
        }

        return BackupType.Full;
    }
}
