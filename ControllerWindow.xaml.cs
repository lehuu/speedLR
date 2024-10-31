using System.Windows;
using System.Windows.Interop;

namespace SpeedLR
{

    public partial class ControllerWindow : Window
    {
        private ControlButton[][] _menus;
        private GlobalHotkey[] _hotkeys;
        private GlobalMouseHook _mouseHook;

        private string _currentCommand = "";
        private int _currentButtonIndex = -1;
        private int _currentMenuIndex = 0;

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
        }

        private void ControllerWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(_hotkeys?.Length > 0))
            {
                var firstMenu = new ControlButton[]
                {
                    exposure,
                    shadows,
                    highlights
                };

                var secondMenu = new ControlButton[]
                {
                    blacks,
                    whites,
                };

                _menus = new ControlButton[2][];
                _menus[0] = firstMenu;
                _menus[1] = secondMenu;


                _hotkeys = new GlobalHotkey[]
                {
                CreateHotkey(2, 0, GlobalHotkey.ESCAPE, Escape_Pressed),
                CreateHotkey(3, 0, GlobalHotkey.RIGHT, Next_Pressed),
                CreateHotkey(4, 0, GlobalHotkey.LEFT, Prev_Pressed),
                CreateHotkey(5, 0, GlobalHotkey.UP, Inc_Pressed),
                CreateHotkey(6, 0, GlobalHotkey.DOWN, Dec_Pressed),
                CreateHotkey(7, 0, GlobalHotkey.SPACE, Reset_Pressed),
                CreateHotkey(8, GlobalHotkey.MOD_ALT, GlobalHotkey.RIGHT, Next_Submenu),
                CreateHotkey(9, GlobalHotkey.MOD_ALT, GlobalHotkey.LEFT, Prev_Submenu),
                };

                _mouseHook = new GlobalMouseHook();
                _mouseHook.OnMouseScrollUp += HandleGlobalScrollUp;
                _mouseHook.OnMouseScrollDown += HandleGlobalScrollDown;
                _mouseHook.OnMiddleMouseButtonClick += Reset_Pressed;
            }

            if (IsVisible)
            {
                foreach (var key in _hotkeys)
                {
                    key.Register(HwndHook);
                }
                _mouseHook.Register();
            }
            else
            {
                foreach (var key in _hotkeys)
                {
                    key.Unregister(HwndHook);
                }
                _mouseHook.Dispose();
            }
        }

        private GlobalHotkey CreateHotkey(int id, int modifier, int key, EventHandler clickEvent)
        {
            var helper = new WindowInteropHelper(this);
            var result = new GlobalHotkey(helper.Handle, id, modifier, key);
            result.HotKeyPressed += clickEvent;
            return result;
        }

        private void Escape_Pressed(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }

            ClearActiveButtons();
        }

        private void Next_Pressed(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }

            ToggleButton(_currentMenuIndex, (_currentButtonIndex + 1) % _menus[_currentMenuIndex].Length);
        }

        private void Next_Submenu(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            var nextSubmenu = (_currentMenuIndex + 1) % _menus.Length;
            ToggleButton(nextSubmenu, Math.Clamp(_currentButtonIndex, 0, _menus[nextSubmenu].Length - 1));
        }

        private void Prev_Submenu(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            var nextSubmenu = (_currentMenuIndex - 1 + _menus.Length) % _menus.Length;
            ToggleButton(nextSubmenu, Math.Clamp(_currentButtonIndex, 0, _menus[nextSubmenu].Length - 1));
        }

        private void Prev_Pressed(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            ToggleButton(_currentMenuIndex, (_currentButtonIndex - 1 + _menus[_currentMenuIndex].Length) % _menus[_currentMenuIndex].Length);
        }

        private void Reset_Pressed(object sender, EventArgs e)
        {
            SendCommand(CommandType.RESET);
        }

        private void Reset_Pressed()
        {
            SendCommand(CommandType.RESET);
        }

        private void Inc_Pressed(object sender, EventArgs e)
        {
            SendCommand(CommandType.UP);
        }

        private void Dec_Pressed(object sender, EventArgs e)
        {
            SendCommand(CommandType.DOWN);
        }

        private void SendCommand(CommandType type)
        {
            if (!IsVisible)
            {
                return;
            }
            if (String.IsNullOrEmpty(_currentCommand))
            {
                return;
            }

            switch (type)
            {
                case CommandType.DOWN:
                    Connector.Instance.SendCommandAsync(_currentCommand + "=-1%");
                    break;
                case CommandType.UP:
                    Connector.Instance.SendCommandAsync(_currentCommand + "=+1%");
                    break;
                case CommandType.RESET:
                    Connector.Instance.SendCommandAsync(_currentCommand + "=reset");
                    break;
            }
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
            _currentCommand = "";
        }

        private void ToggleButton(int menu, int key)
        {
            ClearActiveButtons();
            var item = _menus[menu][key];

            item.IsActive = true;
            _currentMenuIndex = menu;
            _currentButtonIndex = key;
            _currentCommand = String.IsNullOrEmpty(item.LRCommand) ? "" : item.LRCommand;
        }

        private void HandleGlobalScrollUp()
        {
            // This will be invoked on any upward scroll globally
            Dispatcher.Invoke(() =>
            {
                SendCommand(CommandType.UP);
            });
        }

        private void HandleGlobalScrollDown()
        {
            // This will be invoked on any downward scroll globally
            Dispatcher.Invoke(() =>
            {
                SendCommand(CommandType.DOWN);
            });
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
                        _currentCommand = item.IsActive ? item.LRCommand : "";
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
            return IntPtr.Zero;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ControlButton clickedButton)
            {
                if (String.IsNullOrEmpty(clickedButton.LRCommand))
                {
                    return;
                }

                ToggleButton(clickedButton);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (var key in _hotkeys)
            {
                key.Unregister(HwndHook);
            }
            _mouseHook?.Dispose();
        }
    }
}
