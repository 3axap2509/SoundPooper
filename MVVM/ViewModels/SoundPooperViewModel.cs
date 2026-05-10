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

    public List<SoundButtonViewModel> SoundListLeftPart { get; init; }
    public List<SoundButtonViewModel> SoundListRightPart { get; init; }

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
    }

    public SoundPooperViewModel(
        ICursorLimiterService cursorLimiterService,
        IScreenInfoService screenInfoService,
        ISoundService soundService,
        ISoundViewModelFactory soundVmFactory)
    {
        ContainerWidth = 700;
        ContainerHeight = 500;
        var soundFiles = Directory.GetFiles(
            "D:\\HehMusic\\test"
        );

        SoundElementHeight = SoundElementWidth = 70;
        var soundsCount = soundFiles.Length;
        SoundListLeftPart = soundFiles
            .Take(soundsCount / 2)
            .Select(sf =>
                soundVmFactory.Create(sf, SoundElementHeight, SoundElementWidth)
            ).ToList();
        SoundListRightPart = soundFiles
            .TakeLast(soundsCount / 2)
            .Select(sf =>
                soundVmFactory.Create(sf, SoundElementHeight, SoundElementWidth)
            ).ToList();

        _cursorLimiterService = cursorLimiterService;
        _soundService = soundService;
        _windowTopLeftX = (int)screenInfoService.TopLeft.X;
        _windowTopLeftY = (int)screenInfoService.TopLeft.Y;
    }

    private int _windowTopLeftX;
    private int _windowTopLeftY;
    private float _soundVolume = 0.7f;
    private int SoundElementHeight { get; }
    private int SoundElementWidth { get; }

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

    public ActionButtonViewModel StopActionButton =>
        new(
            _soundService,
            ButtonFunctionEnum.StopPlaying,
            "Stop"
        );

    public ActionButtonViewModel RepeatActionButton =>
        new(
            _soundService,
            ButtonFunctionEnum.RepeatLastSound,
            "Repeat"
        );

    public ActionButtonViewModel CancelActionButton =>
        new(
            _soundService,
            ButtonFunctionEnum.DoNothing,
            "Cancel"
        );

    public float SoundVolume
    {
        get => _soundVolume;
        set
        {
            if (!SetField(ref _soundVolume, value)) return;
            _soundService.SetSoundVolume(value);
        }
    }


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