using SoundPooper.Infrastructure.Enums;
using SoundPooper.Infrastructure.Services;
using SoundPooper.MVVM.ViewModels;

namespace SoundPooper.Infrastructure.IoC.Factories;

public interface IActionButtonViewModelFactory
{
    ActionButtonViewModel Create(ButtonFunctionEnum buttonFunction, string title);
}

public class ActionButtonViewModelFactory(
    ISoundService soundService,
    IActionService actionService
) : IActionButtonViewModelFactory
{
    public ActionButtonViewModel Create(ButtonFunctionEnum buttonFunction, string title) =>
        new(soundService, actionService, buttonFunction, title);
}