﻿using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Interop;
using SpeedLR.Controls;
using SpeedLR.Model;
using Timer = System.Timers.Timer;
using System.Diagnostics;

namespace SpeedLR
{

    public partial class ControllerWindow : Window, INotifyPropertyChanged
    {
        internal enum ButtonType
        {
            MENU,
            LR,
            NONE
        }
        private enum CommandType
        {
            UP,
            DOWN,
            RESET
        }

        internal class ButtonData
        {
            public string Data;
            public ButtonType Type;

            public ButtonData(string data, ButtonType type)
            {
                Data = data;
                Type = type;
            }
        }

        private LRControlButton[] _stepButtons;
        private ControlButton[][] _menus;
        private GlobalHotkey[] _hotkeys;
        private GlobalMouseHook _mouseHook;
        private readonly Timer _hideTimer;
        private ActiveWindowWatcher _watcher = new ActiveWindowWatcher();

        private int _currentButtonIndex = -1;
        private int _currentMenuIndex = 0;

        private string _currentMenuId = "";

        public ControllerWindow()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
            IsVisibleChanged += ControllerWindow_IsVisibleChanged;
            _hideTimer = new Timer(500);
            _hideTimer.AutoReset = false;
            _hideTimer.Elapsed += OnHideElapsed;
            _stepButtons = new LRControlButton[] {
                lowStepButton,
                midStepButton,
                highStepButton
            };
            this.DataContext = this;
        }

        public bool IsPinned
        {
            get { return pinButton.IsActive; }
            set
            {
                pinButton.IsActive = value;
            }
        }

        private bool _inEditMode;

        public bool InEditMode
        {
            get => _inEditMode;
            set
            {
                if (_inEditMode != value)
                {
                    _inEditMode = value;
                    OnPropertyChanged(nameof(InEditMode));
                }
                UpdateMouseHook();
            }
        }

        public int CurrentButtonIndex
        {
            get => _currentButtonIndex;
            set
            {
                _currentButtonIndex = value;
                UpdateMouseHook();
            }
        }
        public int CurrentMenuIndex
        {
            get => _currentMenuIndex;
            set
            {
                _currentMenuIndex = value;
                UpdateMouseHook();
            }
        }

        private ButtonData CurrentButton
        {
            get
            {
                if (CurrentMenuIndex < 0 || CurrentButtonIndex < 0 || CurrentMenuIndex >= _menus.Length || CurrentButtonIndex >= _menus[CurrentMenuIndex].Length)
                {
                    return new ButtonData("", ButtonType.NONE);
                }
                var button = _menus[CurrentMenuIndex][CurrentButtonIndex];

                if (!button.IsActive)
                {
                    return new ButtonData("", ButtonType.NONE);
                }

                if (button is LRControlButton lrControlButton)
                {
                    return new ButtonData(lrControlButton.LRCommand, ButtonType.LR);
                }

                if (button is MenuControlButton menuControlButton)
                {
                    return new ButtonData(menuControlButton.MenuCommand, ButtonType.MENU);
                }

                return new ButtonData("", ButtonType.NONE);
            }
        }

        private void UpdateMouseHook()
        {
            if (String.IsNullOrEmpty(CurrentButton.Data) || !InEditMode)
            {
                _mouseHook?.Dispose();
            }
            else
            {
                _mouseHook?.Register();
            }
        }

