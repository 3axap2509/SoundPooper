using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SoundPooper.UserControls
{
    // this is AI-created user-control, be careful with your changes
    public partial class CircularSlider : UserControl
    {
        private const double DefaultMinimumValue = 0;
        private const double DefaultMaximumValue = 100;
        private const double DefaultValue = 0;
        private const double MinAngle = -90;
        private const double MaxAngle = 90;
        private const double SweepAngle = MaxAngle - MinAngle;

        private bool _isDragging;
        private Point _bottomCenter;
        private double _radius;
        private double _thumbRadius;
        private double _innerRadius;

        #region Dependency Properties

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                nameof(Minimum),
                typeof(double),
                typeof(CircularSlider),
                new FrameworkPropertyMetadata(DefaultMinimumValue, OnRangeChanged)
            );

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(double),
                typeof(CircularSlider),
                new FrameworkPropertyMetadata(DefaultMaximumValue, OnRangeChanged)
            );

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(CircularSlider),
                new FrameworkPropertyMetadata(
                    DefaultValue,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged,
                    CoerceValue
                )
            );

        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(
                nameof(SmallChange),
                typeof(double),
                typeof(CircularSlider),
                new PropertyMetadata(1.0)
            );

        #endregion

        #region Properties

        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double SmallChange
        {
            get => (double)GetValue(SmallChangeProperty);
            set => SetValue(SmallChangeProperty, value);
        }

        #endregion

        public CircularSlider()
        {
            InitializeComponent();
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var finalSize = base.ArrangeOverride(arrangeBounds);

            if (finalSize.Width > 0 && finalSize.Height > 0)
            {
                CalculateGeometry();
                UpdateBackground();
                UpdateGaugeArc();
                UpdateTrackPath();
                UpdateThumbPosition();
                UpdateValueText();
            }

            return finalSize;
        }

        private void UpdateValueText()
        {
            if (_innerRadius <= 0 || ValueText == null) return;

            var x = _bottomCenter.X;
            var y = _bottomCenter.Y - _innerRadius;

            ValueText.Text = Value.ToString("0%");
            ValueText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(ValueText, x - ValueText.DesiredSize.Width / 2);
            Canvas.SetTop(ValueText, y + ValueText.DesiredSize.Height / 2);
        }


        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = (CircularSlider)d;
            slider.CoerceValue(ValueProperty);
            slider.UpdateLayout();
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var slider = (CircularSlider)d;
            var val = (double)baseValue;
            return Math.Clamp(val, slider.Minimum, slider.Maximum);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = (CircularSlider)d;
            slider.UpdateGaugeArc();
            slider.UpdateThumbPosition();
            slider.UpdateValueText();
        }

        private void CalculateGeometry()
        {
            var w = this.ActualWidth;
            var h = this.ActualHeight;

            if (w < 1 || double.IsNaN(w)) w = this.Width;
            if (h < 1 || double.IsNaN(h)) h = this.Height;
            if (double.IsNaN(w)) w = 100;
            if (double.IsNaN(h)) h = 50;

            _bottomCenter = new Point(w / 2, h);

            var thumbWidth = Thumb != null && !double.IsNaN(Thumb.Width) ? Thumb.Width : 16;
            _thumbRadius = thumbWidth / 2;

            var trackThickness = TrackPath != null && !double.IsNaN(TrackPath.StrokeThickness)
                ? TrackPath.StrokeThickness
                : 6;

            var margin = Math.Max(trackThickness / 2, _thumbRadius);
            _radius = w / 2 - margin;
            _innerRadius = _radius - trackThickness - 2;
        }

        // Pie shaped background
        private void UpdateBackground()
        {
            if (_innerRadius <= 0) return;

            var startPoint = GetPointOnCircle(MinAngle, _innerRadius);
            var endPoint = GetPointOnCircle(MaxAngle, _innerRadius);

            var figure = new PathFigure { StartPoint = _bottomCenter, IsClosed = true };
            // Line from center to 'Minimal value' point
            figure.Segments.Add(new LineSegment(startPoint, true));
            // Line from 'Minimal value' point to 'Maximum value'
            figure.Segments.Add(new ArcSegment
            {
                Point = endPoint,
                Size = new Size(_innerRadius, _innerRadius),
                IsLargeArc = SweepAngle > 180,
                SweepDirection = SweepDirection.Clockwise,
                IsSmoothJoin = true
            });
            // Line from 'Maximum value' back to the center
            figure.Segments.Add(new LineSegment(_bottomCenter, true));

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            BackgroundPath.Data = geometry;
        }

        // Colored arc from 'Minimal value' point to 'Current value'
        private void UpdateGaugeArc()
        {
            if (_innerRadius <= 0) return;

            var percent = (Value - Minimum) / (Maximum - Minimum);
            var currentAngle = MinAngle + percent * SweepAngle;

            var startPoint = GetPointOnCircle(MinAngle, _innerRadius);
            var endPoint = GetPointOnCircle(currentAngle, _innerRadius);

            var figure = new PathFigure { StartPoint = startPoint };
            figure.Segments.Add(new ArcSegment
            {
                Point = endPoint,
                Size = new Size(_innerRadius, _innerRadius),
                IsLargeArc = (currentAngle - MinAngle) > 180,
                SweepDirection = SweepDirection.Clockwise,
                IsSmoothJoin = true
            });

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            GaugeArc.Data = geometry;
        }

        private void UpdateTrackPath()
        {
            if (_radius <= 0) return;

            var startPoint = GetPointOnCircle(MinAngle, _radius);
            var endPoint = GetPointOnCircle(MaxAngle, _radius);

            var figure = new PathFigure { StartPoint = startPoint };
            figure.Segments.Add(new ArcSegment
            {
                Point = endPoint,
                Size = new Size(_radius, _radius),
                IsLargeArc = SweepAngle > 180,
                SweepDirection = SweepDirection.Clockwise,
                IsSmoothJoin = true
            });

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            TrackPath.Data = geometry;
        }

        private Point GetPointOnCircle(double angleDeg, double radius)
        {
            var rad = angleDeg * Math.PI / 180;
            var x = _bottomCenter.X + radius * Math.Sin(rad);
            var y = _bottomCenter.Y - radius * Math.Cos(rad);
            return new Point(x, y);
        }

        private void UpdateThumbPosition()
        {
            var percent = (Value - Minimum) / (Maximum - Minimum);
            var angleDeg = MinAngle + percent * SweepAngle;
            var pos = GetPointOnCircle(angleDeg, _radius);
            Canvas.SetLeft(Thumb, pos.X - _thumbRadius);
            Canvas.SetTop(Thumb, pos.Y - _thumbRadius);
        }

        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var delta = e.Delta > 0 ? SmallChange : -SmallChange;
            var newValue = Value + delta;
            Value = Math.Max(Minimum, Math.Min(Maximum, newValue));
            e.Handled = true;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainCanvas.CaptureMouse();
            _isDragging = true;
            UpdateValueFromMouse(e.GetPosition(MainCanvas));
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
                UpdateValueFromMouse(e.GetPosition(MainCanvas));
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            MainCanvas.ReleaseMouseCapture();
        }

        private void UpdateValueFromMouse(Point mousePos)
        {
            var dx = mousePos.X - _bottomCenter.X;
            var dy = _bottomCenter.Y - mousePos.Y;
            var angleRad = Math.Atan2(dy, dx);
            var angleDeg = 90 - angleRad * 180 / Math.PI;

            var clampedAngle = Math.Max(MinAngle, Math.Min(MaxAngle, angleDeg));
            var percent = (clampedAngle - MinAngle) / SweepAngle;
            var newValue = Minimum + percent * (Maximum - Minimum);
            Value = Math.Round(newValue, 2);
        }
    }
}