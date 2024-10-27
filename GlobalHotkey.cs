using System.Runtime.InteropServices;

namespace SpeedLR
{
    public class GlobalHotkey
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static int MOD_CONTROL = 0x0002;
        private const int WM_HOTKEY = 0x0312;

        private int id;
        private int key;
        private int modifier;
        private IntPtr hWnd;

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
        }

        public GlobalHotkey(IntPtr hWnd, int id, int modifier)
        {
            this.hWnd = hWnd;
            this.id = id;
            this.modifier = modifier;
            this.key = 0;
        }

        public void Register()
        {
            RegisterHotKey(hWnd, id, modifier, key);
        }

        public void Unregister()
        {
            UnregisterHotKey(hWnd, id);
        }

        public void ProcessWindowMessage(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == id)
            {
                OnHotKeyPressed();

                DateTime currentTime = DateTime.Now;
                TimeSpan timeSinceLastClick = currentTime - lastClickTime;


                if (timeSinceLastClick.TotalMilliseconds <= DOUBLE_CLICK_INTERVAL)
                {
                    OnHotKeyDoublePressed();
                }

                lastClickTime = currentTime;
            }
        }

        protected virtual void OnHotKeyPressed()
        {
            HotKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnHotKeyDoublePressed()
        {
            HotKeyDoublePressed?.Invoke(this, EventArgs.Empty);
        }
    }
}
