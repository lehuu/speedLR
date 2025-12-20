using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using SpeedLR.Model;
using Binding = System.Windows.Data.Binding;

namespace SpeedLR.Controls
{
    class SubmenuButton: ToggleButton
	{
        public SubmenuButton()
        {
			ToolTipService.SetInitialShowDelay(this, 0);
			ToolTipService.SetShowDuration(this, int.MaxValue);
		}

		public static readonly DependencyProperty SubmenuProperty =
		DependencyProperty.Register(nameof(Submenu), typeof(Submenu), typeof(SubmenuButton),
			new PropertyMetadata(null, OnSubmenuChanged));

		public Submenu Submenu
		{
			get => (Submenu)GetValue(SubmenuProperty);
			set => SetValue(SubmenuProperty, value);
		}

		private static void OnSubmenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SubmenuButton button && e.NewValue is Submenu submenu)
			{
				button.SetBinding(ContentProperty, new Binding(nameof(submenu.ShortName)) { Source = submenu });
				button.SetBinding(ToolTipProperty, new Binding(nameof(submenu.Name)) { Source = submenu });
				button.SetBinding(BackgroundProperty, new Binding(nameof(submenu.BackgroundColor)) { Source = submenu });
				button.SetBinding(ForegroundProperty, new Binding(nameof(submenu.FontColor)) { Source = submenu });
			}
		}
	}
}
