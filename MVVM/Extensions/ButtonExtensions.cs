using System.Windows;

namespace SoundPooper.MVVM.Extensions;

public static class ButtonExtensions
{
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.RegisterAttached(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(ButtonExtensions),
            new PropertyMetadata(new CornerRadius(0)));

    public static void SetCornerRadius(UIElement element, CornerRadius value)
        => element.SetValue(CornerRadiusProperty, value);

    public static CornerRadius GetCornerRadius(UIElement element)
        => (CornerRadius)element.GetValue(CornerRadiusProperty);
}