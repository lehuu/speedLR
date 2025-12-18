using System.Globalization;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;

namespace SpeedLR.Utils
{
	public class ObjectToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
							  object parameter, CultureInfo culture)
			=> value != null ? true : false;

		public object ConvertBack(object value, Type targetType,
								  object parameter, CultureInfo culture)
			=> Binding.DoNothing;
	}
}
