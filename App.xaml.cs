using System.Windows;
using SoundPooper.Infrastructure.IoC;
using SoundPooper.Infrastructure.Services;
using SoundPooper.Services;
using SoundPooper.MVVM.ViewModels;
using SoundPooper.MVVM.Views;
using Unity;
using Application = System.Windows.Application;

namespace SoundPooper;

public partial class App : Application
{
    private readonly IKeyboardService _keyboardService;

    public App()
    {
        IocManager.InitializeContainer();
        InitializeComponent();
        MainWindow = IocManager.Resolve<Window>();
        _keyboardService = IocManager.Resolve<IKeyboardService>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        MainWindow?.Show();
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _keyboardService.RemoveHooks();
        base.OnExit(e);
    }
}