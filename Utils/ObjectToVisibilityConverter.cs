using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;

namespace SpeedLR.Utils
{
	public class ObjectToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
							  object parameter, CultureInfo culture)
			=> value != null ? Visibility.Visible : Visibility.Collapsed;

		public object ConvertBack(object value, Type targetType,
								  object parameter, CultureInfo culture)
			=> Binding.DoNothing;
	}
}
