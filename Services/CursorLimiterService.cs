using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SoundPooper.Infrastructure.Services;

namespace SoundPooper.Services;

public class CursorLimiterService : ICursorLimiterService
{

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    public void CheckAndLimit(
        Point pos,
        double centerX,
        double centerY,
        double radius,
        Func<Point, Point> toScreen)
    {
        var dx = pos.X - centerX;
        var dy = pos.Y - centerY;
        if (!(dx * dx + dy * dy > radius * radius))
            return;
        var angle = Math.Atan2(dy, dx);
        var newX = (int)(centerX + radius * Math.Cos(angle));
        var newY = (int)(centerY + radius * Math.Sin(angle));
        var screenPoint = toScreen(new Point(newX, newY));
        // SetCursorPos((int)screenPoint.X, (int)screenPoint.Y);
        var a = SystemParameters.VirtualScreenWidth;
        var b = Screen.PrimaryScreen?.Bounds.Width;
        // SetCursorPos(
        //     (int)(SystemParameters.PrimaryScreenHeight / 2),
        //     (int)(SystemParameters.PrimaryScreenWidth / 2)
        // );
    }
}