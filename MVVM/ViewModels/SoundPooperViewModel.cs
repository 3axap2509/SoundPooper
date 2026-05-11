using System.IO;
using System.Windows;
using System.Windows.Input;
using SoundPooper.Infrastructure.Enums;
using SoundPooper.Infrastructure.IoC.Factories;
using SoundPooper.Infrastructure.Services;

namespace SoundPooper.MVVM.ViewModels;

public class SoundPooperViewModel : ViewModelBase
{
    private readonly ICursorLimiterService _cursorLimiterService;
    private readonly ISoundService _soundService;
    private int _windowTopLeftX;
    private int _windowTopLeftY;
    private float _soundVolume = 0.3f;
    private int SoundElementHeight { get; }
    private int SoundElementWidth { get; }


    //constructor for data-designer
    public SoundPooperViewModel()
    {
        ContainerWidth = 700;
        ContainerHeight = 500;
        SoundElementHeight = SoundElementWidth = 80;
        SoundListLeftPart =
        [
            new SoundButtonViewModel() { Title = "heh1", Height = SoundElementHeight, Width = SoundElementWidth },
            new SoundButtonViewModel() { Title = "heh2", Height = SoundElementHeight, Width = SoundElementWidth },
            new SoundButtonViewModel() { Title = "heh3", Height = SoundElementHeight, Width = SoundElementWidth },
            new SoundButtonViewModel() { Title = "heh4", Height = SoundElementHeight, Width = SoundElementWidth }
        ];
        SoundListRightPart =
        [
            new SoundButtonViewModel() { Title = "heh5", Height = SoundElementHeight, Width = SoundElementWidth },
            new SoundButtonViewModel() { Title = "heh6", Height = SoundElementHeight, Width = SoundElementWidth },
            new SoundButtonViewModel() { Title = "heh7", Height = SoundElementHeight, Width = SoundElementWidth },
            new SoundButtonViewModel() { Title = "heh8", Height = SoundElementHeight, Width = SoundElementWidth }
        ];
        RepeatActionButton = new(null, null, ButtonFunctionEnum.RepeatLastSound, "Repeat");
        CancelActionButton = new(null, null, ButtonFunctionEnum.DoNothing, "Cancel");
        StopActionButton = new(null, null, ButtonFunctionEnum.StopPlaying, "Stop");
        QuitActionButton = new(null, null, ButtonFunctionEnum.Quit, "Quit");
    }

    public SoundPooperViewModel(
        ICursorLimiterService cursorLimiterService,
        IScreenInfoService screenInfoService,
        ISoundService soundService,
        ISoundViewModelFactory soundVmFactory,
        IActionButtonViewModelFactory actionButtonVmFactory)
    {
        ContainerWidth = 700;
        ContainerHeight = 500;
        var soundFiles = Directory.GetFiles("D:\\HehMusic\\test");

        SoundElementHeight = SoundElementWidth = 70;
        var soundsCount = soundFiles.Length;
        SoundListLeftPart = soundFiles
            .Take(soundsCount / 2)
            .Select(sf => soundVmFactory.Create(sf, SoundElementHeight, SoundElementWidth))
            .ToList();
        SoundListRightPart = soundFiles
            .TakeLast(soundsCount / 2)
            .Select(sf => soundVmFactory.Create(sf, SoundElementHeight, SoundElementWidth))
            .ToList();

        _cursorLimiterService = cursorLimiterService;
        _soundService = soundService;

        _windowTopLeftX = (int)screenInfoService.TopLeft.X;
        _windowTopLeftY = (int)screenInfoService.TopLeft.Y;

        RepeatActionButton = actionButtonVmFactory.Create(ButtonFunctionEnum.RepeatLastSound, "Repeat");
        StopActionButton = actionButtonVmFactory.Create(ButtonFunctionEnum.StopPlaying, "Stop");
        CancelActionButton = actionButtonVmFactory.Create(ButtonFunctionEnum.DoNothing, "Cancel");
        QuitActionButton = actionButtonVmFactory.Create(ButtonFunctionEnum.Quit, "Quit");
    }

    public List<SoundButtonViewModel> SoundListLeftPart { get; }
    public List<SoundButtonViewModel> SoundListRightPart { get; }

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

    public int ContainerHeight { get; }
    public int ContainerWidth { get; }

    public ActionButtonViewModel StopActionButton { get; }
    public ActionButtonViewModel RepeatActionButton { get; }
    public ActionButtonViewModel CancelActionButton { get; }
    public ActionButtonViewModel QuitActionButton { get; }

    public float SoundVolume
    {
        get => _soundVolume;
        set
        {
            if (!SetField(ref _soundVolume, value)) return;
            _soundService.SetSoundVolume(value);
        }
    }
}