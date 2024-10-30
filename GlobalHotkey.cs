using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace SpeedLR
{
    public class GlobalHotkey
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static int MOD_CONTROL = 0x0002;
        public static int ESCAPE = 27;
        public static int LEFT = 37;
        public static int RIGHT = 39;
        public static int UP = 38;
        public static int DOWN = 40;
        private const int WM_HOTKEY = 0x0312;

        private int id;
        private int key;
        private int modifier;
        private IntPtr hWnd;
        private HwndSource _source;

        private DateTime lastClickTime = DateTime.MinValue;
        private const int DOUBLE_CLICK_INTERVAL = 500;

        public event EventHandler HotKeyPressed;
        public event EventHandler HotKeyDoublePressed;

        public GlobalHotkey(IntPtr hWnd, int id, int modifier, int key)
        {
            this.hWnd = hWnd;
            this.id = id;
            this.modifier = modifier;
            this.key = key;
            _source = HwndSource.FromHwnd(hWnd);
        }

        public GlobalHotkey(IntPtr hWnd, int id, int modifier)
        {
            this.hWnd = hWnd;
            this.id = id;
            this.modifier = modifier;
            this.key = 0;
            _source = HwndSource.FromHwnd(hWnd);
        }

        public void Register(HwndSourceHook hook)
        {
            _source.AddHook(hook);
            RegisterHotKey(hWnd, id, modifier, key);
        }

        public void Unregister(HwndSourceHook hook)
        {
            _source.RemoveHook(hook);
            UnregisterHotKey(hWnd, id);
        }

        public void ProcessWindowMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == id)
            {
                handled = OnHotKeyPressed();

                DateTime currentTime = DateTime.Now;
                TimeSpan timeSinceLastClick = currentTime - lastClickTime;


                if (timeSinceLastClick.TotalMilliseconds <= DOUBLE_CLICK_INTERVAL)
                {
                    handled = handled || OnHotKeyDoublePressed();
                }

                lastClickTime = currentTime;
            }
        }

        protected virtual bool OnHotKeyPressed()
        {
            HotKeyPressed?.Invoke(this, EventArgs.Empty);
            return HotKeyPressed?.GetInvocationList().Length > 0;
        }

        protected virtual bool OnHotKeyDoublePressed()
        {
            HotKeyDoublePressed?.Invoke(this, EventArgs.Empty);
            return HotKeyDoublePressed?.GetInvocationList().Length > 0;
        }
    }
}
