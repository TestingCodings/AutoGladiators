using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace AutoGladiators.Client.Converters
{
    public class PercentageToDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage)
            {
                return percentage / 100.0;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double decimal_value)
            {
                return decimal_value * 100.0;
            }
            return 0.0;
        }
    }

    public class IsGreaterThanZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue > 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MoveAvailabilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // This would check if a move can be used by the bot
            // For now, return true as a placeholder
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1.0 : 0.5;
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MoveTierToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() is string tier)
            {
                return tier switch
                {
                    "Basic" => Colors.Gray,
                    "Advanced" => Colors.Yellow,
                    "Special" => Colors.Orange,
                    "Ultimate" => Colors.Red,
                    _ => Colors.White
                };
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SafeHealthDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Expects the bot object, parameter should be the type ("current" or "max")
            if (value is AutoGladiators.Core.Core.GladiatorBot bot)
            {
                string type = parameter?.ToString() ?? "current";
                return type.ToLower() switch
                {
                    "current" => bot.CurrentHealth.ToString(),
                    "max" => bot.MaxHealth.ToString(),
                    "currentmp" => bot.CurrentMP.ToString(),
                    "maxmp" => bot.MaxMP.ToString(),
                    "energy" => bot.Energy.ToString(),
                    "maxenergy" => bot.MaxEnergy.ToString(),
                    _ => "0"
                };
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SafeHealthRatioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AutoGladiators.Core.Core.GladiatorBot bot)
            {
                string type = parameter?.ToString() ?? "health";
                return type.ToLower() switch
                {
                    "health" => $"{bot.CurrentHealth}/{bot.MaxHealth}",
                    "mp" => $"{bot.CurrentMP}/{bot.MaxMP}",
                    "energy" => $"{bot.Energy}/{bot.MaxEnergy}",
                    _ => "0/0"
                };
            }
            return "0/0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}