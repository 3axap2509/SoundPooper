using System.Windows;
using System.Windows.Media;

namespace SoundPooper.MVVM.Extensions;

public static class ButtonExtensions
{
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.RegisterAttached(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(ButtonExtensions),
            new PropertyMetadata(new CornerRadius(0)));

    public static readonly DependencyProperty LabelBackgroundProperty =
        DependencyProperty.RegisterAttached(
            "LabelBackground",
            typeof(Brush),
            typeof(ButtonExtensions),
            new PropertyMetadata(Brushes.Transparent));

    public static void SetCornerRadius(UIElement element, CornerRadius value)
        => element.SetValue(CornerRadiusProperty, value);

    public static CornerRadius GetCornerRadius(UIElement element)
        => (CornerRadius)element.GetValue(CornerRadiusProperty);


    public static void SetLabelBackground(UIElement element, Brush value)
        => element.SetValue(LabelBackgroundProperty, value);

    public static CornerRadius GetLabelBackground(UIElement element)
        => (CornerRadius)element.GetValue(LabelBackgroundProperty);
}