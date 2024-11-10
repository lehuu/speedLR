using System.Windows;
using System.Windows.Interop;
using Brushes = System.Windows.Media.Brushes;
using Application = System.Windows.Application;
using Point = System.Drawing.Point;
using SpeedLR.Model;
using System.Windows.Controls;
using SpeedLR.Controls;

namespace SpeedLR
{
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;

        private ControllerWindow _controller;
        private GlobalHotkey _activatorHotkey;
        private PortWindow? _portWindow;

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
            IsVisibleChanged += MainWindow_IsVisibleChanged;
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
            int numberOfMenus = 3;
            int numberOfButtons = 8;

            if (_menuButtons == null)
            {
                _menuButtons = new EmptyButton[numberOfMenus, numberOfButtons];
                for (int i = 0; i < numberOfMenus; i++)
                {
                    for (int j = 0; j < numberOfButtons; j++)
                    {
                        var currentMenu = i;
                        var currentButton = j;

                        EmptyButton button = new EmptyButton();
                        button.Margin = CircleCreator.CreateButtonsInCircle(buttonGrid, currentMenu, (float)currentButton / (float)numberOfButtons);
                        _menuButtons[currentMenu, currentButton] = button;
                        buttonGrid.Children.Add(button);
                    }
                }
            }

            for (int i = 0; i < numberOfMenus; i++)
            {
                for (int j = 0; j < numberOfButtons; j++)
                {
                    var currentMenu = i;
                    var currentButton = j;

                    EmptyButton button = _menuButtons[currentMenu, currentButton];
                    button.ResetEventsHandlers();

                    button.MenuItemClick += (s, args) =>
                    {
                        var menu = LocalData.Instance.AvailableMenus.Menus[menuIndex];
                        var existingIndex = menu?.Buttons.FindIndex(item => item.MenuIndex == currentMenu && item.ButtonIndex == currentButton);

                        var backgroundColor = ColorData.DEFAULT_BACKGROUND;
                        var fontColor = ColorData.DEFAULT_FONT;

                        if (existingIndex.HasValue && existingIndex.Value != -1)
                        {
                            backgroundColor = menu?.Buttons[existingIndex.Value].BackgroundColor;
                            fontColor = menu?.Buttons[existingIndex.Value].FontColor;
                            menu?.Buttons.RemoveAt(existingIndex.Value);
                        }
                        menu?.Buttons.Add(new CommandButton(args.Value, currentMenu, currentButton, backgroundColor, fontColor));

                        if (menu != null)
                        {
                            LocalData.Instance.AvailableMenus.UpdateMenu(menu);
                            LocalData.Instance.SaveAvailableMenus();
                        }
                    };

                    button.ClearClick += (s, args) =>
                    {
                        var menu = LocalData.Instance.AvailableMenus.Menus[menuIndex];
                        var existingIndex = menu?.Buttons.FindIndex(item => item.MenuIndex == currentMenu && item.ButtonIndex == currentButton);
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
                        var existingIndex = menu?.Buttons.FindIndex(item => item.MenuIndex == currentMenu && item.ButtonIndex == currentButton);

                        var backgroundColor = ColorData.DEFAULT_BACKGROUND;
                        var fontColor = ColorData.DEFAULT_FONT;

                        if (existingIndex.HasValue && existingIndex.Value != -1)
                        {
                            backgroundColor = menu?.Buttons[existingIndex.Value].BackgroundColor;
                            fontColor = menu?.Buttons[existingIndex.Value].FontColor;
                            menu?.Buttons.RemoveAt(existingIndex.Value);
                        }
                        menu?.Buttons.Add(new MenuButton(args, currentMenu, currentButton, backgroundColor, fontColor));

                        if (menu != null)
                        {
                            LocalData.Instance.AvailableMenus.UpdateMenu(menu);
                            LocalData.Instance.SaveAvailableMenus();
                        }
                    };

                    button.ColorItemClick += (s, args) =>
                    {
                        var menu = LocalData.Instance.AvailableMenus.Menus[menuIndex];
                        var existingIndex = menu?.Buttons.FindIndex(item => item.MenuIndex == currentMenu && item.ButtonIndex == currentButton);

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
                            menu?.Buttons.Add(new CommandButton(command, currentMenu, currentButton, backgroundColor, fontColor));
                        }
                        else
                        {
                            menu?.Buttons.Add(new MenuButton(submenu, currentMenu, currentButton, backgroundColor, fontColor));
                        }

                        if (menu != null)
                        {
                            LocalData.Instance.AvailableMenus.UpdateMenu(menu);
                            LocalData.Instance.SaveAvailableMenus();
                        }
                    };

                    var existingButton = selectedMenu.Buttons.FirstOrDefault(item => item.MenuIndex == currentMenu && item.ButtonIndex == currentButton);
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

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CheckConnection(sender, null);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ConnectToServer();

            // init context menu
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = new Icon("wheel.ico"); // Replace with your icon file
            _notifyIcon.Text = Title;
            _notifyIcon.Visible = true;

            // Create a context menu for the NotifyIcon
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += ExitMenuItem_Click;
            ToolStripMenuItem openMenuItem = new ToolStripMenuItem("Open");
            openMenuItem.Click += OpenMenuItem_Click;

            contextMenu.Items.Add(exitMenuItem);
            contextMenu.Items.Add(openMenuItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.MouseDoubleClick += OpenMenuItem_Click;

            _controller = new ControllerWindow();

            var helper = new WindowInteropHelper(this);

            _activatorHotkey = new GlobalHotkey(helper.Handle, 1, GlobalHotkey.MOD_CONTROL);
            _activatorHotkey.Register(HwndHook);
            _activatorHotkey.HotKeyDoublePressed += Ctrl_DoublePressed;
        }

        private async void CheckConnection(object sender, EventArgs? e)
        {
            var isConnected = await Connector.Instance.IsConnected();
            if (isConnected)
            {
                this.connectButton.Background = Brushes.Green;
                this.connectButton.Content = "Connected";
            }
            else
            {
                this.connectButton.Background = Brushes.Red;
                this.connectButton.Content = "Reconnect";
            }
        }

        private async void ConnectToServer()
        {
            this.portButton.Content = "Port: " + LocalData.Instance.Port;

            try
            {
                this.connectButton.Content = "Connecting...";
                this.connectButton.Background = Brushes.Blue;

                await Connector.Instance.Connect(LocalData.Instance.Port);

                this.connectButton.Background = Brushes.Green;
                this.connectButton.Content = "Connected";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                this.connectButton.Background = Brushes.Red;
                this.connectButton.Content = "Reconnect";
            }
        }

        private void Ctrl_DoublePressed(object sender, EventArgs e)
        {
            if (_controller.IsVisible)
            {
                _controller.Hide();
            }
            else
            {
                if (_controller.IsPinned)
                {
                    _controller.Show();
                    return;
                }
                var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                Point mousePosition = System.Windows.Forms.Control.MousePosition;

                var mouse = transform.Transform(new System.Windows.Point(mousePosition.X, mousePosition.Y));

                Screen screen = Screen.FromPoint(mousePosition);

                var formWidth = _controller.Width;
                var formHeight = _controller.Height;

                double x = mouse.X - formWidth / 2;
                double y = mouse.Y - formHeight / 2;

                var topLeft = transform.Transform(new System.Windows.Point(screen.WorkingArea.Left, screen.WorkingArea.Top));
                var bottomRight = transform.Transform(new System.Windows.Point(screen.WorkingArea.Right, screen.WorkingArea.Bottom));
                x = Math.Max(topLeft.X, Math.Min(x, bottomRight.X - formWidth));
                y = Math.Max(topLeft.Y, Math.Min(y, bottomRight.Y - formHeight));

                _controller.Left = x;
                _controller.Top = y;
                _controller.Show();
            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var isConnected = await Connector.Instance.IsConnected();
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

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_activatorHotkey != null)
            {
                _activatorHotkey.ProcessWindowMessage(hwnd, msg, wParam, lParam, ref handled); // Pass the message to the GlobalHotkey class
            }
            return IntPtr.Zero;
        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        protected override void OnClosed(EventArgs e)
        {
            Connector.Instance.CloseConnection();
            _activatorHotkey.Unregister(HwndHook);
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