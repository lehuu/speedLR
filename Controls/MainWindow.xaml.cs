using System.Windows;
using System.Windows.Interop;
using Brushes = System.Windows.Media.Brushes;
using Application = System.Windows.Application;
using Point = System.Drawing.Point;
using SpeedLR.Model;
using System.Windows.Controls;
using SpeedLR.Controls;
using System.Windows.Input;

namespace SpeedLR
{
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;

        private ControllerWindow _controller;
        private LowLevelHotkey _hotkeyHook;
        private PortWindow? _portWindow;
        private ActiveWindowWatcher _watcher = new ActiveWindowWatcher();

        private EmptyButton[,] _menuButtons;
        private string _currentMenuId = "";
        private bool _isSwitching = false;

        public MainWindow()
        {
            InitializeComponent();
            var startMenuIndex = LocalData.Instance.AvailableMenus.Menus.FindIndex(m => m.Id == LocalData.Instance.AvailableMenus.DefaultMenu);
            SwitchToMenu(startMenuIndex == -1 ? 0 : startMenuIndex);

            Loaded += (s, e) =>
            {
                Hide();
            };
            menuTextbox.DebouncedTextChanged += TextBoxChanged;
        }

        private void SwitchToMenu(int menuIndex)
        {
            _isSwitching = true;
            if (menuIndex < 0 || menuIndex >= LocalData.Instance.AvailableMenus.Menus.Count)
            {
                return;
            }

            var selectedMenu = LocalData.Instance.AvailableMenus.Menus[menuIndex];
            _currentMenuId = selectedMenu.Id;

            defaultCheckBox.IsChecked = LocalData.Instance.AvailableMenus.DefaultMenu == selectedMenu.Id;
            menuTextbox.Text = selectedMenu.Name;

            if (_menuButtons == null)
            {
                _menuButtons = new EmptyButton[GridCreator.MAX_ROWS, GridCreator.MAX_COLS];
                for (int i = 0; i < GridCreator.MAX_ROWS; i++)
                {
                    for (int j = 0; j < GridCreator.MAX_COLS; j++)
                    {
                        var currentRow = i;
                        var currentCol = j;

                        EmptyButton button = new EmptyButton();
                        button.Margin = GridCreator.Create(buttonGrid, currentCol, currentRow);
                        _menuButtons[currentRow, currentCol] = button;

                        buttonGrid.Children.Add(button);
                    }
                }
            }

            for (int i = 0; i < GridCreator.MAX_ROWS; i++)
            {
                for (int j = 0; j < GridCreator.MAX_COLS; j++)
                {
                    var currentRow = i;
                    var currentCol = j;

                    EmptyButton button = _menuButtons[currentRow, currentCol];
                    button.ResetEventsHandlers();

                    button.MenuItemClick += (s, args) =>
                    {
                        var menu = LocalData.Instance.AvailableMenus.Menus[menuIndex];
                        var existingIndex = menu?.Buttons.FindIndex(item => item.Row == currentRow && item.Col == currentCol);

                        var backgroundColor = ColorData.DEFAULT_BACKGROUND;
                        var fontColor = ColorData.DEFAULT_FONT;

                        if (existingIndex.HasValue && existingIndex.Value != -1)
                        {
                            backgroundColor = menu?.Buttons[existingIndex.Value].BackgroundColor;
                            fontColor = menu?.Buttons[existingIndex.Value].FontColor;
                            menu?.Buttons.RemoveAt(existingIndex.Value);
                        }
                        menu?.Buttons.Add(new CommandButton(args.Value, currentRow, currentCol, backgroundColor, fontColor));

                        if (menu != null)
                        {
                            LocalData.Instance.AvailableMenus.UpdateMenu(menu);
                            LocalData.Instance.SaveAvailableMenus();
                        }
                    };

                    button.ClearClick += (s, args) =>
                    {
                        var menu = LocalData.Instance.AvailableMenus.Menus[menuIndex];
                        var existingIndex = menu?.Buttons.FindIndex(item => item.Row == currentRow && item.Col == currentCol);
                        if (existingIndex.HasValue && existingIndex.Value != -1 && menu != null)
                        {
                            menu.Buttons.RemoveAt(existingIndex.Value);
                            LocalData.Instance.AvailableMenus.UpdateMenu(menu);
                            LocalData.Instance.SaveAvailableMenus();
                        }
                    };

                    button.SubmenuItemClick += (s, args) =>
                    {
                        var menu = LocalData.Instance.AvailableMenus.Menus[menuIndex];
                        var existingIndex = menu?.Buttons.FindIndex(item => item.Row == currentRow && item.Col == currentCol);

                        var backgroundColor = ColorData.DEFAULT_BACKGROUND;
                        var fontColor = ColorData.DEFAULT_FONT;

                        if (existingIndex.HasValue && existingIndex.Value != -1)
                        {
                            backgroundColor = menu?.Buttons[existingIndex.Value].BackgroundColor;
                            fontColor = menu?.Buttons[existingIndex.Value].FontColor;
                            menu?.Buttons.RemoveAt(existingIndex.Value);
                        }
                        menu?.Buttons.Add(new MenuButton(args, currentRow, currentCol, backgroundColor, fontColor));

                        if (menu != null)
                        {
                            LocalData.Instance.AvailableMenus.UpdateMenu(menu);
                            LocalData.Instance.SaveAvailableMenus();
                        }
                    };

                    button.ColorItemClick += (s, args) =>
                    {
                        var menu = LocalData.Instance.AvailableMenus.Menus[menuIndex];
                        var existingIndex = menu?.Buttons.FindIndex(item => item.Row == currentRow && item.Col == currentCol);

                        var backgroundColor = ColorData.DEFAULT_BACKGROUND;
                        var fontColor = ColorData.DEFAULT_FONT;
                        Model.Command command = null;
                        string submenu = "";
                        bool isCommandButton = true;

                        if (existingIndex.HasValue && existingIndex.Value != -1)
                        {
                            var existingButton = menu?.Buttons[existingIndex.Value];

                            if (existingButton is CommandButton castCommandButton)
                            {
                                command = castCommandButton.Command;
                            }
                            else if (existingButton is MenuButton castMenuButton)
                            {
                                submenu = castMenuButton.Submenu;
                                isCommandButton = false;
                            }
                            backgroundColor = menu?.Buttons[existingIndex.Value].BackgroundColor;
                            fontColor = menu?.Buttons[existingIndex.Value].FontColor;
                            menu?.Buttons.RemoveAt(existingIndex.Value);
                        }

                        switch (args.Type)
                        {
                            case EmptyButton.ColorType.Background:
                                backgroundColor = args.Value;
                                break;
                            case EmptyButton.ColorType.Font:
                                fontColor = args.Value;
                                break;
                        }

                        if (isCommandButton)
                        {
                            menu?.Buttons.Add(new CommandButton(command, currentRow, currentCol, backgroundColor, fontColor));
                        }
                        else
                        {
                            menu?.Buttons.Add(new MenuButton(submenu, currentRow, currentCol, backgroundColor, fontColor));
                        }

                        if (menu != null)
                        {
                            LocalData.Instance.AvailableMenus.UpdateMenu(menu);
                            LocalData.Instance.SaveAvailableMenus();
                        }
                    };

                    var existingButton = selectedMenu.Buttons.FirstOrDefault(item => item.Row == currentRow && item.Col == currentCol);
                    if (existingButton != null)
                    {
                        if (existingButton is CommandButton castCommandButton)
                        {
                            button.Command = castCommandButton.Command;
                        }
                        else if (existingButton is MenuButton castMenuButton)
                        {
                            var submenuId = castMenuButton.Submenu;
                            var existingSubmenu = LocalData.Instance.AvailableMenus.Menus.FirstOrDefault(item => item.Id == submenuId);
                            if (existingSubmenu != null)
                            {
                                button.Submenu = existingSubmenu;
                            }
                        }
                        button.Background = BrushHelper.GetBrushFromHex(existingButton.BackgroundColor);
                        button.Foreground = BrushHelper.GetBrushFromHex(existingButton.FontColor);
                    }
                    else
                    {
                        button.Command = null;
                        button.Background = BrushHelper.GetBrushFromHex(ColorData.DEFAULT_BACKGROUND);
                        button.Foreground = BrushHelper.GetBrushFromHex(ColorData.DEFAULT_FONT);
                    }
                }
            }
            _isSwitching = false;
        }

