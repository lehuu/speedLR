using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using SpeedLR.Controls;
using SpeedLR.Model;
using SpeedLR.Utils;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;

namespace SpeedLR
{
	public partial class MainWindow : Window
	{
		public MainViewModel ViewModel => (MainViewModel)DataContext;

		private NotifyIcon? _notifyIcon;

		private ControllerWindow? _controller;
		private LowLevelHotkey? _hotkeyHook;
		private ActiveWindowWatcher _watcher = new ActiveWindowWatcher();


		public MainWindow()
		{
			InitializeComponent();
			var helper = new System.Windows.Interop.WindowInteropHelper(this);
			var handle = helper.EnsureHandle();

			this.DataContext = new MainViewModel();
		}

		private void SwitchToMenu(int menuIndex)
		{
			if (this.DataContext is MainViewModel ViewModel)
			{
				if (menuIndex < 0 || menuIndex >= ViewModel.UserMenus.Count)
				{
					return;
				}

				ViewModel.SelectedMenu = ViewModel.UserMenus[menuIndex];
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
			_controller?.ToggleVisibility(_watcher.IsLightroomActive);
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
			PortWindow portWindow = new PortWindow
			{
				Owner = this, // Keeps the window centered over the main app
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};

			if (portWindow.ShowDialog() == true)
			{
				// This only runs if DialogResult was set to true
				Connector.Instance.CloseConnection();
				ConnectToServer();
			}
		}

		private void ExitMenuItem_Click(object sender, EventArgs e)
		{
			// Close the application
			_notifyIcon?.Dispose(); // Release the notify icon resource
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
			_hotkeyHook?.Dispose();
		}

		private void AddMenu_Click(object sender, RoutedEventArgs e)
		{
			EditMenuWindow dialog = new EditMenuWindow()
			{
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
			};

			if (dialog.ShowDialog() == true)
			{
				var menuName = dialog.ResultName;
				var newMenu = new Model.Menu(menuName, ViewModel.UserMenus.Count);
				ViewModel.UserMenus.Add(newMenu);
				ViewModel.SaveMenus();
				ViewModel.SelectedMenu = newMenu;
			}
		}

		private void DeleteMenu_Click(object sender, RoutedEventArgs e)
		{
			if (ViewModel.SelectedMenu != null)
			{
				ViewModel.UserMenus.Remove(ViewModel.SelectedMenu);
				ViewModel.SaveMenus();
				SwitchToMenu(0);
			}
		}

		private void EditMenu_Click(object sender, RoutedEventArgs e)
		{
			if (ViewModel.SelectedMenu == null)
			{
				return;
			}

			EditMenuWindow dialog = new EditMenuWindow(ViewModel.SelectedMenu.Name)
			{
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
			};

			if (dialog.ShowDialog() == true)
			{
				ViewModel.SelectedMenu.Name = dialog.ResultName;
				ViewModel.SaveMenus();
			}
		}

		private void MoveUp_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.MoveSelectedMenu(true);
		}
		private void MoveDown_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.MoveSelectedMenu(false);
		}

		private void AddSubenu_Click(object sender, RoutedEventArgs e)
		{
			if (ViewModel.SelectedMenu == null)
			{
				return;
			}

			EditMenuWindow dialog = new EditMenuWindow()
			{
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
			};

			if (dialog.ShowDialog() == true)
			{
				var menuName = dialog.ResultName;
				var newMenu = new Model.Submenu(menuName, ViewModel.SelectedMenu.Submenus.Count);
				ViewModel.SelectedMenu.Submenus.Add(newMenu);
				ViewModel.SelectedSubmenu = newMenu;
				ViewModel.SaveMenus();
			}
		}

		private Submenu? ExtractSubmenuContext(object sender)
		{
			var button = sender as SubmenuCreatorButton;
			return button?.Submenu;
		}

		private void SubmenuButton_Click(object sender, RoutedEventArgs e)
		{
			if (ExtractSubmenuContext(sender) is Submenu submenu)
			{
				ViewModel.SelectedSubmenu = submenu;
			}
		}

		private void SubmenuEdit_Click(object sender, EventArgs e)
		{
			if (ViewModel.SelectedMenu == null)
			{
				return;
			}

			if (ExtractSubmenuContext(sender) is Submenu submenu)
			{
				var subMenuIndex = ViewModel.SelectedMenu.Submenus.IndexOf(submenu);
				if (subMenuIndex < 0)
				{
					return;
				}

				EditMenuWindow dialog = new EditMenuWindow(submenu.Name)
				{
					Owner = this,
					WindowStartupLocation = WindowStartupLocation.CenterOwner,
				};

				if (dialog.ShowDialog() == true)
				{
					var menuName = dialog.ResultName;
					ViewModel.SelectedMenu.Submenus[subMenuIndex].Name = menuName;
					ViewModel.SaveMenus();
				}
			}
		}

