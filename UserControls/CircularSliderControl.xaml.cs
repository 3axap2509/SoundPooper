using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SoundPooper.UserControls
{
    public partial class CircularSlider : UserControl
    {
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(CircularSlider),
                new FrameworkPropertyMetadata(0.0, OnRangeChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(CircularSlider),
                new FrameworkPropertyMetadata(100.0, OnRangeChanged));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(CircularSlider),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged, CoerceValue));

        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(CircularSlider),
                new PropertyMetadata(1.0));

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

        // Углы в градусах
        private const double MinAngle = -90;
        private const double MaxAngle = 90;
        private const double SweepAngle = MaxAngle - MinAngle;

        private bool _isDragging;
        private Point _center;
        private double _radius; // радиус дорожки
        private double _thumbRadius;
        private double _innerRadius; // радиус внутреннего сектора

        public CircularSlider()
        {
            InitializeComponent();
        }

        private void UpdateValueText()
        {
            if (_innerRadius <= 0 || ValueText == null) return;

            // Позиция: по горизонтали — центр, по вертикали — чуть ниже центра (на 30% внутреннего радиуса вниз)
            double x = _center.X;
            double y = _center.Y - _innerRadius * 0.8;

            ValueText.Text = Value.ToString("0.00"); // или "0.0" для дробных
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
            double val = (double)baseValue;
            return Math.Max(slider.Minimum, Math.Min(slider.Maximum, val));
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
            double w = MainCanvas.ActualWidth;
            double h = MainCanvas.ActualHeight;
            _center = new Point(w / 2, h / 2);
            _thumbRadius = Thumb.Width / 2;

            double margin = Math.Max(TrackPath.StrokeThickness / 2, _thumbRadius);
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

            double percent = (Value - Minimum) / (Maximum - Minimum);
            double currentAngle = MinAngle + percent * SweepAngle;

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
            double rad = angleDeg * Math.PI / 180;
            double x = _center.X + radius * Math.Sin(rad);
            double y = _center.Y - radius * Math.Cos(rad);
            return new Point(x, y);
        }

        private void UpdateThumbPosition()
        {
            double percent = (Value - Minimum) / (Maximum - Minimum);
            double angleDeg = MinAngle + percent * SweepAngle;
            Point pos = GetPointOnCircle(angleDeg, _radius);
            Canvas.SetLeft(Thumb, pos.X - _thumbRadius);
            Canvas.SetTop(Thumb, pos.Y - _thumbRadius);
        }

        // Прокрутка мыши
        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double delta = e.Delta > 0 ? SmallChange : -SmallChange;
            double newValue = Value + delta;
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
            double dx = mousePos.X - _center.X;
            double dy = _center.Y - mousePos.Y;
            double angleRad = Math.Atan2(dy, dx);
            double angleDeg = 90 - angleRad * 180 / Math.PI;

            double clampedAngle = Math.Max(MinAngle, Math.Min(MaxAngle, angleDeg));
            double percent = (clampedAngle - MinAngle) / SweepAngle;
            double newValue = Minimum + percent * (Maximum - Minimum);
            Value = Math.Round(newValue, 2);
        }
    }
}