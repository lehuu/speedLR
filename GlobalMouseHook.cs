using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpeedLR
{
    public class GlobalMouseHook : IDisposable
    {
        private const int WH_MOUSE_LL = 14; // Low-level mouse hook
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MBUTTONDOWN = 0x0207;

        private IntPtr _hookID = (IntPtr) 999;
        private LowLevelMouseProc _proc;

        public event Action OnMouseScrollUp;    // Event for scroll up
        public event Action OnMouseScrollDown;  // Event for scroll down
        public event Action OnMiddleMouseButtonClick;

        public GlobalMouseHook()
        {
            _proc = HookCallback;
        }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_MOUSEWHEEL)
                {
                    int scrollDelta = ((int)Marshal.ReadInt32(lParam + 8)) >> 16;

                    if (scrollDelta > 0 && OnMouseScrollUp?.GetInvocationList().Length > 0)
                    {
                        OnMouseScrollUp?.Invoke();
                        return (IntPtr)1;

                    }
                    else if (scrollDelta < 0 && OnMouseScrollDown?.GetInvocationList().Length > 0)
                    {
                        OnMouseScrollDown?.Invoke();
                        return (IntPtr)1;

                    }
                }
                else if (wParam == (IntPtr)WM_MBUTTONDOWN && OnMiddleMouseButtonClick?.GetInvocationList().Length > 0)
                {
                    OnMiddleMouseButtonClick?.Invoke(); // Trigger middle mouse button event
                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Register()
        {
            UnhookWindowsHookEx(_hookID);
            _hookID = SetHook(_proc);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }

        // Delegate for the hook procedure
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
