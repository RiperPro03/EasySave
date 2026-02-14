using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using EasySave.Core.Enums;

namespace EasySave.App.Gui.Converters;

public sealed class LogLevelToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LogLevel level)
            return Brush.Parse("#80FFFFFF");

        return level switch
        {
            LogLevel.Emergency => Brush.Parse("#FF3B30"),
            LogLevel.Alert => Brush.Parse("#FF3B30"),
            LogLevel.Critical => Brush.Parse("#FF453A"),
            LogLevel.Error => Brush.Parse("#FF3B30"),
            LogLevel.Warning => Brush.Parse("#FF9F0A"),
            LogLevel.Notice => Brush.Parse("#FFD60A"),
            LogLevel.Info => Brush.Parse("#0A84FF"),
            LogLevel.Debug => Brush.Parse("#8E8E93"),
            _ => Brush.Parse("#80FFFFFF")
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}
