using System.IO;
using SoundPooper.Infrastructure.Services;
using SoundPooper.MVVM.ViewModels;

namespace SoundPooper.Infrastructure.IoC.Factories;

public interface ISoundViewModelFactory
{
    SoundButtonViewModel Create(string filePath, int height, int width);
}

public class SoundViewModelFactory(
    ISoundService soundService,
    IActionService actionService
) : ISoundViewModelFactory
{
    public SoundButtonViewModel Create(string filePath, int height, int width) =>
        new(
            soundService,
            actionService,
            Path.GetFileNameWithoutExtension(filePath),
            filePath,
            height,
            width
        );
}