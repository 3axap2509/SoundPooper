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
    private IActionService _actionService;

    public SoundViewModelFactory(ISoundService soundService, IActionService actionService)
    {
        _soundService = soundService;
        _actionService = actionService;
    }

    public SoundButtonViewModel Create(string filePath, int height, int width) =>
        new(
            _soundService,
            _actionService,
            Path.GetFileNameWithoutExtension(filePath),
            filePath,
            height,
            width
        );
}