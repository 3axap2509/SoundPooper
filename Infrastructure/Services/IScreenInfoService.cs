using System.Windows;

namespace SoundPooper.Infrastructure.Services;

public interface IScreenInfoService
{
    Point ToDpiPoint(double x, double y);
    public int Width { get; init; }
    public int Height { get; init; }
    public Point TopLeft { get; init; }
    public Point Center { get; init; }
    public double ScalingFactor { get; init; }
}