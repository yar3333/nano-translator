using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NanoTranslator;

static class InterceptKeys
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    
    private static LowLevelKeyboardProc _proc = HookCallback;
    
    private static IntPtr _hookID = IntPtr.Zero;

    public static void Start()
    {
        _hookID = SetHook(_proc);
    }

    public static void Stop()
    {
        UnhookWindowsHookEx(_hookID);
    }

    public static Action<Keys>? OnKeyDown = null;
    public static Action<Keys>? OnKeyUp = null;

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using Process curProcess = Process.GetCurrentProcess();
        using ProcessModule curModule = curProcess.MainModule!;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName!), 0);
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            if (wParam == (IntPtr)WM_KEYDOWN)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                OnKeyDown?.Invoke((Keys)vkCode);
            }
            else 
            if (wParam == (IntPtr)WM_KEYUP)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                OnKeyUp?.Invoke((Keys)vkCode);
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
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
