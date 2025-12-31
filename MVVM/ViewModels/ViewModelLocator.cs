using SoundPooper.Infrastructure.IoC;
using Unity;

namespace SoundPooper.MVVM.ViewModels;

public class ViewModelLocator
{
    public SoundPooperViewModel SoundPooperViewModel => IocManager.Container.Resolve<SoundPooperViewModel>();
}