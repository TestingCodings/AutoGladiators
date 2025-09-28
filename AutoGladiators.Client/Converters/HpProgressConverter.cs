using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace AutoGladiators.Client.Converters
{
    public class HpProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int hp && parameter is string maxHpStr && int.TryParse(maxHpStr, out int maxHp))
            {
                return Math.Max(0, (double)hp / maxHp);
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
