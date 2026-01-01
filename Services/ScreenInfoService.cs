using System.Runtime.InteropServices;
using System.Windows;
using SoundPooper.Infrastructure.Services;

namespace SoundPooper.Services;

public class ScreenInfoService : IScreenInfoService
{
    private const int SmCxScreen = 0;
    private const int SmCyScreen = 1;

    public int Width { get; init; }
    public int Height { get; init; }
    public Point TopLeft { get; init; }
    public Point Center { get; init; }
    public double ScalingFactor { get; init; }

    [DllImport("user32.dll")]
    static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    static extern uint GetDpiForSystem();

    public ScreenInfoService()
    {
        var trueWidth = GetSystemMetrics(SmCxScreen);
        Width = (int)SystemParameters.PrimaryScreenWidth;
        Height = (int)SystemParameters.PrimaryScreenHeight;
        ScalingFactor = (double)trueWidth / Width;

        TopLeft = new Point(0, 0);
        Center = new Point(Width / 2d, Height / 2d);
    }

    public Point ToDpiPoint(double x, double y)
    {
        return new Point(x / ScalingFactor, y / ScalingFactor);
    }
}