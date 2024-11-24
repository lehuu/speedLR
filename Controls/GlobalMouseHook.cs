using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpeedLR.Controls
{
    public class GlobalMouseHook : IDisposable
    {
        private const int WH_MOUSE_LL = 14; // Low-level mouse hook
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        
        private nint _hookID = 999;
        private LowLevelMouseProc _proc;
        private bool _isDragging = false;
        private int _lastX = 0;
        private static int MIN_DRAG_DISTANCE = 10; 

        public event Action OnMouseScrollUp;     // Event for scroll up
        public event Action OnMouseScrollDown;   // Event for scroll down
        public event Action OnMiddleMouseButtonClick;
        public event Action OnMouseDragLeft;     // Event for dragging left
        public event Action OnMouseDragRight;    // Event for dragging right
        public event Func<int, int, bool> OnMouseClickDown;
        public event Func<int, int, bool> OnMouseClickUp;

        public GlobalMouseHook()
        {
            _proc = HookCallback;
        }

        private nint SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private nint HookCallback(int nCode, nint wParam, nint lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                if (wParam == WM_MOUSEWHEEL)
                {
                    int scrollDelta = Marshal.ReadInt32(lParam + 8) >> 16;

                    if (scrollDelta > 0 && OnMouseScrollUp?.GetInvocationList().Length > 0)
                    {
                        OnMouseScrollUp?.Invoke();
                        return 1;
                    }
                    else if (scrollDelta < 0 && OnMouseScrollDown?.GetInvocationList().Length > 0)
                    {
                        OnMouseScrollDown?.Invoke();
                        return 1;
                    }
                }
                else if (wParam == WM_MBUTTONDOWN && OnMiddleMouseButtonClick?.GetInvocationList().Length > 0)
                {
                    OnMiddleMouseButtonClick?.Invoke(); // Trigger middle mouse button event
                    return 1;
                }
                else if (wParam == WM_LBUTTONDOWN)
                {
                    _isDragging = true;
                    _lastX = hookStruct.pt.x;

                    if(OnMouseClickDown != null && OnMouseClickDown.Invoke(hookStruct.pt.x, hookStruct.pt.y))
                    {
                        return 1;
                    }
                }
                else if (wParam == WM_LBUTTONUP)
                {
                    _isDragging = false;

                    if (OnMouseClickUp != null && OnMouseClickUp.Invoke(hookStruct.pt.x, hookStruct.pt.y))
                    {
                        return 1;
                    }
                }
                else if (wParam == WM_MOUSEMOVE && _isDragging)
                {
                    if ((_lastX - hookStruct.pt.x) > MIN_DRAG_DISTANCE  && OnMouseDragLeft?.GetInvocationList().Length > 0)
                    {
                        OnMouseDragLeft?.Invoke();
                        _lastX = hookStruct.pt.x;
                    }
                    else if ((hookStruct.pt.x - _lastX) > MIN_DRAG_DISTANCE && OnMouseDragRight?.GetInvocationList().Length > 0)
                    {
                        OnMouseDragRight?.Invoke();
                        _lastX = hookStruct.pt.x;
                    }
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
        private delegate nint LowLevelMouseProc(int nCode, nint wParam, nint lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, nint hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(nint hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}