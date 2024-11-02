using System.Timers;
using System.Windows;
using System.Windows.Interop;
using Timer = System.Timers.Timer;

namespace SpeedLR
{

    public partial class ControllerWindow : Window
    {
        private ControlButton[] _stepButtons;
        private ControlButton[][] _menus;
        private GlobalHotkey[] _hotkeys;
        private GlobalHotkey[] _commandHotkeys;
        private GlobalMouseHook _mouseHook;
        private readonly Timer _hideTimer;

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
            DpiHelper.AdjustScaleForDpi(this);
            SourceInitialized += (s, e) => this.DpiChanged += Window_DpiChanged;
            _hideTimer = new Timer(500);
            _hideTimer.AutoReset = false;
            _hideTimer.Elapsed += OnHideElapsed;
            _stepButtons = new ControlButton[] {
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
                    texture
                };

                _menus = new ControlButton[2][];
                _menus[0] = firstMenu;
                _menus[1] = secondMenu;


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
                        item.IsEnabled = isConnected;
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
            CurrentCommand = String.IsNullOrEmpty(item.LRCommand) ? "" : item.LRCommand;
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
                        CurrentCommand = item.IsActive ? item.LRCommand : "";
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
