using SoundPooper.Infrastructure.IoC;
using SoundPooper.Infrastructure.Services;
using SoundPooper.MVVM.Services;
using SoundPooper.MVVM.ViewModels;
using SoundPooper.MVVM.Views;
using Unity;

namespace SoundPooper;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        var container = IocManager.Container = new UnityContainer();
        container.RegisterSingleton<ICursorManager, CursorManagerService>();
        container.RegisterSingleton<SoundPooperViewModel>();
        container.RegisterSingleton<App>();
        container.RegisterSingleton<SoundPooperView>();
        var app = IocManager.Container.Resolve<App>();
        app.InitializeComponent();
        app?.Run();
    }
}