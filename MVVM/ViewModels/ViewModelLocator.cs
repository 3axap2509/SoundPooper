using SoundPooper.Infrastructure.IoC;
using Unity;

namespace SoundPooper.MVVM.ViewModels;

public class ViewModelLocator
{
    public static SoundPooperViewModel SoundPooperViewModel => IocManager.Resolve<SoundPooperViewModel>();
}