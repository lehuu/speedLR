﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

public class LowLevelHotkey : IDisposable
{
    private static int DOUBLE_PRESS_THRESHOLD = 400;
    private static int LONG_PRESS_THRESHOLD = 400;

    private IntPtr _hookId = IntPtr.Zero;
    private LowLevelKeyboardProc _proc;
    private Key _key;

    private DateTime? _lastPressDown;
    private DateTime? _secondLastPressDown;
    private DateTime? _lastPressUp;
    private DateTime? _firstPressDown;
    private bool _isHeld = false;

    public event Action KeyPressed;
    public event Action KeyDoublePressed;
    public event Action KeyHoldStart;
    public event Action KeyHoldEnd;

    public LowLevelHotkey(Key key)
    {
        _proc = HookCallback;
        _hookId = SetHook(_proc);
        _key = key;
    }

    public void Dispose()
    {
        UnhookWindowsHookEx(_hookId);
    }

    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var keyInfo = (KeyboardInputStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardInputStruct));
            var key = KeyInterop.KeyFromVirtualKey(keyInfo.vkCode);
            if (key == _key)
            {
                var isKeyDown = wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN;
                var now = DateTime.Now;

                if (isKeyDown)
                {
                     _secondLastPressDown = _lastPressDown;
                     _lastPressDown = now;
                   
                    if(!_isHeld)
                    {
                        if(_firstPressDown.HasValue && (now - _firstPressDown.Value).TotalMilliseconds < LONG_PRESS_THRESHOLD)
                        {
                            _isHeld = true;
                            _firstPressDown = null;

                            KeyHoldStart?.Invoke();
                        } else
                        {
                            _firstPressDown = DateTime.Now;
                        }
                    }

                    KeyPressed?.Invoke();
                }
                else if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
                {
                    if (_lastPressUp.HasValue && _secondLastPressDown.HasValue)
                    {
                        if ((now - _lastPressUp.Value).TotalMilliseconds < DOUBLE_PRESS_THRESHOLD
                        && (now - _secondLastPressDown.Value).TotalMilliseconds < 2 * DOUBLE_PRESS_THRESHOLD)
                        {
                            KeyDoublePressed?.Invoke();
                        }
                    }

                    if(_isHeld)
                    {
                        KeyHoldEnd?.Invoke();
                    }

                    _lastPressUp = now;
                    _isHeld = false;
                    _firstPressDown = null;
                }
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInputStruct
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}