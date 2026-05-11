using SoundPooper.Infrastructure.Enums;
using SoundPooper.Infrastructure.Services;
using SoundPooper.MVVM.ViewModels;

namespace SoundPooper.Infrastructure.IoC.Factories;

public interface IActionButtonViewModelFactory
{
    ActionButtonViewModel Create(ButtonFunctionEnum buttonFunction, string title);
}

public class ActionButtonViewModelFactory : IActionButtonViewModelFactory
{
    private ISoundService _soundService;
    private IActionService _actionService;

    public ActionButtonViewModelFactory(ISoundService soundService, IActionService actionService)
    {
        _soundService = soundService;
        _actionService = actionService;
    }

    public ActionButtonViewModel Create(ButtonFunctionEnum buttonFunction, string title) =>
        new(_soundService, _actionService, buttonFunction, title);
}