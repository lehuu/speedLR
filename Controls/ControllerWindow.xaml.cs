using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation; // Added for animations
using SpeedLR.Model;
using SpeedLR.Utils;
using static SpeedLR.Controls.ControllerViewModel;
using Point = System.Drawing.Point;
using Timer = System.Timers.Timer;

namespace SpeedLR.Controls
{
	public partial class ControllerWindow : Window
	{
		public ControllerViewModel ViewModel => (ControllerViewModel)DataContext;
		private readonly Timer _hideTimer;

		public ControllerWindow()
		{
			InitializeComponent();
			_hideTimer = new Timer(500);
			_hideTimer.AutoReset = false;
			_hideTimer.Elapsed += OnHideElapsed;
		}

		// ... [ToggleVisibility and Positioning methods remain unchanged] ...

		private void Command_Scroll(int direction)
		{
			if (!ViewModel.IsConnected || ViewModel.SelectedAction == null)
			{
				return;
			}

			// Animate to 0.01 opacity instantly (or very fast)
			DoubleAnimation fadeOut = new DoubleAnimation
			{
				To = 0.01,
				Duration = TimeSpan.FromMilliseconds(150),
				EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
			};
			this.BeginAnimation(OpacityProperty, fadeOut);

			string[] steps = new string[] { "1%", "2%", "5%" };
			var stepPercentage = steps[(int)ViewModel.StepSize];

			if (direction > 0)
			{
				Connector.Instance.SendCommandAsync(ViewModel.SelectedAction.Command + "=+" + stepPercentage);
			}
			else
			{
				Connector.Instance.SendCommandAsync(ViewModel.SelectedAction.Command + "=-" + stepPercentage);
			}

			_hideTimer.Stop();
			_hideTimer.Start();
		}

		private void OnHideElapsed(object sender, ElapsedEventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				// Smoothly animate back to full opacity
				DoubleAnimation fadeIn = new DoubleAnimation
				{
					To = 1.0,
					Duration = TimeSpan.FromMilliseconds(150),
					EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
				};
				this.BeginAnimation(OpacityProperty, fadeIn);
			});
		}

		// ... [Rest of your helper methods (Command_UpDown, Reset, etc.)] ...

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
			System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromPoint(mousePosition);
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

		private void HideButton_Click(object sender, RoutedEventArgs e) => Hide();

		private void DragButton_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left) this.DragMove();
		}

		private void SubmenuButton_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as SubmenuButton)?.Submenu is Submenu submenu)
				ViewModel.SelectedSubmenu = submenu;
		}

		private void ActionButton_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as ActionButton)?.Action is ActionElement action)
				ViewModel.SelectedAction = action;
		}

		private void Stepper_Scroll(int direction)
		{
			var values = (StepMode[])Enum.GetValues(typeof(StepMode));
			int count = values.Length;
			int currentIndex = (int)ViewModel.StepSize;
			currentIndex = direction > 0 ? (currentIndex - 1 + count) % count : (currentIndex + 1) % count;
			ViewModel.StepSize = (StepMode)currentIndex;
		}

		private void Stepper_LeftRight(int direction) => Stepper_Scroll(-1 * direction);

		private void Menu_Scroll(int direction)
		{
			int count = ViewModel.UserMenus.Count;
			int currentIndex = ViewModel.SelectedMenu != null ? ViewModel.UserMenus.IndexOf(ViewModel.SelectedMenu) : -1;
			currentIndex = direction > 0 ? (currentIndex - 1 + count) % count : (currentIndex + 1) % count;
			ViewModel.SelectedMenu = ViewModel.UserMenus[currentIndex];
		}

		private void Submenu_Scroll(int direction)
		{
			int count = ViewModel.SelectedMenu?.Submenus.Count ?? 0;
			if (count == 0 || ViewModel.SelectedMenu == null) return;
			int currentIndex = ViewModel.SelectedSubmenu != null ? ViewModel.SelectedMenu.Submenus.IndexOf(ViewModel.SelectedSubmenu) : -1;
			currentIndex = direction > 0 ? (currentIndex - 1 + count) % count : (currentIndex + 1) % count;
			ViewModel.SelectedSubmenu = ViewModel.SelectedMenu.Submenus[currentIndex];
		}

		private void Menu_LeftRight(int direction) => Menu_Scroll(-1 * direction);
		private void Submenu_LeftRight(int direction) => Submenu_Scroll(-1 * direction);

		private void Command_UpDown(int direction)
		{
			if (ViewModel.SelectedSubmenu == null) return;
			var actionItems = ViewModel.SelectedSubmenu.Items.OfType<ActionElement>().ToList();
			int count = actionItems?.Count ?? 0;
			if (count == 0) return;
			int currentIndex = ViewModel.SelectedAction != null ? actionItems.IndexOf(ViewModel.SelectedAction) : -1;
			currentIndex = direction > 0 ? (currentIndex - 1 + count) % count : (currentIndex + 1) % count;
			ViewModel.SelectedAction = actionItems[currentIndex];
		}

		private void Command_Reset() => Connector.Instance.SendCommandAsync(ViewModel.SelectedAction.Command + "=reset");
		private void Step_Reset() => ViewModel.StepSize = 0;
		private void Menu_Reset() => ViewModel.SelectedMenu = ViewModel.UserMenus.FirstOrDefault();
		private void Submenu_Reset() => ViewModel.SelectedSubmenu = ViewModel.SelectedMenu?.Submenus.FirstOrDefault();
	}
}