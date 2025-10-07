using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using IndustrialPanel.Models;

namespace IndustrialPanel.ViewModels;

public class StatusColorConverter : IValueConverter
{
    public static readonly StatusColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DeviceStatus status)
        {
            return status switch
            {
                DeviceStatus.Running => new SolidColorBrush(Color.Parse("#7E57C2")),   // purple medium
                DeviceStatus.Warning => new SolidColorBrush(Color.Parse("#AB47BC")),   // purple brighter
                DeviceStatus.Error => new SolidColorBrush(Color.Parse("#4A148C")),     // deep purple
                DeviceStatus.Stopped => new SolidColorBrush(Color.Parse("#9E9E9E")),  // neutral gray
                _ => new SolidColorBrush(Color.Parse("#9E9E9E"))
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class RealTimeButtonConverter : IValueConverter
{
    public static readonly RealTimeButtonConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isEnabled)
        {
            return isEnabled ? "⏸ Zatrzymaj" : "▶ Uruchom Czas Rzeczywisty";
        }
        return "▶ Uruchom Czas Rzeczywisty";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EqualsConverter : IValueConverter
{
    public static readonly EqualsConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is null) return false;
        var left = value?.ToString() ?? string.Empty;
        var right = parameter?.ToString() ?? string.Empty;
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
