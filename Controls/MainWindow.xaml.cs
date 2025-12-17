using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using SpeedLR.Controls;
using SpeedLR.Utils;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Drawing.Point;

namespace SpeedLR
{
	public partial class MainWindow : Window
	{
		private NotifyIcon _notifyIcon;

		private ControllerWindow _controller;
		private LowLevelHotkey _hotkeyHook;
		private PortWindow? _portWindow;
		private ActiveWindowWatcher _watcher = new ActiveWindowWatcher();


		public MainWindow()
		{
			InitializeComponent();
			this.DataContext = new MainViewModel();

			SwitchToMenu(0);

			Loaded += (s, e) =>
			{
				Hide();
			};
		}

		private void SwitchToMenu(int menuIndex)
		{
			if (this.DataContext is MainViewModel viewModal)
			{
				if (menuIndex < 0 || menuIndex >= viewModal.UserMenus.Count)
				{
					return;
				}

				viewModal.SelectedMenu = viewModal.UserMenus[menuIndex];

			}

		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			Connector.Instance.ConnectionChanged += OnConnectionChanged;
			ConnectToServer();

			SetupContextMenu();

			_controller = new ControllerWindow();

			var helper = new WindowInteropHelper(this);

			_hotkeyHook = new LowLevelHotkey(Key.LeftCtrl);
			_hotkeyHook.KeyDoublePressed += Ctrl_DoublePressed;
		}

		private void SetupContextMenu()
		{
			// init context menu
			if (_notifyIcon == null)
			{
				_notifyIcon = new NotifyIcon();
				_notifyIcon.Icon = new Icon("wheel.ico"); // Replace with your icon file
				_notifyIcon.Text = Title;
				_notifyIcon.Visible = true;
				_notifyIcon.MouseDoubleClick += OpenMenuItem_Click;
			}

			// Create a context menu for the NotifyIcon
			ContextMenuStrip contextMenu = new ContextMenuStrip();
			ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
			exitMenuItem.Click += ExitMenuItem_Click;
			ToolStripMenuItem openMenuItem = new ToolStripMenuItem("Open");
			openMenuItem.Click += OpenMenuItem_Click;

			ToolStripMenuItem connectItem = null;

			switch (Connector.Instance.Status)
			{
				case Connector.ConnectionStatus.CONNECTING:
					connectItem = new ToolStripMenuItem("Connecting...");
					connectItem.BackColor = Color.LightBlue;
					break;
				case Connector.ConnectionStatus.CONNECTED:
					connectItem = new ToolStripMenuItem("Connected");
					connectItem.BackColor = Color.LightGreen;
					break;
				case Connector.ConnectionStatus.DISCONNECTED:
				default:
					connectItem = new ToolStripMenuItem("Connect");
					connectItem.BackColor = Color.IndianRed;
					connectItem.Click += (s, e) => { ConnectToServer(); };
					break;
			}

			contextMenu.Items.Add(openMenuItem);
			contextMenu.Items.Add(connectItem);
			contextMenu.Items.Add(exitMenuItem);

			_notifyIcon.ContextMenuStrip = contextMenu;
		}

		private void OnConnectionChanged(object sender, Connector.ConnectionStatus status)
		{
			Dispatcher.Invoke(() =>
			{
				switch (status)
				{
					case Connector.ConnectionStatus.CONNECTING:
						this.connectButton.Content = "Connecting...";
						this.connectButton.Background = Brushes.Blue;
						break;
					case Connector.ConnectionStatus.CONNECTED:
						this.connectButton.Background = Brushes.Green;
						this.connectButton.Content = "Connected";
						break;
					case Connector.ConnectionStatus.DISCONNECTED:
					default:
						this.connectButton.Background = Brushes.Red;
						this.connectButton.Content = "Reconnect";
						break;
				}
				SetupContextMenu();
			});
		}

		private async void ConnectToServer()
		{
			this.portButton.Content = "Port: " + LocalData.Instance.Port;
			await Connector.Instance.Connect(LocalData.Instance.Port);
		}

		double _prevScale = -1;

		private void Ctrl_DoublePressed(ref bool _)
		{
			if (_controller.IsVisible)
			{
				_controller.Hide();
				return;
			}

			if (!_watcher.IsLightroomActive)
			{
				return;
			}

			if (_controller.IsPinned)
			{
				_controller.Show();
				return;
			}

			Point mousePosition = System.Windows.Forms.Control.MousePosition;
			Screen screen = Screen.FromPoint(mousePosition);
			var scale = DpiHelper.GetDpiScaleFactorForMousePosition();

			var formWidth = _controller.Width;
			var formHeight = _controller.Height;

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

			_controller.Left = x;
			_controller.Top = y;

			_controller.Show();
		}

		private void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			var isConnected = Connector.Instance.Status == Connector.ConnectionStatus.CONNECTED;
			if (!isConnected)
			{
				ConnectToServer();
			}
		}

		private void PortButton_Click(object sender, RoutedEventArgs e)
		{
			if (_portWindow != null)
			{
				_portWindow.Close();
				_portWindow = null;
			}

			_portWindow = new PortWindow();
			_portWindow.Left = Left;
			_portWindow.Top = Top;
			_portWindow.Confirm += (s, args) =>
			{
				Connector.Instance.CloseConnection();
				ConnectToServer();
			};

			_portWindow.Show();
		}

		private void ExitMenuItem_Click(object sender, EventArgs e)
		{
			// Close the application
			_notifyIcon.Dispose(); // Release the notify icon resource
			Application.Current.Shutdown();
		}

		private void OpenMenuItem_Click(object sender, EventArgs e)
		{
			Show();
		}

		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			Hide();
		}

		protected override void OnClosed(EventArgs e)
		{
			Connector.Instance.CloseConnection();
			_hotkeyHook.Dispose();
		}

		private void AddMenu_Click(object sender, RoutedEventArgs e)
		{
			if (this.DataContext is MainViewModel viewModal)
			{
				var menuName = $"Menu_{viewModal.UserMenus.Count}";
				var newMenu = new Model.Menu(menuName);
				LocalData.Instance.UpdateUserMenu(newMenu);
				LocalData.Instance.SaveUserMenus();
				SwitchToMenu(viewModal.UserMenus.Count - 1);
			}
		}

		private void DeleteMenu_Click(object sender, RoutedEventArgs e)
		{
			if (this.DataContext is MainViewModel viewModal)
			{
				if (viewModal.SelectedMenu != null)
				{
					LocalData.Instance.UserMenus.Remove(viewModal.SelectedMenu);
					LocalData.Instance.SaveUserMenus();
					SwitchToMenu(0);
				}
			}
		}
	}
}