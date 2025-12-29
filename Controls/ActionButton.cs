using System.Windows;
using System.Windows.Controls.Primitives;
using SpeedLR.Model;
using Binding = System.Windows.Data.Binding;

namespace SpeedLR.Controls
{
    class ActionButton: ToggleButton
	{
        public ActionButton()
        {
		}

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
			}
		}
	}
}
