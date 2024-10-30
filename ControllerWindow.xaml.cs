using System.Windows;
using System.Windows.Interop;

namespace SpeedLR
{

    public partial class ControllerWindow : Window
    {
        private ControlButton[][] menus;
        private GlobalHotkey[] hotkeys;

        private string currentCommand = "";
        private int currentButtonIndex = -1;
        private int currentMenuIndex = 0;

        public ControllerWindow()
        {
            InitializeComponent();
            IsVisibleChanged += ControllerWindow_IsVisibleChanged;
        }

        private void ControllerWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(hotkeys?.Length > 0))
            {
                var firstMenu = new ControlButton[3];
                firstMenu[0] = this.exposure;
                firstMenu[1] = this.shadows;
                firstMenu[2] = this.highlights;

                var secondMenu = new ControlButton[3];
                secondMenu[0] = this.blacks;
                secondMenu[1] = this.whites;
                secondMenu[2] = this.texture;

                menus = new ControlButton[2][];
                menus[0] = firstMenu;
                menus[1] = secondMenu;


                hotkeys = new GlobalHotkey[]
                {
                CreateHotkey(2, 0, GlobalHotkey.ESCAPE, Escape_Pressed),
                CreateHotkey(3, 0, GlobalHotkey.RIGHT, Next_Pressed),
                CreateHotkey(4, 0, GlobalHotkey.LEFT, Prev_Pressed),
                CreateHotkey(5, 0, GlobalHotkey.UP, Inc_Pressed),
                CreateHotkey(6, 0, GlobalHotkey.DOWN, Dec_Pressed),
                CreateHotkey(7, 0, GlobalHotkey.SPACE, Reset_Pressed),
                };
            }

            foreach (var key in hotkeys)
            {
                if (IsVisible)
                {
                    key.Register(HwndHook);
                }
                else
                {
                    key.Unregister(HwndHook);
                }
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

            ToggleButton(currentMenuIndex, (currentButtonIndex + 1) % menus[currentMenuIndex].Length);
        }

        private void Prev_Pressed(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            ToggleButton(currentMenuIndex, (currentButtonIndex - 1 + menus[currentMenuIndex].Length) % menus[currentMenuIndex].Length);
        }

        private void Reset_Pressed(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            Connector.Instance.SendCommandAsync(currentCommand + "=reset");
        }

        private void Inc_Pressed(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            if (String.IsNullOrEmpty(currentCommand))
            {
                return;
            }
            Connector.Instance.SendCommandAsync(currentCommand + "=+1%");
        }

        private void Dec_Pressed(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            if (String.IsNullOrEmpty(currentCommand))
            {
                return;
            }
            Connector.Instance.SendCommandAsync(currentCommand + "=-1%");
        }

        private void ControllerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void ClearActiveButtons()
        {
            for (int i = 0; i < menus.Length; i++)
            {
                for (int j = 0; j < menus[i].Length; j++)
                {
                    var item = menus[i][j];
                    item.IsActive = false;
                }
            }
            currentCommand = "";
        }

        private void ToggleButton(int menu, int key)
        {
            ClearActiveButtons();
            var item = menus[menu][key];

            item.IsActive = true;
            currentMenuIndex = menu;
            currentButtonIndex = key;
            currentCommand = String.IsNullOrEmpty(item.LRCommand) ? "" : item.LRCommand;
        }

        private void ToggleButton(ControlButton button)
        {
            for (int i = 0; i < menus.Length; i++)
            {
                for (int j = 0; j < menus[i].Length; j++)
                {
                    var item = menus[i][j];
                    if (button != null && item.Name == button.Name)
                    {
                        item.IsActive = !item.IsActive;
                        currentCommand = item.IsActive ? item.LRCommand : "";
                        currentButtonIndex = j;
                        currentMenuIndex = i;
                        continue;
                    }

                    item.IsActive = false;
                }
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            foreach (var key in hotkeys)
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
            foreach (var key in hotkeys)
            {
                key.Unregister(HwndHook);
            }
        }
    }
}
