using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SpeedLR.Model;
using SpeedLR.Utils;
using Binding = System.Windows.Data.Binding;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;

namespace SpeedLR.Controls
{
	class SubmenuCreatorButton : Button
	{

		public event EventHandler<ColorItemEventArg> ColorItemClick;
		public event EventHandler DeleteClick;

		public enum ColorType
		{
			Background,
			Font
		}

		public class ColorItemEventArg : EventArgs
		{
			public string Value { get; set; }
			public ColorType Type { get; set; }

			public ColorItemEventArg(string value, ColorType type)
			{
				Value = value;
				Type = type;
			}
		}

		public SubmenuCreatorButton()
		{
			Style style = (Style)FindResource("EmptyButtonStyle");

			if (style != null)
			{
				Style = style;
			}

			this.DataContextChanged += OnDataContextChanged;
			this.ContextMenu = CreateContextMenu();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue is Submenu submenu)
			{
				this.SetBinding(Button.ContentProperty, new Binding("ShortName"));
				this.SetBinding(Button.ToolTipProperty, new Binding("Name"));
				this.SetBinding(Button.BackgroundProperty, new Binding("BackgroundColor"));
				this.SetBinding(Button.ForegroundProperty, new Binding("FontColor"));

				ToolTipService.SetInitialShowDelay(this, 0);
				ToolTipService.SetShowDuration(this, int.MaxValue);
			}
		}

		private ContextMenu CreateContextMenu()
		{
			ContextMenu contextMenu = new ContextMenu();

			MenuItem fontColorItem = new MenuItem { Header = "Font Color", Background = Brushes.DarkGray };
			MenuItem backgroundItem = new MenuItem { Header = "Background Color", Background = Brushes.DarkGray };
			foreach (var colorGroup in ColorData.GetColorGroups())
			{
				fontColorItem.Items.Add(CreateColorGroupMenuItem(colorGroup, ColorType.Font));
			}

			foreach (var colorGroup in ColorData.GetColorGroups())
			{
				backgroundItem.Items.Add(CreateColorGroupMenuItem(colorGroup, ColorType.Background));
			}

			contextMenu.Items.Add(backgroundItem);
			contextMenu.Items.Add(fontColorItem);

			MenuItem deleteItem = new MenuItem { Header = "Delete", Background = Brushes.IndianRed };
			deleteItem.Click += (s, args) =>
			{
				Command = null;
				Background = BrushHelper.GetBrushFromHex(ColorData.DEFAULT_BACKGROUND);
				Foreground = BrushHelper.GetBrushFromHex(ColorData.DEFAULT_FONT);
				DeleteClick?.Invoke(this, EventArgs.Empty);
			};

			contextMenu.Items.Add(deleteItem);

			contextMenu.PlacementTarget = this;
			return contextMenu;
		}

		private MenuItem CreateColorGroupMenuItem(ColorGroup colorGroup, ColorType type)
		{
			MenuItem menuItem = new MenuItem { Header = colorGroup.Name };
			var brushConveter = new BrushConverter();

			foreach (var hex in colorGroup.ColorHexes)
			{
				MenuItem colorItem = new MenuItem
				{
					Header = $"{colorGroup.Name} {Array.IndexOf(colorGroup.ColorHexes.ToArray(), hex) * 100 + 50}",
					Background = (SolidColorBrush)brushConveter.ConvertFromString(hex),
				};

				colorItem.Click += (s, args) =>
				{
					ColorItemClick?.Invoke(this, new ColorItemEventArg(hex, type));
				};

				menuItem.Items.Add(colorItem);
			}

			return menuItem;
		}
	}
}
