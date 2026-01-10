using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SpeedLR.Model;
using Binding = System.Windows.Data.Binding;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Point = System.Windows.Point;

namespace SpeedLR.Controls
{
	public class ActionButton : ToggleButton
	{
		public ActionButton() { }

		public static readonly DependencyProperty ActionProperty =
			DependencyProperty.Register(nameof(Action), typeof(ActionElement), typeof(ActionButton),
				new PropertyMetadata(null, OnActionChanged));


		public ActionElement Action
		{
			get => (ActionElement)GetValue(ActionProperty);
			set => SetValue(ActionProperty, value);
		}

		private static void OnActionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ActionButton button && e.NewValue is ActionElement action)
			{
				button.SetBinding(ContentProperty, new Binding(nameof(action.Name)) { Source = action });
				var background = CreateGradientBrush(action.Gradient.ToArray());
				if(background != null)
				{
					button.Background = background;
					button.Foreground = System.Windows.Media.Brushes.White;
				}
			}
		}

		private static Brush? CreateGradientBrush(string[] hexColors)
		{
			if (hexColors == null || !hexColors.Any()) return null;

			var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };
			var count = hexColors.Length;
			for (int i = 0; i < count; i++)
			{
				var offset = (double)i / (count - 1);
				try
				{
					brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(hexColors[i]), offset));
				}
				catch
				{
					// Skip invalid hex
				}
			}
			return brush;
		}
	}
}
