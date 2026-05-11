using System.Drawing;
using SoundPooper.Infrastructure.Enums;
using SoundPooper.Infrastructure.Services;

namespace SoundPooper.MVVM.ViewModels;

public class SoundButtonViewModel : ActionButtonViewModel
{
    protected string Path { get; init; }
    public Icon? Icon { get; set; }
    public int? Height { get; init; }
    public int? Width { get; init; }

    public SoundButtonViewModel()
    {
    }

    public SoundButtonViewModel(
        ISoundService soundService,
        IActionService actionService,
        string title,
        string path,
        int height,
        int width)
        : base(soundService, actionService, ButtonFunctionEnum.PlaySound, title)
    {
        Path = path;
        Height = height;
        Width = width;
    }

    protected override void SetActionToExecute(object? e)
    {
        if (ButtonFunction != ButtonFunctionEnum.PlaySound)
            throw new InvalidOperationException();
        ActionService.SetActionToExecute(SoundService.PlaySound, Path);
    }
}