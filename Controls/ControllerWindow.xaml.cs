using System.Timers;
using System.Windows;
using System.Windows.Interop;
using SpeedLR.Controls;
using SpeedLR.Model;
using Timer = System.Timers.Timer;

namespace SpeedLR
{

    public partial class ControllerWindow : Window
    {
        private List<string> _menuHistory = new List<string>();
        private LRControlButton[] _stepButtons;
        private ControlButton[][] _menus;
        private GlobalHotkey[] _hotkeys;
        private GlobalHotkey[] _commandHotkeys;
        private GlobalMouseHook _mouseHook;
        private readonly Timer _hideTimer;

        private string _currentCommand = "";
        private int _currentButtonIndex = -1;
        private int _currentMenuIndex = 0;

        private string _currentMenuId = "";

        private enum CommandType
        {
            UP,
            DOWN,
            RESET
        }

        public ControllerWindow()
        {
            InitializeComponent();
            IsVisibleChanged += ControllerWindow_IsVisibleChanged;
            DpiHelper.AdjustScaleForDpi(this);
            SourceInitialized += (s, e) => this.DpiChanged += Window_DpiChanged;
            _hideTimer = new Timer(500);
            _hideTimer.AutoReset = false;
            _hideTimer.Elapsed += OnHideElapsed;
            _stepButtons = new LRControlButton[] {
                lowStepButton,
                midStepButton,
                highStepButton
            };
        }

        public bool IsPinned
        {
            get { return pinButton.IsActive; }
            set
            {
                pinButton.IsActive = value;
            }
        }

        private string CurrentCommand
        {
            get => _currentCommand;
            set
            {
                if (_currentCommand != value)
                {
                    _currentCommand = value;

                    if (String.IsNullOrEmpty(value))
                    {
                        _mouseHook.Dispose();
                        DeactivateCommandKeys();
                    }
                    else
                    {
                        _mouseHook.Register();
                        ActivateCommandKeys();
                    }
                }
            }
        }

        private void SwitchToMenu(int menuIndex)
        {
            if (menuIndex < 0 || menuIndex >= LocalData.Instance.AvailableMenus.Menus.Count || !IsVisible)
            {
                return;
            }

            int maxNumberOfButtons = 8;

            buttonGrid.Children.Clear();

            var selectedMenu = LocalData.Instance.AvailableMenus.Menus[menuIndex];
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

            var distinctMenus = filteredButtons.Select(button => button.MenuIndex).Distinct().OrderBy(index => index).ToArray();
            int menuNumbers = distinctMenus.Count();
            var isConnected = Connector.Instance.IsConnected().Result;

            _menus = new ControlButton[menuNumbers][];
            for (int i = 0; i < menuNumbers; i++)
            {
                var menuButtons = filteredButtons.Where(button => button.MenuIndex == distinctMenus[i]).ToArray();
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

                    button.Click += Button_Click;
                    button.Margin = CircleCreator.CreateButtonsInCircle(buttonGrid, distinctMenus[i], (float)menuButton.ButtonIndex / (float)maxNumberOfButtons);
                    button.Style = (Style) FindResource("LargeControlButton");
                    _menus[i][j] = button;
                    buttonGrid.Children.Add(button);
                }

            }
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

        private void ControllerWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(_currentMenuId))
            {
                var startMenuIndex = LocalData.Instance.AvailableMenus.Menus.FindIndex(m => m.Id == LocalData.Instance.AvailableMenus.DefaultMenu);
                SwitchToMenu(startMenuIndex == -1 ? 0 : startMenuIndex);
                _menuHistory.Add(_currentMenuId);
            }
            else
            {
                SwitchToMenu(_currentMenuId);
            }

            if (!(_hotkeys?.Length > 0))
            {
                _hotkeys = new GlobalHotkey[]
                {
                    CreateHotkey(3, 0, GlobalHotkey.RIGHT, Next_Pressed),
                    CreateHotkey(4, 0, GlobalHotkey.LEFT, Prev_Pressed),
                    CreateHotkey(8, GlobalHotkey.MOD_ALT, GlobalHotkey.RIGHT, Next_Submenu),
                    CreateHotkey(9, GlobalHotkey.MOD_ALT, GlobalHotkey.LEFT, Prev_Submenu),
                    CreateHotkey(10, GlobalHotkey.MOD_ALT, GlobalHotkey.UP, Increase_Step),
                    CreateHotkey(11, GlobalHotkey.MOD_ALT, GlobalHotkey.DOWN, Decrease_Step),
                };

                _commandHotkeys = new GlobalHotkey[]
                {
                    CreateHotkey(2, 0, GlobalHotkey.ESCAPE, Escape_Pressed),
                    CreateHotkey(5, 0, GlobalHotkey.UP, Inc_Pressed),
                    CreateHotkey(6, 0, GlobalHotkey.DOWN, Dec_Pressed),
                    CreateHotkey(7, 0, GlobalHotkey.SPACE, Reset_Pressed),
                };

                _mouseHook = new GlobalMouseHook();
                _mouseHook.OnMouseScrollUp += HandleGlobalScrollUp;
                _mouseHook.OnMouseScrollDown += HandleGlobalScrollDown;
                _mouseHook.OnMiddleMouseButtonClick += Reset_Pressed;
            }

