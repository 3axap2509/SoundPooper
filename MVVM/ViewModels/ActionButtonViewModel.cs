using System.Windows;
using System.Windows.Input;
using SoundPooper.Infrastructure.Enums;
using SoundPooper.Infrastructure.Services;
using SoundPooper.MVVM.Commands;

namespace SoundPooper.MVVM.ViewModels;

public class ActionButtonViewModel : ViewModelBase
{
    protected readonly ISoundService _soundService;
    public string? Title { get; init; }
    public ICommand MouseEnterCommand => new CommandBase(SetActionToExecute);

    public ButtonFunctionEnum ButtonFunction { get; init; }

    public ActionButtonViewModel()
    {
    }

    public ActionButtonViewModel(
        ISoundService soundService,
        ButtonFunctionEnum buttonFunction,
        string title)
    {
        Title = title;
        _soundService = soundService;
        ButtonFunction = buttonFunction;
    }


    protected virtual void SetActionToExecute(object? e)
    {
        switch (ButtonFunction)
        {
            case ButtonFunctionEnum.StopPlaying:
                _soundService.SetStopPlayingAction();
                break;
            case ButtonFunctionEnum.RepeatLastSound:
                _soundService.SetLastPlayedSoundToRepeat();
                break;
            case ButtonFunctionEnum.DoNothing:
            {
                _soundService.SetVoidAction();
                break;
            }
            case ButtonFunctionEnum.Quit:
                Application.Current.Shutdown();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}