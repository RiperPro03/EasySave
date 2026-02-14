using System;
using System.Globalization;
using Avalonia.Data.Converters;
using EasySave.Core.Enums;

namespace EasySave.App.Gui.Converters;

public sealed class TypeToTextConverter : IValueConverter
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
