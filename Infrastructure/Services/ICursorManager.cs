using System.Windows;

namespace SoundPooper.Infrastructure.Services;

public interface ICursorManager
{
    void CheckAndLimit(Point pos, double centerX, double centerY, double radius, Func<Point, Point> toScreen);
}