            if (IsVisible)
            {
                ActivateHotkeys();
                if (!String.IsNullOrWhiteSpace(CurrentCommand))
                {
                    ActivateCommandKeys();
                    _mouseHook.Register();
                }

                var isConnected = Connector.Instance.IsConnected().Result;

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
            else
            {
                DeactivateHotkeys();
                DeactivateCommandKeys();
                _mouseHook.Dispose();
            }
        }
        private void Window_DpiChanged(object sender, System.Windows.DpiChangedEventArgs e)
        {
            DpiHelper.AdjustScaleForDpi(this);
        }

        private GlobalHotkey CreateHotkey(int id, int modifier, int key, EventHandler clickEvent)
        {
            var helper = new WindowInteropHelper(this);
            var result = new GlobalHotkey(helper.Handle, id, modifier, key);
            result.HotKeyPressed += clickEvent;
            return result;
        }

        private void SendCommand(CommandType type)
        {
            if (!IsVisible)
            {
                return;
            }
            if (String.IsNullOrEmpty(CurrentCommand))
            {
                return;
            }

            Opacity = 0;

            var stepSize = _stepButtons.First(item => item.IsActive)?.LRCommand;
            stepSize = String.IsNullOrEmpty(stepSize) ? "1%" : stepSize;

            switch (type)
            {
                case CommandType.DOWN:
                    Connector.Instance.SendCommandAsync(CurrentCommand + "=-" + stepSize);
                    break;
                case CommandType.UP:
                    Connector.Instance.SendCommandAsync(CurrentCommand + "=+" + stepSize);
                    break;
                case CommandType.RESET:
                    Connector.Instance.SendCommandAsync(CurrentCommand + "=reset");
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
            CurrentCommand = "";
        }

        private void ToggleButton(int menu, int key)
        {
            ClearActiveButtons();
            var item = _menus[menu][key];

            item.IsActive = true;
            _currentMenuIndex = menu;
            _currentButtonIndex = key;
            if (item is LRControlButton lrControlButton)
            {
                CurrentCommand = String.IsNullOrEmpty(lrControlButton.LRCommand) ? "" : lrControlButton.LRCommand;
            }
        }

        private void ToggleButton(ControlButton button)
        {
            for (int i = 0; i < _menus.Length; i++)
            {
                for (int j = 0; j < _menus[i].Length; j++)
                {
                    var item = _menus[i][j];
                    if (button != null && item.Name == button.Name)
                    {
                        item.IsActive = !item.IsActive;
                        if (item is LRControlButton lrControlButton)
                        {
                            CurrentCommand = lrControlButton.IsActive ? lrControlButton.LRCommand : "";
                        }
                        _currentButtonIndex = j;
                        _currentMenuIndex = i;
                        continue;
                    }

                    item.IsActive = false;
                }
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            foreach (var key in _hotkeys)
            {
                key.ProcessWindowMessage(hwnd, msg, wParam, lParam, ref handled);
            }
            foreach (var key in _commandHotkeys)
            {
                key.ProcessWindowMessage(hwnd, msg, wParam, lParam, ref handled);
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            DeactivateHotkeys();
            DeactivateCommandKeys();
            _mouseHook?.Dispose();
        }

        private void ActivateCommandKeys()
        {
            foreach (var key in _commandHotkeys)
            {
                key.Register(HwndHook);
            }
        }

        private void DeactivateCommandKeys()
        {
            foreach (var key in _commandHotkeys)
            {
                key.Unregister(HwndHook);
            }
        }

        private void ActivateHotkeys()
        {
            foreach (var key in _hotkeys)
            {
                key.Register(HwndHook);
            }
        }

        private void DeactivateHotkeys()
        {
            foreach (var key in _hotkeys)
            {
                key.Unregister(HwndHook);
            }
        }
    }
}
