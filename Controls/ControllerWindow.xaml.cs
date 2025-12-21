using System.Windows;
using System.Windows.Input;
using SpeedLR.Model;
using SpeedLR.Utils;
using Point = System.Drawing.Point;

namespace SpeedLR.Controls
{
	/// <summary>
	/// Interaction logic for ControllerWindow.xaml
	/// </summary>
	public partial class ControllerWindow : Window
	{
		public ControllerViewModel ViewModel => (ControllerViewModel)DataContext;
		public ControllerWindow()
		{
			InitializeComponent();
		}

		public void ToggleVisibility(bool isLightroomActive)
		{
			if (this.IsVisible)
			{
				this.Hide();
				return;
			}

			if (!isLightroomActive) return;

			if (ViewModel.IsPinned)
			{
				this.Show();
				return;
			}

			PositionAtMouse();
			this.Show();
		}

		private void ControllerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			Hide();
		}

		double _prevScale = -1;

		private void PositionAtMouse()
		{
			Point mousePosition = System.Windows.Forms.Control.MousePosition;
			Screen screen = Screen.FromPoint(mousePosition);
			var scale = DpiHelper.GetDpiScaleFactorForMousePosition();

			var formWidth = Width;
			var formHeight = Height;

			var topLeft = (new System.Windows.Point(screen.WorkingArea.Left / scale, screen.WorkingArea.Top / scale));
			var bottomRight = (new System.Windows.Point(screen.WorkingArea.Right / scale, screen.WorkingArea.Bottom / scale));

			var x = mousePosition.X / scale - formWidth / 2;
			var y = mousePosition.Y / scale - formHeight / 2;

			x = Math.Max(topLeft.X, Math.Min(x, bottomRight.X - formWidth));
			y = Math.Max(topLeft.Y, Math.Min(y, bottomRight.Y - formHeight));

			if (_prevScale > 0)
			{
				x = x * scale / _prevScale;
				y = y * scale / _prevScale;
			}
			_prevScale = scale;

			Left = x;
			Top = y;
		}
		private void HideButton_Click(object sender, RoutedEventArgs e)
		{
			Hide();
		}
		private void DragButton_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				this.DragMove();
			}
		}

		private void SubmenuButton_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as SubmenuButton)?.Submenu is Submenu submenu)
			{
				ViewModel.SelectedSubmenu = submenu;
			}
		}

		private void ActionButton_Click(object sender, RoutedEventArgs e)
		{
			if ( (sender as ActionButton)?.Action is ActionElement action)
			{
				ViewModel.SelectedAction = action;
			}
		}
	}
}
