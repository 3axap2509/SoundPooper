using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using SoundPooper.Infrastructure.Services;

namespace SoundPooper.Services;

public class KeyboardService : IKeyboardService
{
    private const int WhKeyboardLl = 13;
    private const int WmKeyDown = 0x0100;
    private const int WmKeyUp = 0x0101;
    private LowLevelKeyboardProc _proc;
    private static IntPtr _hookID = IntPtr.Zero;

    private readonly Window _view;
    private readonly IScreenInfoService _screenInfoService;
    private readonly ISoundService _soundService;

    private bool _isPressed;

    public KeyboardService(
        Window view,
        IScreenInfoService screenInfoService,
        ISoundService soundService)
    {
        _proc = HookCallback;
        _view = view;
        _screenInfoService = screenInfoService;
        _soundService = soundService;
        InitializeHooks();
    }

    public void InitializeHooks()
    {
        _hookID = SetHook(_proc);
    }

    public void RemoveHooks()
    {
        UnhookWindowsHookEx(_hookID);
    }


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(
        int idHook,
        LowLevelKeyboardProc lpfn,
        IntPtr hMod,
        uint dwThreadId
    );

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(
        IntPtr hhk,
        int nCode,
        IntPtr wParam,
        IntPtr lParam
    );


    private delegate IntPtr LowLevelKeyboardProc(
        int nCode,
        IntPtr wParam,
        IntPtr lParam
    );

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (Marshal.ReadInt32(lParam) != (int)Keys.Oemtilde || nCode < 0)
            return CallNextHookEx(_hookID, nCode, wParam, lParam);

        GetCursorPos(out var trueResPoint);
        var dpiPoint = _screenInfoService.ToDpiPoint(trueResPoint.X, trueResPoint.Y);
        switch (wParam)
        {
            case WmKeyDown when _isPressed:
                break;
            case WmKeyDown:
                _isPressed = true;
                _view.Left = dpiPoint.X - _view.Width / 2;
                _view.Top = dpiPoint.Y - _view.Height / 2;
                _view.Show();
                _view.Activate();
                _view.Focus();
                break;
            case WmKeyUp:
                _isPressed = false;
                _view.Hide();
                _soundService.ExecuteCurrentAction();
                Console.WriteLine("KeyUp");
                break;
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }


    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);


    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        if (curModule is null) throw new NullReferenceException();
        return SetWindowsHookEx(
            WhKeyboardLl,
            proc,
            GetModuleHandle(curModule.ModuleName),
            0
        );
    }

    // mouse
    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Point lpPoint);
}