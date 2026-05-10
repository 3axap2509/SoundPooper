using System.ComponentModel;
using System.Windows;
using SoundPooper.Infrastructure.IoC;
using SoundPooper.Infrastructure.Services;
using Application = System.Windows.Application;

namespace SoundPooper;

public partial class App : Application
{
    private readonly IKeyboardService _keyboardService;
    private readonly ISoundService soundService;

    public App()
    {
        IocManager.InitializeContainer();
        InitializeComponent();
        MainWindow = IocManager.Resolve<Window>();
        _keyboardService = IocManager.Resolve<IKeyboardService>();
        soundService = IocManager.Resolve<ISoundService>();
        soundService.Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _keyboardService.RemoveHooks();
        base.OnExit(e);
    }
}