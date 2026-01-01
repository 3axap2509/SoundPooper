using System.Windows;
using SoundPooper.Infrastructure.Services;
using SoundPooper.MVVM.ViewModels;
using SoundPooper.MVVM.Views;
using SoundPooper.Services;
using Unity;

namespace SoundPooper.Infrastructure.IoC;

public static class IocManager
{
    static IocManager()
    {
        Container = new UnityContainer();
    }

    private static readonly UnityContainer Container;

    public static void InitializeContainer()
    {
        Container.RegisterSingleton<IScreenInfoService, ScreenInfoService>();
        Container.RegisterSingleton<ICursorLimiterService, CursorLimiterService>();
        Container.RegisterSingleton<SoundPooperViewModel>();
        Container.RegisterSingleton<App>();
        Container.RegisterSingleton<Window, SoundPooperView>();
        Container.RegisterSingleton<IKeyboardService, KeyboardService>();
    }

    public static T Resolve<T>() => Container.Resolve<T>();
}