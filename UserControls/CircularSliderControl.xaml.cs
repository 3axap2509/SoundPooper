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
        private Point _center;
        private double _radius; // радиус дорожки
        private double _thumbRadius;
        private double _innerRadius; // радиус внутреннего сектора

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

        public CircularSlider()
        {
            InitializeComponent();
        }

        private void UpdateValueText()
        {
            if (_innerRadius <= 0 || ValueText == null) return;

            var x = _center.X;
            var y = _center.Y - _innerRadius;

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


        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateGeometry();
            UpdateBackground();
            UpdateGaugeArc();
            UpdateTrackPath();
            UpdateThumbPosition();
            UpdateValueText();
        }

        private void CalculateGeometry()
        {
            var w = MainCanvas.ActualWidth;
            var h = MainCanvas.ActualHeight;
            _center = new Point(w / 2, h / 2);
            _thumbRadius = Thumb.Width / 2;

            var margin = Math.Max(TrackPath.StrokeThickness / 2, _thumbRadius);
            _radius = Math.Min(w, h) / 2 - margin;
            _innerRadius = _radius - TrackPath.StrokeThickness - 2;
        }

        // Фон – сектор (pie shape) от MinAngle до MaxAngle
        private void UpdateBackground()
        {
            if (_innerRadius <= 0) return;

            var startPoint = GetPointOnCircle(MinAngle, _innerRadius);
            var endPoint = GetPointOnCircle(MaxAngle, _innerRadius);

            var figure = new PathFigure { StartPoint = _center, IsClosed = true };
            // От центра к начальной точке
            figure.Segments.Add(new LineSegment(startPoint, true));
            // Дуга до конечной точки
            figure.Segments.Add(new ArcSegment
            {
                Point = endPoint,
                Size = new Size(_innerRadius, _innerRadius),
                IsLargeArc = SweepAngle > 180,
                SweepDirection = SweepDirection.Clockwise,
                IsSmoothJoin = true
            });
            // Линия назад к центру
            figure.Segments.Add(new LineSegment(_center, true));

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            BackgroundPath.Data = geometry;
        }

        // Цветная дуга от минимального до текущего значения
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
            var x = _center.X + radius * Math.Sin(rad);
            var y = _center.Y - radius * Math.Cos(rad);
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

        // Прокрутка мыши
        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var delta = e.Delta > 0 ? SmallChange : -SmallChange;
            var newValue = Value + delta;
            Value = Math.Max(Minimum, Math.Min(Maximum, newValue));
            e.Handled = true;
        }

        // Перетаскивание бегунка
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
            var dx = mousePos.X - _center.X;
            var dy = _center.Y - mousePos.Y;
            var angleRad = Math.Atan2(dy, dx);
            var angleDeg = 90 - angleRad * 180 / Math.PI;

            var clampedAngle = Math.Max(MinAngle, Math.Min(MaxAngle, angleDeg));
            var percent = (clampedAngle - MinAngle) / SweepAngle;
            var newValue = Minimum + percent * (Maximum - Minimum);
            Value = Math.Round(newValue, 2);
        }
    }
}