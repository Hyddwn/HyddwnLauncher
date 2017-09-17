using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace HyddwnLauncher.PackOps.Util.Converters
{
	[ValueConversion(typeof(int), typeof(bool))]
	public class BooleanAndMultiConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var valuesAreInts = values.All(x => x is int);

			if (!valuesAreInts) throw new Exception("Alls values must be of type 'System.Int32'");

			var initialValue = (int) values[0];
			return values.Any(x => (int) x != initialValue);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
		}
	}
}