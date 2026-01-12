using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageHandle.ROI
{
    public enum ROIOperateType
    {
        TopLeftDrag,
        BottomRightDrag,
        TopRightDrag,
        BottomLeftDrag,
        CenterDrag,
        TopDrag,
        BottomDrag,
        LeftDrag,
        RightDrag,
        None,
    }

    public class ROICanvas : Canvas
    {
        private readonly int _roiMinWidth = 20;
        private readonly int _roiMinHeight = 20;

        private ROIOperateType operate = ROIOperateType.None;
        private Point lastPoint;
        private readonly ROIDrawingVisual roi;
        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            return roi;
        }

        public ROICanvas()
        {
            roi = new ROIDrawingVisual();
            this.AddLogicalChild(roi);
            this.AddVisualChild(roi);
            Background = Brushes.Transparent;

            // 在控件加载和尺寸改变时，确保根据真实尺寸重新约束并绘制
            this.Loaded += ROICanvas_Loaded;
            this.SizeChanged += ROICanvas_SizeChanged;
        }

        private void ROICanvas_Loaded(object? sender, RoutedEventArgs e)
        {
            // 在加载完成后，用当前实际大小约束并绘制
            TopLeftP = CoerceTopLeftP(TopLeftP, this);
            BottomRightP = CoerceBottomRightP(BottomRightP, this);
            roi.Draw(TopLeftP, BottomRightP, RecantangleBrush);
        }

        private void ROICanvas_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            // 大小变化时重新约束并绘制（例如窗口从 0 尺寸变为有值时）
            TopLeftP = CoerceTopLeftP(TopLeftP, this);
            BottomRightP = CoerceBottomRightP(BottomRightP, this);
            roi.Draw(TopLeftP, BottomRightP, RecantangleBrush);
        }

        private bool HitPointTest(Point target, Point point)
        {
            double offset = 8;

            if (point.X > target.X + offset)
                return false;

            if (point.X < target.X - offset)
                return false;

            if (point.Y > target.Y + offset)
                return false;

            if (point.Y < target.Y - offset)
                return false;

            return true;
        }

        private bool HitCneterTest(DrawingVisual target, Point point)
        {
            return target.ContentBounds.Contains(point);
        }

        private bool HitLineXTest(double x, Point point, double minY, double maxY)
        {
            double offset = 8;
            return (Math.Abs(x - point.X) < offset) && (point.Y >= minY) && (point.Y <= maxY);
        }

        private bool HitLineYTest(double y, Point point, double minX, double maxX)
        {
            double offset = 8;
            return (Math.Abs(y - point.Y) < offset) && (point.X >= minX) && (point.X <= maxX);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            operate = ROIOperateType.None;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point point = e.GetPosition(this);

            Point TopRight = new Point(BottomRightP.X, TopLeftP.Y);
            Point BottomLeft = new Point(TopLeftP.X, BottomRightP.Y);
            if (operate == ROIOperateType.None)
            {
                if (HitPointTest(TopLeftP, point))
                {
                    this.Cursor = Cursors.SizeNWSE;
                    if (e.LeftButton == MouseButtonState.Pressed)
                        operate = ROIOperateType.TopLeftDrag;
                }
                else if (HitPointTest(BottomRightP, point))
                {
                    this.Cursor = Cursors.SizeNWSE;

                    if (e.LeftButton == MouseButtonState.Pressed)
                        operate = ROIOperateType.BottomRightDrag;
                }
                else if (HitPointTest(TopRight, point))
                {
                    this.Cursor = Cursors.SizeNESW;

                    if (e.LeftButton == MouseButtonState.Pressed)
                        operate = ROIOperateType.TopRightDrag;
                }
                else if (HitPointTest(BottomLeft, point))
                {
                    this.Cursor = Cursors.SizeNESW;

                    if (e.LeftButton == MouseButtonState.Pressed)
                        operate = ROIOperateType.BottomLeftDrag;
                }
                else if (HitLineXTest(TopLeftP.X, point, TopLeftP.Y, BottomLeft.Y))
                {
                    this.Cursor = Cursors.SizeWE;

                    if (e.LeftButton == MouseButtonState.Pressed)
                        operate = ROIOperateType.LeftDrag;
                }
                else if (HitLineXTest(BottomRightP.X, point, TopLeftP.Y, BottomLeft.Y))
                {
                    this.Cursor = Cursors.SizeWE;

                    if (e.LeftButton == MouseButtonState.Pressed)
                        operate = ROIOperateType.RightDrag;
                }
                else if (HitLineYTest(TopLeftP.Y, point, TopLeftP.X, TopRight.X))
                {
                    this.Cursor = Cursors.SizeNS;

                    if (e.LeftButton == MouseButtonState.Pressed)
                        operate = ROIOperateType.TopDrag;
                }
                else if (HitLineYTest(BottomRightP.Y, point, TopLeftP.X, TopRight.X))
                {
                    this.Cursor = Cursors.SizeNS;

                    if (e.LeftButton == MouseButtonState.Pressed)
                        operate = ROIOperateType.BottomDrag;
                }
                else if (HitCneterTest(roi, point))
                {
                    this.Cursor = Cursors.SizeAll;
                    if (e.LeftButton == MouseButtonState.Pressed)
                        operate = ROIOperateType.CenterDrag;
                }
                else
                    this.Cursor = Cursors.Arrow;
            }

            switch (operate)
            {
                case ROIOperateType.None:
                    break;

                case ROIOperateType.TopLeftDrag:
                    {
                        TopLeftP = new Point(Math.Min(point.X, BottomRightP.X - _roiMinWidth), Math.Min(point.Y, BottomRightP.Y - _roiMinHeight));
                    }
                    break;

                case ROIOperateType.BottomRightDrag:
                    {
                        BottomRightP = new Point(Math.Max(point.X, TopLeftP.X + _roiMinWidth), Math.Max(point.Y, TopLeftP.Y + _roiMinHeight)); ;
                    }
                    break;

                case ROIOperateType.CenterDrag:
                    {
                        double xOffset = (point.X - lastPoint.X);//右方向为正
                        double yOffset = (point.Y - lastPoint.Y);//下方向为正

                        if (TopLeftP.X == 0 && xOffset < 0)//不能往左 xOffset不能小于0
                            break;
                        if (TopLeftP.Y == 0 && yOffset < 0)//不能往上 yOffset不能小于0
                            break;
                        if (BottomRightP.X == this.ActualWidth && xOffset > 0)// 不能往右  xOffset不能大于0
                            break;
                        if (BottomRightP.Y == this.ActualHeight && yOffset > 0)// 不能往下 yOffset不能大于0
                            break;

                        var topLeft = CoerceTopLeftP(new Point(TopLeftP.X + xOffset, TopLeftP.Y + yOffset), this);
                        var bottomRight = CoerceBottomRightP(new Point(BottomRightP.X + xOffset, BottomRightP.Y + yOffset), this);

                        if (TopLeftP != topLeft)
                            TopLeftP = topLeft;

                        if (BottomRightP != bottomRight)
                            BottomRightP = bottomRight;
                    }
                    break;

                case ROIOperateType.BottomLeftDrag:
                    {
                        TopLeftP = new Point(Math.Min(point.X, BottomRightP.X - _roiMinWidth), TopLeftP.Y);
                        BottomRightP = new Point(BottomRightP.X, Math.Max(point.Y, TopLeftP.Y + _roiMinHeight));
                    }
                    break;

                case ROIOperateType.TopRightDrag:
                    {
                        TopLeftP = new Point(TopLeftP.X, Math.Min(point.Y, BottomRightP.Y - _roiMinHeight));
                        BottomRightP = new Point(Math.Max(point.X, TopLeftP.X + _roiMinWidth), BottomRightP.Y);
                    }
                    break;

                case ROIOperateType.TopDrag:
                    {
                        TopLeftP = new Point(TopLeftP.X, Math.Min(point.Y, BottomRightP.Y - _roiMinHeight));
                    }
                    break;

                case ROIOperateType.BottomDrag:
                    {
                        BottomRightP = new Point(BottomRightP.X, Math.Max(point.Y, TopLeftP.Y + _roiMinHeight));
                    }
                    break;

                case ROIOperateType.LeftDrag:
                    {
                        TopLeftP = new Point(Math.Min(point.X, BottomRightP.X - _roiMinWidth), TopLeftP.Y);
                    }
                    break;

                case ROIOperateType.RightDrag:
                    {
                        BottomRightP = new Point(Math.Max(point.X, TopLeftP.X + _roiMinWidth), BottomRightP.Y);
                    }
                    break;
            }

            lastPoint = point;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            Point point = e.GetPosition(this);

            if (HitCneterTest(roi, point))
            {
                double offset;
                if (e.Delta > 0)
                    offset = -1;
                else
                    offset = 1;

                this.TopLeftP = new Point(TopLeftP.X + offset, TopLeftP.Y + offset);
                this.BottomRightP = new Point(BottomRightP.X - offset, BottomRightP.Y - offset);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            lastPoint = e.GetPosition(this);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            operate = ROIOperateType.None;
        }

        public Point BottomRightP
        {
            get
            {
                return ((Point)GetValue(BottomRightPProperty));
            }
            set
            {
                SetValue(BottomRightPProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for BottomRightP.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BottomRightPProperty =
            DependencyProperty.Register("BottomRightP", typeof(Point), typeof(ROICanvas),
                new FrameworkPropertyMetadata(new Point(0, 0),
                    new PropertyChangedCallback(OnBottomRightPPropertyChanged)));

        private static void OnBottomRightPPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ROICanvas canvas = (ROICanvas)d;
            Point value = ((Point)e.NewValue);

            Point coercedValue = CoerceBottomRightP(value, canvas);
            if (coercedValue != value)
            {
                canvas.BottomRightP = coercedValue;
            }

            canvas.roi.Draw(canvas.TopLeftP, canvas.BottomRightP, canvas.RecantangleBrush);
        }

        private static Point CoerceBottomRightP(Point point, ROICanvas canvas)
        {
            // 如果控件尚未布局（宽高为 0），不要用 ActualWidth/Height 强制约束，
            // 否则在初始化阶段会把坐标夹为 0。
            if (canvas.ActualWidth <= 0 || canvas.ActualHeight <= 0)
            {
                return point;
            }

            if (point.X < canvas.TopLeftP.X)
            {
                point.X = canvas.TopLeftP.X;
            }

            if (point.Y < canvas.TopLeftP.Y)
            {
                point.Y = canvas.TopLeftP.Y;
            }

            if (point.X > canvas.ActualWidth)
                point.X = canvas.ActualWidth;
            if (point.Y > canvas.ActualHeight)
                point.Y = canvas.ActualHeight;

            return point;
        }

        public Point TopLeftP
        {
            get { return (Point)GetValue(TopLeftPProperty); }
            set
            {
                SetValue(TopLeftPProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopLeftPProperty =
            DependencyProperty.Register("TopLeftP", typeof(Point), typeof(ROICanvas),
                new FrameworkPropertyMetadata(new Point(0, 0),
                    new PropertyChangedCallback(OnTopLeftPPropertyChanged)));

        private static void OnTopLeftPPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ROICanvas canvas = (ROICanvas)d;
            Point value = ((Point)e.NewValue);

            Point coercedValue = CoerceTopLeftP(value, canvas);
            if (coercedValue != value)
            {
                canvas.TopLeftP = coercedValue;
            }

            canvas.roi.Draw(canvas.TopLeftP, canvas.BottomRightP, canvas.RecantangleBrush);
        }

        private static Point CoerceTopLeftP(Point point, ROICanvas canvas)
        {
            // 如果控件尚未布局（宽高为 0），不要用 ActualWidth/Height 强制约束，
            // 否则在初始化阶段会把坐标夹为 0。
            if (canvas.ActualWidth <= 0 || canvas.ActualHeight <= 0)
            {
                return point;
            }
            if (point.X > canvas.BottomRightP.X)
                point.X = canvas.BottomRightP.X;

            if (point.Y > canvas.BottomRightP.Y)
                point.Y = canvas.BottomRightP.Y;

            if (point.X < 0)
                point.X = 0;
            if (point.Y < 0)
                point.Y = 0;

            return point;
        }

        public Brush RecantangleBrush
        {
            get { return (Brush)GetValue(RecantangleBrushProperty); }
            set { SetValue(RecantangleBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RecantangleBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RecantangleBrushProperty =
            DependencyProperty.Register("RecantangleBrush", typeof(Brush), typeof(ROICanvas),
                new FrameworkPropertyMetadata(null,
                new PropertyChangedCallback(OnRecantangleBrushPropertyChanged)));

        private static void OnRecantangleBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ROICanvas canvas = (ROICanvas)d;
            Brush value = ((Brush)e.NewValue);

            canvas.roi.Draw(canvas.TopLeftP, canvas.BottomRightP, canvas.RecantangleBrush);
        }
    }
}