        private void SwitchToMenu(int menuIndex)
        {
            InEditMode = false;
            if (menuIndex < 0 || menuIndex >= LocalData.Instance.AvailableMenus.Menus.Count || !IsVisible)
            {
                return;
            }

            buttonGrid.Children.Clear();

            var selectedMenu = LocalData.Instance.AvailableMenus.Menus[menuIndex];
            menuTitle.Text = selectedMenu.Name;

            var filteredButtons = selectedMenu.Buttons.Where(b =>
            {
                if (b is CommandButton)
                {
                    return !String.IsNullOrEmpty(((CommandButton)b).Command?.CommandName);
                }

                if (b is MenuButton)
                {
                    return !String.IsNullOrEmpty(((MenuButton)b).Submenu);
                }
                return false;
            }).ToArray();

            _currentMenuId = selectedMenu.Id;

            var distinctMenus = filteredButtons.Select(button => button.Row).Distinct().OrderBy(index => index).ToArray();
            int menuNumbers = distinctMenus.Count();
            var isConnected = Connector.Instance.Status == Connector.ConnectionStatus.CONNECTED;

            _menus = new ControlButton[menuNumbers][];
            for (int i = 0; i < menuNumbers; i++)
            {
                var menuButtons = filteredButtons.Where(button => button.Row == distinctMenus[i]).OrderBy(b => b.Col).ToArray();
                if (menuButtons.Count() == 0)
                { continue; }

                _menus[i] = new ControlButton[menuButtons.Count()];
                for (int j = 0; j < menuButtons.Count(); j++)
                {
                    var menuButton = menuButtons[j];

                    ControlButton button = null;

                    if (menuButton is CommandButton lrMenuButton)
                    {
                        button = new LRControlButton(lrMenuButton.Command);
                        button.IsEnabled = isConnected;
                    }
                    else if (menuButton is MenuButton menuControlButton)
                    {
                        var menu = LocalData.Instance.AvailableMenus.Menus.FirstOrDefault(m => m.Id == menuControlButton.Submenu);
                        if (menu == null)
                        {
                            continue;
                        }
                        button = new MenuControlButton(menu);
                    }

                    if (button == null)
                    {
                        continue;
                    }

                    button.Background = BrushHelper.GetBrushFromHex(menuButton.BackgroundColor);
                    button.Foreground = BrushHelper.GetBrushFromHex(menuButton.FontColor);
                    button.Click += Button_Click;
                    button.Margin = GridCreator.Create(buttonGrid, menuButton.Col, menuButton.Row);
                    button.Style = (Style)FindResource("GridControlButton");

                    _menus[i][j] = button;
                    buttonGrid.Children.Add(button);
                }
            }

            ToggleButton(-1, -1);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            int exStyle = (int)GetWindowLong(helper.Handle, GWL_EXSTYLE);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, exStyle | WS_EX_NOACTIVATE);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

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
        }

        private void UpdateHooksAndControls()
        {
            var isConnected = Connector.Instance.Status == Connector.ConnectionStatus.CONNECTED;
            var isLightroomActive = _watcher.IsLightroomActive();

            InEditMode = false;

            if (!IsVisible || !isLightroomActive || !isConnected)
            {
                DeactivateHotkeys();

                if (_mouseHook != null)
                    _mouseHook.Dispose();
            }
            else
            {
                ActivateHotkeys();
            }

            if (String.IsNullOrEmpty(_currentMenuId))
            {
                var startMenuIndex = LocalData.Instance.AvailableMenus.Menus.FindIndex(m => m.Id == LocalData.Instance.AvailableMenus.DefaultMenu);
                SwitchToMenu(startMenuIndex == -1 ? 0 : startMenuIndex);
            }
            else
            {
                SwitchToMenu(_currentMenuId);
            }

            foreach (var submenu in _menus)
            {
                foreach (var item in submenu)
                {
                    if (item is LRControlButton)
                    {
                        item.IsEnabled = isConnected;
                    }
                    else
                    {
                        item.IsEnabled = true;
                    }
                }
            }
        }

        private void OnConnectionChanged(object sender, Connector.ConnectionStatus status)
        {
            Dispatcher.Invoke(() => UpdateHooksAndControls());
        }

        private void ControllerWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(_hotkeys?.Length > 0))
            {
                _hotkeys = new GlobalHotkey[]
                {
                    CreateHotkey(2, 0, (int) Keys.Escape, Escape_Pressed),
                    CreateHotkey(3, 0, (int) Keys.Right, Right_Pressed),
                    CreateHotkey(4, 0, (int) Keys.Left, Left_Pressed),
                    CreateHotkey(5, 0, (int) Keys.Up, Up_Pressed),
                    CreateHotkey(6, 0, (int) Keys.Down, Down_Pressed),
                    CreateHotkey(7, 0, (int) Keys.Back, Back_Pressed),
                    CreateHotkey(8, 0, (int) Keys.Return, Enter_Pressed),
                };

                _mouseHook = new GlobalMouseHook();
                _mouseHook.OnMouseDragRight += HandleGlobalScrollUp;
                _mouseHook.OnMouseScrollUp += HandleGlobalScrollUp;
                _mouseHook.OnMouseDragLeft += HandleGlobalScrollDown;
                _mouseHook.OnMouseScrollDown += HandleGlobalScrollDown;
                _mouseHook.OnMiddleMouseButtonClick += Reset_Pressed;
                _mouseHook.OnMouseClickDown += IsClickInWindow;
                _mouseHook.OnMouseClickUp += IsClickInWindow;
            }

            UpdateHooksAndControls();
        }

        private GlobalHotkey CreateHotkey(int id, int modifier, int key, EventHandler clickEvent)
        {
            var helper = new WindowInteropHelper(this);
            var result = new GlobalHotkey(helper.Handle, id, modifier, key);
            result.HotKeyPressed += clickEvent;
            return result;
        }

        private bool IsClickInWindow(int x, int y)
        {
            // Scale the mouse coordinates according to DPI
            double scale = DpiHelper.GetWindowScale(this);
            int scaledX = (int)(x / scale);
            int scaledY = (int)(y / scale);
            return !(Left <= scaledX && (Left + ActualWidth >= scaledX)) || !(Top <= scaledY && (Top + ActualHeight >= scaledY));
        }

        private void SendCommand(CommandType type)
        {
            if (!IsVisible)
            {
                return;
            }
            if (String.IsNullOrEmpty(CurrentButton.Data) || CurrentButton.Type != ButtonType.LR)
            {
                return;
            }
            if (!_watcher.IsLightroomActive())
            {
                return;
            }


            Opacity = 0;

            var stepSize = _stepButtons.FirstOrDefault(item => item.IsActive)?.LRCommand;
            stepSize = String.IsNullOrEmpty(stepSize) ? "1%" : stepSize;

            switch (type)
            {
                case CommandType.DOWN:
                    Connector.Instance.SendCommandAsync(CurrentButton.Data + "=-" + stepSize);
                    break;
                case CommandType.UP:
                    Connector.Instance.SendCommandAsync(CurrentButton.Data + "=+" + stepSize);
                    break;
                case CommandType.RESET:
                    Connector.Instance.SendCommandAsync(CurrentButton.Data + "=reset");
                    break;
            }

            _hideTimer.Stop();
            _hideTimer.Start();
        }

        private void OnHideElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Opacity = 1;
            });
        }

        private void ControllerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void ClearActiveButtons()
        {
            for (int i = 0; i < _menus.Length; i++)
            {
                for (int j = 0; j < _menus[i].Length; j++)
                {
                    var item = _menus[i][j];
                    item.IsActive = false;
                }
            }
        }

        private void ToggleButton(int menu, int key)
        {
            if (menu < 0 || key < 0 || menu >= _menus.Length || key >= _menus[menu].Length)
            {
                popup.Visibility = Visibility.Collapsed;
                ClearActiveButtons();
                return;
            }

            var wasActive = _menus[menu][key].IsActive;
            ClearActiveButtons();
            var item = _menus[menu][key];
            item.IsActive = !wasActive;
            CurrentMenuIndex = menu;
            CurrentButtonIndex = key;

            popup.Visibility = item.IsActive ? Visibility.Visible : Visibility.Collapsed;
            popup.Text = item.PopupText;
            popup.Margin = new Thickness(
                item.Margin.Left,
                item.Margin.Top + item.Height,
                item.Margin.Right,
                item.Margin.Bottom
            );
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            foreach (var key in _hotkeys)
            {
                key.ProcessWindowMessage(hwnd, msg, wParam, lParam, ref handled);
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            DeactivateHotkeys();
            _mouseHook?.Dispose();
        }

        private void ActivateHotkeys()
        {
            if (_hotkeys == null)
            {
                return;
            }
            foreach (var key in _hotkeys)
            {
                key.Register(HwndHook);
            }
        }

        private void DeactivateHotkeys()
        {
            if (_hotkeys == null)
            {
                return;
            }
            foreach (var key in _hotkeys)
            {
                key.Unregister(HwndHook);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
