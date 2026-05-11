using System.Windows;
using System.Windows.Input;
using SoundPooper.Infrastructure.Enums;
using SoundPooper.Infrastructure.Services;
using SoundPooper.MVVM.Commands;

namespace SoundPooper.MVVM.ViewModels;

public class ActionButtonViewModel : ViewModelBase
{
    private static readonly Action VoidAction = () => { };

    protected readonly IActionService ActionService;
    protected readonly ISoundService SoundService;
    protected ButtonFunctionEnum ButtonFunction { get; }
    public string Title { get; init; } = string.Empty;
    public ICommand MouseEnterCommand => new CommandBase(SetActionToExecute);

    public ActionButtonViewModel(
        ISoundService soundService,
        IActionService actionService,
        ButtonFunctionEnum buttonFunction,
        string title)
    {
        Title = title;
        ActionService = actionService;
        SoundService = soundService;
        ButtonFunction = buttonFunction;
    }

    protected ActionButtonViewModel()
    {
    }


    protected virtual void SetActionToExecute(object? e) => ActionService.SetActionToExecute(ButtonFunction switch
        {
            ButtonFunctionEnum.RepeatLastSound => SoundService.RepeatLastPlayedSound,
            ButtonFunctionEnum.StopPlaying => SoundService.StopPlaying,
            ButtonFunctionEnum.Quit => Application.Current.Shutdown,
            ButtonFunctionEnum.DoNothing => VoidAction,
            _ => VoidAction
        }
    );
}