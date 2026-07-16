using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SoundPooper.UserControls
{
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
        private double _margin;

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

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(CircularSlider),
                new PropertyMetadata("Title")
            );

        public static readonly DependencyProperty MainColorProperty =
            DependencyProperty.Register(
                nameof(MainColor),
                typeof(Brush),
                typeof(CircularSlider),
                new PropertyMetadata(Brushes.Blue)
            );

        public static readonly DependencyProperty GlowColorProperty =
            DependencyProperty.Register(
                nameof(GlowColor),
                typeof(Brush),
                typeof(CircularSlider),
                new PropertyMetadata(Brushes.Aqua)
            );

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(StrokeThickness),
                typeof(double),
                typeof(CircularSlider),
                new PropertyMetadata(2d)
            );

        public static readonly DependencyProperty ThumbSizeProperty =
            DependencyProperty.Register(
                nameof(ThumbSize),
                typeof(double),
                typeof(CircularSlider),
                new PropertyMetadata(10d)
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

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Brush MainColor
        {
            get => (Brush)GetValue(MainColorProperty);
            set => SetValue(MainColorProperty, value);
        }

        public Brush GlowColor
        {
            get => (Brush)GetValue(GlowColorProperty);
            set => SetValue(GlowColorProperty, value);
        }

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public double ThumbSize
        {
            get => (double)GetValue(ThumbSizeProperty);
            set => SetValue(ThumbSizeProperty, value);
        }

        #endregion

        public CircularSlider()
        {
            InitializeComponent();
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var finalSize = base.ArrangeOverride(arrangeBounds);

            if (finalSize is { Width: > 0, Height: > 0 })
            {
                CalculateGeometry();
                UpdateBackground();
                UpdateTrackArcLine();
                UpdateBottomArcLine();
                UpdateThumbPosition();
            }

            return finalSize;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (CircularSlider)d;
            self.UpdateTrackArcLine();
            self.UpdateThumbPosition();
        }

        private void Canvas_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateGeometry();
            UpdateBackground();
            UpdateTrackArcLine();
            UpdateThumbPosition();
        }

        private void UpdateBottomArcLine()
        {
            const int cornerRadius = 25;
            if (_innerRadius <= 0) return;
            var h = BottomCanvas.ActualHeight;
            var w = BottomCanvas.ActualWidth;
            if (w < 1 || double.IsNaN(w) || h < 1 || double.IsNaN(h)) return;

            var sidesOffset = _margin - StrokeThickness / 2;
            var startPoint = new Point(0 + sidesOffset, 0);
            var endPoint = startPoint with { X = w - sidesOffset };

            var figure = new PathFigure { StartPoint = startPoint, IsClosed = true };
            // left rounded corner
            figure.Segments.Add(new ArcSegment
            {
                Point = startPoint with { Y = startPoint.Y + cornerRadius, X = startPoint.X + cornerRadius },
                Size = new Size(cornerRadius, cornerRadius),
                IsLargeArc = false,
                SweepDirection = SweepDirection.Counterclockwise,
                IsSmoothJoin = true
            });
            // straight line
            figure.Segments.Add(new LineSegment(
                endPoint with { X = endPoint.X - cornerRadius, Y = endPoint.Y + cornerRadius },
                false
            ));

            // right rounded corner
            figure.Segments.Add(new ArcSegment
            {
                Point = endPoint,
                Size = new Size(cornerRadius, cornerRadius),
                IsLargeArc = false,
                SweepDirection = SweepDirection.Counterclockwise,
                IsSmoothJoin = true
            });

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            BottomArcLine.Data = geometry;
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

        private Point GetPointOnCircle(double angleDeg, double radius)
        {
            var rad = angleDeg * Math.PI / 180;
            var x = _bottomCenter.X + radius * Math.Sin(rad);
            var y = _bottomCenter.Y - radius * Math.Cos(rad);
            return new Point(x, y);
        }


        private void CalculateGeometry()
        {
            var w = MainCanvas.ActualWidth;
            var h = MainCanvas.ActualHeight;

            if (w < 1 || double.IsNaN(w) || h < 1 || double.IsNaN(h)) return;

            _bottomCenter = new Point(w / 2, h);

            var thumbWidth = ValueThumb != null && !double.IsNaN(ValueThumb.Width) ? ValueThumb.Width : 16;
            _thumbRadius = thumbWidth / 2;

            var trackThickness = TrackArcLine != null && !double.IsNaN(TrackArcLine.StrokeThickness)
                ? TrackArcLine.StrokeThickness
                : 6;

            _margin = Math.Max(trackThickness / 2, _thumbRadius);
            _radius = w / 2 - _margin;
            _innerRadius = _radius;
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
            BackgroundArc.Data = geometry;
        }

        // Track Arc-line from 'Minimal value' to 'Current value'
        private void UpdateTrackArcLine()
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
            TrackArcLine.Data = geometry;
        }


        private void UpdateThumbPosition()
        {
            var percent = (Value - Minimum) / (Maximum - Minimum);
            var angleDeg = MinAngle + percent * SweepAngle;
            var pos = GetPointOnCircle(angleDeg, _radius);
            Canvas.SetLeft(ValueThumb, pos.X - _thumbRadius);
            Canvas.SetTop(ValueThumb, pos.Y - _thumbRadius);
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