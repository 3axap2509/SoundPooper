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
        var soundFiles = Directory.GetFiles(
            "D:\\Games\\Battle.net\\BattleNetGames\\World of Warcraft\\_retail_\\Interface\\AddOns\\SharedMedia_MyMedia\\sound"
        );
        SoundList = soundFiles
            .Select(
                sf => new RadialMenuItem
                {
                    Name = Path.GetFileNameWithoutExtension(sf),
                    Uid = sf,
                    Content = Path.GetFileNameWithoutExtension(sf)
                }
            ).ToList();

        SoundList.ForEach(rmi => rmi.MouseEnter +=
            (sender, args) =>
            {
                if (sender is not RadialMenuItem rmItem) return;
                _lastSelectedItem = rmItem;
                Console.WriteLine(rmItem.Uid);
            });

        _cursorLimiterService = cursorLimiterService;
        _windowTopLeftX = (int)screenInfoService.TopLeft.X;
        _windowTopLeftY = (int)screenInfoService.TopLeft.Y;
    }

    private RadialMenuItem _lastSelectedItem;

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

    public List<RadialMenuItem> SoundList { get; init; }
}