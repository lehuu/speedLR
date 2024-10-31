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
        public const int MOD_SHIFT = 0x0004;
        public const int MOD_ALT = 0x0001;
        public static int ESCAPE = 27;
        public static int LEFT = 37;
        public static int RIGHT = 39;
        public static int UP = 38;
        public static int DOWN = 40;
        public static int SPACE = 32;
        private const int WM_HOTKEY = 0x0312;

        private int _id;
        private int _key;
        private int _modifier;
        private IntPtr _hWnd;
        private HwndSource _source;

        private DateTime _lastClickTime = DateTime.MinValue;
        private const int DOUBLE_CLICK_INTERVAL = 500;

        public event EventHandler HotKeyPressed;
        public event EventHandler HotKeyDoublePressed;

        public GlobalHotkey(IntPtr hWnd, int id, int modifier, int key)
        {
            this._hWnd = hWnd;
            this._id = id;
            this._modifier = modifier;
            this._key = key;
            _source = HwndSource.FromHwnd(hWnd);
        }

        public GlobalHotkey(IntPtr hWnd, int id, int modifier)
        {
            this._hWnd = hWnd;
            this._id = id;
            this._modifier = modifier;
            this._key = 0;
            _source = HwndSource.FromHwnd(hWnd);
        }

        public void Register(HwndSourceHook hook)
        {
            _source.AddHook(hook);
            RegisterHotKey(_hWnd, _id, _modifier, _key);
        }

        public void Unregister(HwndSourceHook hook)
        {
            _source.RemoveHook(hook);
            UnregisterHotKey(_hWnd, _id);
        }

        public void ProcessWindowMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == _id)
            {
                handled = OnHotKeyPressed();

                DateTime currentTime = DateTime.Now;
                TimeSpan timeSinceLastClick = currentTime - _lastClickTime;


                if (timeSinceLastClick.TotalMilliseconds <= DOUBLE_CLICK_INTERVAL)
                {
                    handled = handled || OnHotKeyDoublePressed();
                }

                _lastClickTime = currentTime;
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
