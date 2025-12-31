using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using SoundPooper.MVVM.Views;
using Unity;
using Application = System.Windows.Application;

namespace SoundPooper;

public partial class App : Application
{
    public App(SoundPooperView mainWindow)
    {
        InitializeComponent();
        MainWindow = mainWindow;
    }

    private const int WhKeyboardLl = 13;
    private const int WmKeydown = 0x0100;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

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

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == WmKeydown)
        {
            if (Marshal.ReadInt32(lParam) == (int)Keys.Oemtilde)
            {
                Console.WriteLine((Keys)Marshal.ReadInt32(lParam));
            }
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }


    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    protected override void OnStartup(StartupEventArgs e)
    {
        Resources.MergedDictionaries.Add(
            new ResourceDictionary
            {
                Source = new Uri("AppResources.xaml", uriKind: UriKind.Relative)
            }
        );
        _hookID = SetHook(_proc);
        MainWindow.Show();
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        UnhookWindowsHookEx(_hookID);
        base.OnExit(e);
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using Process curProcess = Process.GetCurrentProcess();
        using ProcessModule curModule = curProcess.MainModule;
        return SetWindowsHookEx(
            WhKeyboardLl,
            proc,
            GetModuleHandle(curModule.ModuleName),
            0
        );
    }
}