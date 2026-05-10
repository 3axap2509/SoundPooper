using System.IO;
using SoundPooper.Infrastructure.Services;
using SoundPooper.MVVM.ViewModels;

namespace SoundPooper.Infrastructure.IoC.Factories;

public interface ISoundViewModelFactory
{
    SoundButtonViewModel Create(string filePath, int height, int width);
}

public class SoundViewModelFactory : ISoundViewModelFactory
{
    private readonly ISoundService _soundService;

    public SoundViewModelFactory(ISoundService soundService)
    {
        _soundService = soundService;
    }

    public SoundButtonViewModel Create(string filePath, int height, int width) =>
        new(
            _soundService,
            Path.GetFileNameWithoutExtension(filePath),
            filePath,
            height,
            width
        );
}