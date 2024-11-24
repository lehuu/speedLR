using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace SpeedLR.Controls
{
    public class GlobalHotkey
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(nint hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(nint hWnd, int id);

        public static int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;
        public const int MOD_ALT = 0x0001;
        private const int WM_HOTKEY = 0x0312;

        private int _id;
        private int _key;
        private int _modifier;
        private nint _hWnd;
        private HwndSource _source;
        private bool _isRegistered = false;

        private bool _isDoubleClickHandled = false;

        private DateTime _lastClickTime = DateTime.MinValue;
        private const int DOUBLE_CLICK_INTERVAL = 500;

        public event EventHandler HotKeyPressed;
        public event EventHandler HotKeyDoublePressed;

        // https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        public GlobalHotkey(nint hWnd, int id, int modifier, int key)
        {
            _hWnd = hWnd;
            _id = id;
            _modifier = modifier;
            _key = key;
            _source = HwndSource.FromHwnd(hWnd);
        }

        public GlobalHotkey(nint hWnd, int id, int modifier)
        {
            _hWnd = hWnd;
            _id = id;
            _modifier = modifier;
            _key = 0;
            _source = HwndSource.FromHwnd(hWnd);
        }

        public void Register(HwndSourceHook hook)
        {
            if (_isRegistered)
                return;

            _source.AddHook(hook);
            _isRegistered = RegisterHotKey(_hWnd, _id, _modifier, _key);
        }

        public void Unregister(HwndSourceHook hook)
        {
            if (!_isRegistered)
                return;

            _source.RemoveHook(hook);
            _isRegistered = !UnregisterHotKey(_hWnd, _id);
        }

        public void ProcessWindowMessage(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == _id)
            {
                handled = OnHotKeyPressed();

                DateTime currentTime = DateTime.Now;
                TimeSpan timeSinceLastClick = currentTime - _lastClickTime;


                if (timeSinceLastClick.TotalMilliseconds <= DOUBLE_CLICK_INTERVAL && !_isDoubleClickHandled)
                {
                    handled = handled || OnHotKeyDoublePressed();
                } else
                {
                    _isDoubleClickHandled = false;
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
            if (_isDoubleClickHandled)
            {
                return false;
            }
            _isDoubleClickHandled = HotKeyDoublePressed?.GetInvocationList().Length > 0;
            HotKeyDoublePressed?.Invoke(this, EventArgs.Empty);
            return _isDoubleClickHandled;
        }
    }
}
