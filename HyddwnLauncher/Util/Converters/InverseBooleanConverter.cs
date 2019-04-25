using System;
using System.Globalization;
using System.Windows.Data;

namespace HyddwnLauncher.Util.Converters
{
    [ValueConversion(typeof(int), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (targetType != typeof(int))
                throw new InvalidOperationException("The target must be an Int32");

            return !((int) value > 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}