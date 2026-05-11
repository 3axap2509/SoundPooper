using SoundPooper.Infrastructure.IoC;

namespace SoundPooper.MVVM.ViewModels;

public class ViewModelLocator
{
    public static SoundPooperViewModel SoundPooperViewModel => IocManager.Resolve<SoundPooperViewModel>();
}