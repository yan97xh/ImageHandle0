using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageHandle.ROI
{
    public enum PolygonROIOperateType
    {
        None,
        ReadyToDraw,
        DrawDone,
    }

    public class PolygonROICanvas : Canvas
    {
        private readonly int _minDistance = 10; // 两顶点间的最小距离
        private readonly int _minGap = 5; // 顶点与边界的最小距离

        private Point lastPoint;
        private readonly PolygonROIDrawingVisual roi;
        private PolygonROIOperateType operate = PolygonROIOperateType.None;
        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            return roi;
        }

        public PolygonROICanvas()
        {
            roi = new PolygonROIDrawingVisual();
            this.AddLogicalChild(roi);
            this.AddVisualChild(roi);
            Background = Brushes.Transparent;
            PointCollection = new ObservableCollection<Point>();
        }

        /// <summary>
        /// 是否可以开始绘制ROI
        /// </summary>
        public bool EnableDraw
        {
            get { return (bool)GetValue(EnableDrawProperty); }
            set { SetValue(EnableDrawProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Enable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableDrawProperty =
            DependencyProperty.Register("EnableDraw",
                typeof(bool), typeof(PolygonROICanvas),
                new PropertyMetadata(false, OnEnableDrawPropertyChanged));

        private static void OnEnableDrawPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PolygonROICanvas canvas)
                return;
            if (e.NewValue == e.OldValue)
            {
                return;
            }
            if (e.NewValue is bool enable && enable == true)
            {
                canvas.operate = PolygonROIOperateType.ReadyToDraw;
                canvas.PointCollection.Clear();
            }
        }

        public static readonly DependencyProperty PointCollectionProperty =
           DependencyProperty.Register(
               nameof(PointCollection),
               typeof(ObservableCollection<Point>),
               typeof(PolygonROICanvas),
               new PropertyMetadata(null,
                  OnItemsPropertyChanged));

        public ObservableCollection<Point> PointCollection
        {
            get => (ObservableCollection<Point>)GetValue(PointCollectionProperty);
            set => SetValue(PointCollectionProperty, value);
        }

        private static void OnItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PolygonROICanvas canvas)
                return;

            if (e.OldValue is ObservableCollection<Point> oldCollection)
            {
                oldCollection.CollectionChanged -= canvas.PointCollection_CollectionChanged;
            }

            if (e.NewValue is ObservableCollection<Point> newCollection)
            {
                newCollection.CollectionChanged += canvas.PointCollection_CollectionChanged;
            }

            // 初始更新（包括 newCollection == null 的情况）
            canvas.UpdateDrawing();
        }

        public Brush PolygonROIBrush
        {
            get { return (Brush)GetValue(PolygonROIBrushProperty); }
            set { SetValue(PolygonROIBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RecantangleBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PolygonROIBrushProperty =
            DependencyProperty.Register("PolygonROIBrush", typeof(Brush), typeof(PolygonROICanvas),
                new FrameworkPropertyMetadata(null,
                new PropertyChangedCallback(OnRecantangleBrushPropertyChanged)));

        private static void OnRecantangleBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PolygonROICanvas canvas = (PolygonROICanvas)d;
            Brush value = ((Brush)e.NewValue);

            canvas.roi.Draw(canvas.PointCollection?.ToList(), canvas.PolygonROIBrush);
        }

        private void PointCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateDrawing();
        }

        private void UpdateDrawing()
        {
            // 确保在 UI 线程执行绘制
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(UpdateDrawing);
                return;
            }

            var points = PointCollection?.ToList();
            if (points == null || points.Count == 0)
            {
                // 清空绘制（传入空列表）
                roi.Draw(new List<Point>(), PolygonROIBrush);
                return;
            }

            roi.Draw(points, PolygonROIBrush);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point point = e.GetPosition(this);
            if (operate == PolygonROIOperateType.ReadyToDraw)
            {
                List<Point> tempList = PointCollection?.ToList();
                if (tempList == null || tempList.Count == 0)
                {
                    return;
                }
                tempList.Add(point);
                tempList = SortPointsForClosedPolygon(tempList);
                roi.Draw(tempList, PolygonROIBrush);
            }
            else if (operate == PolygonROIOperateType.DrawDone)
            {
                var res = GetNearestPoint(point, PointCollection?.ToList());
                if (res.Item1 >= 0 && res.Item2 <= 8) // 是否选中点
                {
                    this.Cursor = Cursors.Hand;
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        double x = point.X;
                        double y = point.Y;
                        if (x < _minGap)
                        {
                            x = _minGap;
                        }
                        else if (x > this.ActualWidth - _minGap)
                        {
                            x = this.ActualWidth - _minGap;
                        }
                        if (y < _minGap)
                        {
                            y = _minGap;
                        }
                        else if (y > this.ActualHeight - _minGap)
                        {
                            y = this.ActualHeight - _minGap;
                        }
                        // 移动点
                        PointCollection[res.Item1] = new Point(x, y);
                        //对集合进行重新排序，保证多边形闭合且不自交
                        PointCollection = new ObservableCollection<Point>(SortPointsForClosedPolygon(PointCollection?.ToList()));
                        roi.Draw(PointCollection?.ToList(), PolygonROIBrush);
                    }
                }
                else if (IsPointInPolygon(point, PointCollection?.ToList())) // 是否在多边形内
                {
                    this.Cursor = Cursors.SizeAll;
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        double minX = PointCollection.Min(p => p.X);
                        double minY = PointCollection.Min(p => p.Y);
                        double maxX = PointCollection.Max(p => p.X);
                        double maxY = PointCollection.Max(p => p.Y);
                        double stepX = point.X - lastPoint.X;
                        double stepY = point.Y - lastPoint.Y;
                        double finalXStep = stepX;
                        double finalYStep = stepY;
                        if (stepX < 0) // 左移
                        {
                            if (minX + stepX < _minGap)
                            {
                                finalXStep = _minGap - minX;
                            }
                        }
                        else // 右
                        {
                            if (maxX + stepX > this.ActualWidth - _minGap)
                            {
                                finalXStep = this.ActualWidth - _minGap - maxX;
                            }
                        }

                        if (stepY < 0) // 上
                        {
                            if (minY + stepY < _minGap)
                            {
                                finalYStep = _minGap - minY;
                            }
                        }
                        else // 下
                        {
                            if (maxY + stepY > this.ActualHeight - _minGap)
                            {
                                finalYStep = this.ActualHeight - _minGap - maxY;
                            }
                        }

                        PointCollection = new ObservableCollection<Point>(
                            PointCollection.Select(point => new Point(point.X + finalXStep, point.Y + finalYStep)));
                        roi.Draw(PointCollection?.ToList(), PolygonROIBrush);
                    }
                }
                else
                {
                    this.Cursor = Cursors.Arrow;
                }
            }
            lastPoint = point;
        }

        // 找到pointA在pointList中最近的点的索引
        // 返回索引和距离
        public (int, double) GetNearestPoint(Point pointA, List<Point> pointList)
        {
            int index = -1;
            if (pointList == null || pointList.Count == 0)
                return (index, -1);
            double minDistance = double.MaxValue;

            for (int i = 0; i < pointList.Count; i++)
            {
                double dx = pointList[i].X - pointA.X;
                double dy = pointList[i].Y - pointA.Y;
                double distance = dx * dx + dy * dy; // 使用平方距离比较，避免开方

                if (distance < minDistance)
                {
                    minDistance = distance;
                    index = i;
                }
            }

            return (index, Math.Sqrt(minDistance));
        }

        private DateTime _lastClickTime;
        private const int DoubleClickThreshold = 200; // 双击时间间隔阈值，单位为毫秒

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            // 双击
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < DoubleClickThreshold)
            {
                EnableDraw = false;
                operate = PolygonROIOperateType.DrawDone;
                e.Handled = true;
            }
            else // 单击
            {
                if (operate == PolygonROIOperateType.ReadyToDraw)
                {
                    Point point = e.GetPosition(this);
                    if (point.X < 0 || point.X > this.ActualWidth)
                    {
                        return;
                    }
                    if (point.Y < 0 || point.Y > this.ActualHeight)
                    {
                        return;
                    }
                    if (IsPointTooClose(point, PointCollection?.ToList()))
                    {
                        return;
                    }
                    // 单击增加点
                    PointCollection.Add(point);
                    lastPoint = e.GetPosition(this);
                }
            }
            _lastClickTime = DateTime.Now;
        }

        // 判断新点是否与已有点过近
        private bool IsPointTooClose(Point newPoint, List<Point> existingPoints)
        {
            if (existingPoints == null || existingPoints.Count == 0)
            {
                return false;
            }
            foreach (var point in existingPoints)
            {
                if (GetDistance(newPoint, point) < _minDistance)
                    return true;
            }
            return false;
        }

        private double GetDistance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return System.Math.Sqrt(dx * dx + dy * dy);
        }

        // 判断点是否在多边形内
        private bool IsPointInPolygon(Point testPoint, List<Point> points)
        {
            // 基本检查
            if (points == null || points.Count < 3)
                return false;

            // 特殊情况：点在点上
            foreach (var point in points)
            {
                if (Math.Abs(point.X - testPoint.X) < 0.0001 &&
                    Math.Abs(point.Y - testPoint.Y) < 0.0001)
                    return true;
            }

            // 按角度排序
            var sorted = SortPointsClockwise(points);

            // 射线法
            bool inside = false;
            int n = sorted.Count;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var pi = sorted[i];
                var pj = sorted[j];

                // 检查是否在边上
                if (IsPointOnSegment(testPoint, pi, pj))
                    return true;

                // 射线相交检查
                if ((pi.Y > testPoint.Y) != (pj.Y > testPoint.Y))
                {
                    double intersectX = (pj.X - pi.X) * (testPoint.Y - pi.Y) / (pj.Y - pi.Y) + pi.X;

                    if (testPoint.X < intersectX)
                        inside = !inside;
                }
            }

            return inside;
        }

        private List<Point> SortPointsClockwise(List<Point> points)
        {
            double cx = points.Average(p => p.X);
            double cy = points.Average(p => p.Y);

            return points.OrderBy(p => Math.Atan2(p.Y - cy, p.X - cx)).ToList();
        }

        private bool IsPointOnSegment(Point p, Point a, Point b)
        {
            // 检查点是否在线段ab上
            double cross = (p.X - a.X) * (b.Y - a.Y) - (p.Y - a.Y) * (b.X - a.X);

            if (Math.Abs(cross) > 0.0001) // 不共线
                return false;

            // 在边界框内
            return p.X >= Math.Min(a.X, b.X) && p.X <= Math.Max(a.X, b.X) &&
                   p.Y >= Math.Min(a.Y, b.Y) && p.Y <= Math.Max(a.Y, b.Y);
        }

        private List<Point> SortPointsForClosedPolygon(List<Point> points)
        {
            if (points == null || points.Count < 3)
                return points;

            // 计算中心点
            Point center = GetCenter(points);

            // 按角度排序
            return points.OrderBy(p => Math.Atan2(p.Y - center.Y, p.X - center.X))
                         .ToList();
        }

        private Point GetCenter(List<Point> points)
        {
            double sumX = 0, sumY = 0;
            foreach (var point in points)
            {
                sumX += point.X;
                sumY += point.Y;
            }
            return new Point(sumX / points.Count, sumY / points.Count);
        }
    }
}