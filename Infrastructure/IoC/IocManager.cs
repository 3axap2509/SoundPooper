using System.Windows;
using SoundPooper.Infrastructure.IoC.Factories;
using SoundPooper.Infrastructure.Services;
using SoundPooper.MVVM.ViewModels;
using SoundPooper.MVVM.Views;
using SoundPooper.Services;
using Unity;

namespace SoundPooper.Infrastructure.IoC;

public static class IocManager
{
    private static readonly UnityContainer Container;

    static IocManager()
    {
        Container = new UnityContainer();
    }

    public static void InitializeContainer()
    {
        //Helpers
        Container.RegisterSingleton<IScreenInfoService, ScreenInfoService>();
        Container.RegisterSingleton<ICursorLimiterService, CursorLimiterService>();

        //Infrastructure
        Container.RegisterSingleton<IActionService, ActionService>();
        Container.RegisterSingleton<ISoundService, SoundService>();
        Container.RegisterSingleton<ISoundViewModelFactory, SoundViewModelFactory>();
        Container.RegisterSingleton<IActionButtonViewModelFactory, ActionButtonViewModelFactory>();

        //MVVM
        Container.RegisterSingleton<SoundPooperViewModel>();
        Container.RegisterSingleton<App>();
        Container.RegisterSingleton<Window, SoundPooperView>();

        //Hardware
        Container.RegisterSingleton<IKeyboardService, KeyboardService>();
    }

    public static T Resolve<T>() => Container.Resolve<T>();
}