        private void SwitchToMenu(string menuId)
        {
            var index = LocalData.Instance.AvailableMenus.Menus.FindIndex(m => m.Id == menuId);

            if (index == -1)
            {
                return;
            }

            SwitchToMenu(index);
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

            if (!_watcher.IsLightroomActive())
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

        private void TextBoxChanged(object sender, EventArgs e)
        {
            var item = LocalData.Instance.AvailableMenus.Menus.FirstOrDefault(m => m.Id == _currentMenuId);

            if (item == null || item.Name == menuTextbox.Text)
            {
                return;
            }

            item.Name = menuTextbox.Text;
            LocalData.Instance.AvailableMenus.UpdateMenu(item);
            LocalData.Instance.SaveAvailableMenus();
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

        private void MenuDropdown_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();

            LocalData.Instance.AvailableMenus.Menus.ForEach(m =>
            {
                MenuItem menuItem = new MenuItem { Header = m.Name };

                menuItem.Click += (s, args) =>
                {
                    SwitchToMenu(m.Id);
                };
                contextMenu.Items.Add(menuItem);
            });

            contextMenu.PlacementTarget = menuDropdown;
            contextMenu.IsOpen = true;
        }

        private void MenuDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var indexToDelete = LocalData.Instance.AvailableMenus.Menus.FindIndex(m => m.Id == _currentMenuId);

            if (indexToDelete != -1)
            {
                LocalData.Instance.AvailableMenus.Menus.RemoveAt(indexToDelete);
                LocalData.Instance.SaveAvailableMenus();
                SwitchToMenu(0);
            }
        }

        private void MenuAddButton_Click(object sender, RoutedEventArgs e)
        {
            var menuName = $"Menu_{LocalData.Instance.AvailableMenus.Menus.Count}";
            var newMenu = new Model.Menu(menuName);
            LocalData.Instance.AvailableMenus.UpdateMenu(newMenu);
            LocalData.Instance.SaveAvailableMenus();
            SwitchToMenu(newMenu.Id);
        }

        private void DefaultCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_isSwitching)
            {
                return;
            }

            LocalData.Instance.AvailableMenus.DefaultMenu = (defaultCheckBox.IsChecked.HasValue && defaultCheckBox.IsChecked.Value) ? _currentMenuId : "";
            LocalData.Instance.SaveAvailableMenus();
        }
    }
}