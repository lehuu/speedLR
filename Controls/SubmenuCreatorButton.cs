using System.Windows;
using System.Windows.Controls;
using SpeedLR.Model;
using SpeedLR.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Button = System.Windows.Controls.Button;

namespace SpeedLR.Controls
{
	class SubmenuCreatorButton : Button
	{
		public SubmenuCreatorButton()
		{
			Style style = (Style)FindResource("EmptyButtonStyle");

			if (style != null)
			{
				Style = style;
			}

			this.DataContextChanged += OnDataContextChanged;
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			// e.NewValue is the Submenu object from your list
			if (e.NewValue is Submenu submenu)
			{
				Content = submenu.ShortName;
				ToolTip = submenu.Name;
				ToolTip = new System.Windows.Controls.ToolTip { Content = submenu.Name };
				ToolTipService.SetInitialShowDelay(this, 0);
				ToolTipService.SetShowDuration(this, int.MaxValue);
				Background = BrushHelper.GetBrushFromHex(submenu.BackgroundColor);
				Foreground = BrushHelper.GetBrushFromHex(submenu.FontColor);
			}
		}
	}
}
