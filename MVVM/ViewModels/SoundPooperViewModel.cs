using System.Windows;
using System.Windows.Input;
using NAudio.Utils;
using SoundPooper.Infrastructure.Services;
using SoundPooper.MVVM.Commands;

namespace SoundPooper.MVVM.ViewModels;

public class SoundPooperViewModel : ViewModelBase
{
    private readonly ICursorManager _cursorManager;

    public SoundPooperViewModel(ICursorManager cursorManager)
    {
        _cursorManager = cursorManager;
    }

    public ICommand MouseMoveCommand => new CommandBase(OnMouseMove);

    private void OnMouseMove(object? e)
    {
        var param = e as MouseEventArgs;
        _cursorManager.CheckAndLimit(
            param.GetPosition((IInputElement)param.Source),
            200,
            200,
            150,
            (p) => p
        );
    }
}