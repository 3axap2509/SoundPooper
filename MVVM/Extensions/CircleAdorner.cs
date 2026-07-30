using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace SoundPooper.MVVM.Extensions
{
    public class CircleAdorner : Adorner
    {
        public CircleAdorner(UIElement adornedElement) : base(adornedElement) { }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

            // Some arbitrary drawing implements.
            var renderBrush = new SolidColorBrush(Colors.Green)
            {
                Opacity = 0.2
            };
            var renderPen = new Pen(new SolidColorBrush(Colors.DarkRed), 2d);
            const double renderRadius = 2d;

            // Draw a circle at each corner.
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius,
                renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius,
                renderRadius);
        }
    }

    public static class AdornerExtensions
    {
        public static readonly DependencyProperty ShowCircleAdornerProperty =
            DependencyProperty.RegisterAttached(
                "ShowCircleAdorner",
                typeof(bool),
                typeof(AdornerExtensions),
                new PropertyMetadata(false, PropertyChangedCallback)
            );

        public static bool GetShowCircleAdorner(DependencyObject obj) =>
            (bool)obj.GetValue(ShowCircleAdornerProperty);

        public static void SetShowCircleAdorner(DependencyObject obj, bool value) =>
            obj.SetValue(ShowCircleAdornerProperty, value);

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement element) return;

            var adornerLayer = AdornerLayer.GetAdornerLayer(element);
            if (adornerLayer == null) return;

            if ((bool)e.NewValue)
            {
                var existing = adornerLayer.GetAdorners(element)?
                    .OfType<CircleAdorner>().FirstOrDefault();
                if (existing == null)
                    adornerLayer.Add(new CircleAdorner(element));
            }
            else
            {
                var adorners = adornerLayer.GetAdorners(element);
                if (adorners == null) return;
                foreach (var adorner in adorners.OfType<CircleAdorner>().ToList())
                    adornerLayer.Remove(adorner);
            }
        }
    }
}