		private void SubmenuDelete_Click(object sender, EventArgs e)
		{
			if (ViewModel.SelectedMenu == null)
			{
				return;
			}

			if (ExtractSubmenuContext(sender) is Submenu submenu)
			{
				ViewModel.SelectedMenu.Submenus.Remove(submenu);
				ViewModel.SelectedSubmenu = ViewModel.SelectedMenu.Submenus.FirstOrDefault();
				ViewModel.SaveMenus();
			}
		}

		private void SubmenuMove_Click(object sender, SubmenuCreatorButton.DirectionEventArg e)
		{
			if (ViewModel.SelectedMenu == null)
			{
				return;
			}

			if (ExtractSubmenuContext(sender) is Submenu submenu)
			{
				switch (e.Direction)
				{
					case SubmenuCreatorButton.Direction.Left:
						ViewModel.MoveSubmenu(submenu, true);
						break;
					case SubmenuCreatorButton.Direction.Right:
						ViewModel.MoveSubmenu(submenu, false);
						break;
				}
			}
		}

		private void SubmenuColor_Click(object sender, SubmenuCreatorButton.ColorItemEventArg e)
		{
			if (ViewModel.SelectedMenu == null)
			{
				return;
			}

			if (ExtractSubmenuContext(sender) is Submenu submenu)
			{
				var subMenuIndex = ViewModel.SelectedMenu.Submenus.IndexOf(submenu);

				if (subMenuIndex < 0)
				{
					return;
				}

				switch (e.Type)
				{
					case SubmenuCreatorButton.ColorType.Background:
						ViewModel.SelectedMenu.Submenus[subMenuIndex].BackgroundColor = e.Value;
						break;
					case SubmenuCreatorButton.ColorType.Font:
						ViewModel.SelectedMenu.Submenus[subMenuIndex].FontColor = e.Value;
						break;
				}

				ViewModel.SaveMenus();
			}
		}

		private void CreateControlElement_Click(object sender, EventArgs e)
		{
			if (ViewModel.SelectedMenu == null)
			{
				return;
			}

			ContextMenu contextMenu = new ContextMenu();

			var separatorItem = new MenuItem { Header = "Separator" };
			separatorItem.Click += (s, args) =>
			{
				ViewModel.EditMenuElement(new SeparatorElement() { Position = ViewModel.SelectedSubmenu.Items.Count });
			};
			contextMenu.Items.Add(separatorItem);

			foreach (var category in LocalData.Instance.AvailableCommands.Categories)
			{
				MenuItem categoryItem = new MenuItem { Header = category.Title };

				foreach (var command in category.Commands)
				{
					MenuItem commandItem = new MenuItem { Header = command.Title };
					commandItem.Click += (s, args) =>
					{
						ViewModel.EditMenuElement(new ActionElement { Command = command.CommandName, Position = ViewModel.SelectedSubmenu.Items.Count });
					};
					categoryItem.Items.Add(commandItem);
				}

				contextMenu.Items.Add(categoryItem);
			}

			contextMenu.PlacementTarget = sender as System.Windows.Controls.Button;
			contextMenu.IsOpen = true;
		}

		private void EditControlElement_Click(object sender, EventArgs e)
		{
			if (ViewModel.SelectedMenu == null)
			{
				return;
			}

			if ((sender as FrameworkElement)?.DataContext is MenuElement element)
			{
				var actionElement = element as ActionElement;

				ContextMenu contextMenu = new ContextMenu();

				MenuItem moveUpItem = new MenuItem { Header = "Move Up" };
				MenuItem moveDownItem = new MenuItem { Header = "Move Down" };
				moveUpItem.Click += (s, args) =>
				{
					ViewModel.MoveMenuItem(element, true);
				};
				contextMenu.Items.Add(moveUpItem);
				moveDownItem.Click += (s, args) =>
				{
					ViewModel.MoveMenuItem(element, false);
				};
				contextMenu.Items.Add(moveDownItem);

				if (actionElement != null)
				{
					foreach (var category in LocalData.Instance.AvailableCommands.Categories)
					{
						MenuItem categoryItem = new MenuItem { Header = category.Title };

						foreach (var command in category.Commands)
						{
							MenuItem commandItem = new MenuItem { Header = command.Title };
							commandItem.Click += (s, args) =>
							{
								actionElement.Command = command.CommandName;
								ViewModel.EditMenuElement(actionElement);
							};
							categoryItem.Items.Add(commandItem);
						}

						contextMenu.Items.Add(categoryItem);
					}
				}

				var deleteItem = new MenuItem { Header = "Delete", Background = Brushes.IndianRed };
				deleteItem.Click += (s, args) =>
				{
					ViewModel.SelectedSubmenu.Items.Remove(element);
					ViewModel.SaveMenus();
				};
				contextMenu.Items.Add(deleteItem);

				contextMenu.PlacementTarget = sender as System.Windows.Controls.Button;
				contextMenu.IsOpen = true;
			}
		}
	}
}