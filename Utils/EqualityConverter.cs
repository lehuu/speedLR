using System.Globalization;
using System.Windows.Data;

namespace SpeedLR.Utils
{
	public class EqualityConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType,
							  object parameter, CultureInfo culture)
		{
			if (values == null || values.Length < 2) return false;
			var left = values[0];
			var right = values[1];
			return Equals(left, right);
		}

		public object[] ConvertBack(object value, Type[] targetTypes,
									object parameter, CultureInfo culture)
			=> throw new NotSupportedException();
	}
}
