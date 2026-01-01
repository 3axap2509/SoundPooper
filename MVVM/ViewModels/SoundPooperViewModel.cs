using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using NAudio.Utils;
using SoundPooper.Infrastructure.Services;
using SoundPooper.MVVM.Commands;

namespace SoundPooper.MVVM.ViewModels;

public class SoundPooperViewModel : ViewModelBase
{
    private readonly ICursorLimiterService _cursorLimiterService;
    private readonly IScreenInfoService _screenInfoService;

    public SoundPooperViewModel(ICursorLimiterService cursorLimiterService, IScreenInfoService screenInfoService)
    {
        _cursorLimiterService = cursorLimiterService;
        _screenInfoService = screenInfoService;
        _windowTopLeftX = (int)_screenInfoService.TopLeft.X;
        _windowTopLeftY = (int)_screenInfoService.TopLeft.Y;
    }

    private int _windowTopLeftX;
    private int _windowTopLeftY;

    public int WindowTopLeftX
    {
        get => _windowTopLeftX;
        set => SetField(ref _windowTopLeftX, value);
    }

    public int WindowTopLeftY
    {
        get => _windowTopLeftY;
        set => SetField(ref _windowTopLeftY, value);
    }

    public ICommand MouseMoveCommand => new CommandBase(OnMouseMove);

    private void OnMouseMove(object? e)
    {
        if (e is not MouseEventArgs param) return;
        _cursorLimiterService.CheckAndLimit(
            param.GetPosition((IInputElement)param.Source),
            _windowTopLeftX,
            _windowTopLeftY,
            150,
            (p) => p
        );
    }
}