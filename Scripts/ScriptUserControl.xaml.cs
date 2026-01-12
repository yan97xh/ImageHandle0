using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageHandle.Scripts
{
    /// <summary>
    /// 连接点被点击时的事件信息
    /// </summary>
    public class ConnectorClickedEventArgs : EventArgs
    {
        public Point CanvasPosition { get; }
        public string ConnectorName { get; }
        public ScriptUserControl SourceControl { get; }

        public ConnectorClickedEventArgs(Point canvasPosition, string connectorName, ScriptUserControl source)
        {
            CanvasPosition = canvasPosition;
            ConnectorName = connectorName;
            SourceControl = source;
        }
    }

    /// <summary>
    /// MUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ScriptUserControl : UserControl
    {
        // 对外事件：当某个连接点被点击时抛出
        public event EventHandler<ConnectorClickedEventArgs>? ConnectorClicked;

        // 对外事件：当控件位置发生变化时（拖动时触发）
        public event EventHandler? PositionChanged;

        private bool _isMouseDown;
        private bool _isDragging;
        private Point _mouseDownPos; // 相对于父 Canvas 的位置（用于判断阈值）
        private Point _dragOffset;   // 鼠标相对于控件左上角的偏移（用于保持抓取点）
        private Point _lastAllowedPos; // 最近一次允许的位置（用于防止重叠回退）
        private const double OverlapPadding = 4.0;

        public ScriptUserControl()
        {
            InitializeComponent();
        }

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelText.
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(ScriptUserControl), new PropertyMetadata(""));

        // 连接点点击处理（绑定在 XAML 上）
        private void Connector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 阻止冒泡到 UserControl 的拖动逻辑
            e.Handled = true;

            var canvas = FindAncestor<Canvas>(this);
            if (canvas == null) return;

            if (sender is not FrameworkElement ellipse) return;

            // 计算点击点在 Canvas 中的位置（圆心）
            var center = new Point(ellipse.ActualWidth / 2, ellipse.ActualHeight / 2);
            // 如果 TransformToAncestor 抛异常需保证 visual tree 已连接；此处假设控件已加入 Canvas
            var posInCanvas = ellipse.TransformToAncestor(canvas).Transform(center);

            string connectorName = ellipse.Name ?? "Unknown";
            ConnectorClicked?.Invoke(this, new ConnectorClickedEventArgs(posInCanvas, connectorName, this));
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.Capture((IInputElement)sender);
                _isMouseDown = true;
                _isDragging = false;

                var canvas = FindAncestor<Canvas>(this);
                if (canvas != null)
                {
                    _mouseDownPos = e.GetPosition(canvas);
                    _lastAllowedPos = new Point(SafeGetLeft(this), SafeGetTop(this));
                }
                else
                {
                    _mouseDownPos = e.GetPosition(this);
                    _lastAllowedPos = new Point(0, 0);
                }

                _dragOffset = e.GetPosition(this);
                e.Handled = true;
            }
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown || Mouse.Captured != sender) return;

            var canvas = FindAncestor<Canvas>(this);
            if (canvas == null) return;

            var pos = e.GetPosition(canvas);

            if (!_isDragging)
            {
                var dx = Math.Abs(pos.X - _mouseDownPos.X);
                var dy = Math.Abs(pos.Y - _mouseDownPos.Y);
                if (dx <= SystemParameters.MinimumHorizontalDragDistance &&
                    dy <= SystemParameters.MinimumVerticalDragDistance)
                {
                    return;
                }
                _isDragging = true;
            }

            // 计算候选位置（保持鼠标抓取点）
            double candidateLeft = pos.X - _dragOffset.X;
            double candidateTop = pos.Y - _dragOffset.Y;

            // 限制到父 Canvas 的范围内
            double minLeft = 0;
            double minTop = 0;
            double maxLeft = Math.Max(0, canvas.ActualWidth - this.ActualWidth);
            double maxTop = Math.Max(0, canvas.ActualHeight - this.ActualHeight);

            candidateLeft = Math.Max(minLeft, Math.Min(maxLeft, candidateLeft));
            candidateTop = Math.Max(minTop, Math.Min(maxTop, candidateTop));

            var candidateRect = new Rect(candidateLeft - OverlapPadding, candidateTop - OverlapPadding,
                                         this.ActualWidth + OverlapPadding * 2, this.ActualHeight + OverlapPadding * 2);

            // 检查与 Canvas 中其它 MUserControl 的碰撞
            bool intersects = false;
            foreach (var child in canvas.Children.OfType<ScriptUserControl>())
            {
                if (child == this) continue;

                double otherLeft = SafeGetLeft(child);
                double otherTop = SafeGetTop(child);
                double otherWidth = child.ActualWidth;
                double otherHeight = child.ActualHeight;

                if (otherWidth <= 0 || otherHeight <= 0) continue;

                var otherRect = new Rect(otherLeft, otherTop, otherWidth, otherHeight);
                if (candidateRect.IntersectsWith(otherRect))
                {
                    intersects = true;
                    break;
                }
            }

            if (!intersects)
            {
                Canvas.SetLeft(this, candidateLeft);
                Canvas.SetTop(this, candidateTop);
                _lastAllowedPos = new Point(candidateLeft, candidateTop);

                // 通知位置变更（MainWindow 会订阅并更新连接线）
                PositionChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // 回退到上一次允许的位置
                Canvas.SetLeft(this, _lastAllowedPos.X);
                Canvas.SetTop(this, _lastAllowedPos.Y);
            }

            e.Handled = true;
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured == sender)
            {
                Mouse.Capture(null);
            }
            _isMouseDown = false;
            _isDragging = false;

            // 抬起时也通知位置改变（确保最终位置被同步）
            PositionChanged?.Invoke(this, EventArgs.Empty);

            e.Handled = true;
        }

        /// <summary>
        /// 获取指定连接点在指定 Canvas 的坐标（圆心）。
        /// Canvas 必须是该控件的祖先。
        /// </summary>
        public Point GetConnectorPosition(string connectorName, Canvas canvas)
        {
            if (canvas == null) throw new ArgumentNullException(nameof(canvas));

            var element = this.FindName(connectorName) as FrameworkElement;
            if (element == null)
            {
                // 如果找不到指定连接点，返回控件中心点（相对于 Canvas）
                double left = Canvas.GetLeft(this);
                double top = Canvas.GetTop(this);
                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;
                return new Point(left + this.ActualWidth / 2, top + this.ActualHeight / 2);
            }

            var center = new Point(element.ActualWidth / 2, element.ActualHeight / 2);
            var pointInCanvas = element.TransformToAncestor(canvas).Transform(center);
            return pointInCanvas;
        }

        // 安全读取 Canvas.Left（处理 NaN）
        private static double SafeGetLeft(UIElement element)
        {
            double left = Canvas.GetLeft(element);
            return double.IsNaN(left) ? 0 : left;
        }

        private static double SafeGetTop(UIElement element)
        {
            double top = Canvas.GetTop(element);
            return double.IsNaN(top) ? 0 : top;
        }

        // 辅助：向上查找指定类型的祖先
        private static T? FindAncestor<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
    }
}