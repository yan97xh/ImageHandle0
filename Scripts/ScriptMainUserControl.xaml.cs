using ImageHandle.Helpers;
using OpenCvSharp;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImageHandle.Scripts
{
    /// <summary>
    /// ScriptMainUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ScriptMainUserControl : UserControl
    {
        private ScriptViewModel _viewModel;

        private bool _isConnecting; // 是否正在连接中
        private ScriptUserControl? _startControl; // 连接线的起始控件
        private string? _startConnectorName; // 起始控件的连接点名称 BottomConnector TopConnector LeftConnector RightConnector
        private System.Windows.Point _startPoint; // 起始点在 Canvas 中的坐标
        private Polyline? _tempPolyline; // 用来绘制选中起始点还未选择结束点时的临时折线

        // 存储最终连接关系（用于拖动时实时更新）
        private readonly List<Connection> _connections = new();

        // 存储每个控件对应的方法参数列表
        private Dictionary<ScriptUserControl, List<ScriptParamModel>> _controlParamDic = new Dictionary<ScriptUserControl, List<ScriptParamModel>>();

        public ScriptMainUserControl()
        {
            ScriptViewModel scriptViewModel = new ScriptViewModel();
            DataContext = scriptViewModel;
            _viewModel = scriptViewModel;

            InitializeComponent();
            Loaded += ScriptMainUserControl_Loaded;

            // 确保在卸载时清理 Window 事件订阅
            Unloaded += ScriptMainUserControl_Unloaded;
        }

        private void ScriptMainUserControl_Unloaded(object? sender, RoutedEventArgs e)
        {
            // 取消订阅避免内存泄漏或悬挂处理器
            DetachWindowKeyHandler();
        }

        private void ScriptMainUserControl_Loaded(object? sender, RoutedEventArgs e)
        {
            // 为 Canvas 中已有的 MUserControl 订阅连接点和位置变更事件
            SubscribeToControlsInCanvas();
        }

        private void SubscribeToControlsInCanvas()
        {
            foreach (var ctrl in ScriptCanvas.Children.OfType<ScriptUserControl>())
            {
                ctrl.ConnectorClicked -= Ctrl_ConnectorClicked;
                ctrl.ConnectorClicked += Ctrl_ConnectorClicked;

                ctrl.PositionChanged -= Ctrl_PositionChanged;
                ctrl.PositionChanged += Ctrl_PositionChanged;
            }
        }

        private void MethodsListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = MethodsListBox.ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
            if (item == null) return;

            // 获取显示文本
            var content = item.Content?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(content)) return;

            // 使用自定义数据格式启动拖放
            var data = new DataObject("DragDropItem", content);
            DragDrop.DoDragDrop(MethodsListBox, data, DragDropEffects.Copy);
        }

        private void ScriptCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = sender as Canvas;
            System.Windows.Point hitPoint = e.GetPosition(canvas);
            // 命中测试
            HitTestResult result = VisualTreeHelper.HitTest(canvas, hitPoint);
            if (result != null)
            {
                DependencyObject hitObject = result.VisualHit;
                while (hitObject != null && !(hitObject is ScriptUserControl) && !(hitObject is Polyline))
                {
                    hitObject = VisualTreeHelper.GetParent(hitObject);
                }

                if (hitObject is ScriptUserControl clickedRect)// 右键点击控件
                {
                    ContextMenu contextMenu = new ContextMenu();
                    // Style 设置为全局自定义的ContextMenu样式
                    contextMenu.Style = (Style)FindResource("ModernContextMenuStyle");
                    MenuItem paramItem = new MenuItem
                    {
                        Header = "参数",
                        Tag = clickedRect
                    };
                    paramItem.Click += ParamItem_Click;
                    MenuItem deleteItem = new MenuItem
                    {
                        Header = "删除",
                        Tag = clickedRect
                    };
                    deleteItem.Click += DeleteItem_Click;

                    contextMenu.Items.Add(paramItem);
                    contextMenu.Items.Add(deleteItem);
                    contextMenu.IsOpen = true;
                    // 清理事件订阅 避免内存泄漏
                    contextMenu.Closed += (s, e) =>
                    {
                        paramItem.Click -= ParamItem_Click;
                        deleteItem.Click -= DeleteItem_Click;
                        contextMenu.Items.Clear();
                    };
                    e.Handled = true;
                }
                else if (hitObject is Polyline polyline) // 右键点击折线
                {
                    ContextMenu contextMenu = new ContextMenu();
                    MenuItem deletePolylineItem = new MenuItem
                    {
                        Header = "删除",
                        Tag = polyline
                    };
                    deletePolylineItem.Click += DeletePolyline;

                    contextMenu.Items.Add(deletePolylineItem);
                    contextMenu.IsOpen = true;
                    contextMenu.Closed += (s, e) =>
                    {
                        deletePolylineItem.Click -= DeletePolyline;
                        contextMenu.Items.Clear();
                    };
                    e.Handled = true;
                }
            }
        }

        private void DeletePolyline(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is Polyline polyline)
            {
                // 找出对应的连接关系
                var conn = _connections.FirstOrDefault(x => x.Line == polyline);
                if (conn != null)
                {
                    // 移除折线和箭头
                    if (conn.Line != null && ScriptCanvas.Children.Contains(conn.Line))
                    {
                        ScriptCanvas.Children.Remove(conn.Line);
                    }
                    if (conn.Arrow != null && ScriptCanvas.Children.Contains(conn.Arrow))
                    {
                        ScriptCanvas.Children.Remove(conn.Arrow);
                    }
                    _connections.Remove(conn);

                    // 强制更新布局（可选，但有助于立即刷新视觉）
                    ScriptCanvas.UpdateLayout();
                }
            }
        }

        private void ParamItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ScriptUserControl tag)
            {
                if (_controlParamDic.TryGetValue(tag, out var paramModels))
                {
                    if (paramModels.Count == 0)
                    {
                        MessageBox.Show($"{tag.LabelText} 该方法无其他参数", "参数信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        _controlParamDic[tag] = new ParamWindow(paramModels).Show();
                    }
                }
                else
                {
                    MessageBox.Show("该控件没有参数信息。", "参数信息", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ScriptUserControl tag)
            {
                // 找出所有与该控件相关的连接（起点或终点）
                var toRemove = _connections.Where(x => x.StartControl == tag || x.EndControl == tag).ToList();

                // 先移除对应的可视元素（折线与箭头），再从连接列表中删除记录
                foreach (var conn in toRemove)
                {
                    if (conn.Line != null && ScriptCanvas.Children.Contains(conn.Line))
                    {
                        ScriptCanvas.Children.Remove(conn.Line);
                    }

                    if (conn.Arrow != null && ScriptCanvas.Children.Contains(conn.Arrow))
                    {
                        ScriptCanvas.Children.Remove(conn.Arrow);
                    }

                    _connections.Remove(conn);
                }

                // 退订事件监听，避免悬挂引用
                tag.ConnectorClicked -= Ctrl_ConnectorClicked;
                tag.PositionChanged -= Ctrl_PositionChanged;

                // 从 Canvas 中移除控件
                if (ScriptCanvas.Children.Contains(tag))
                {
                    ScriptCanvas.Children.Remove(tag);
                }
                // 强制更新布局（可选，但有助于立即刷新视觉）
                ScriptCanvas.UpdateLayout();
            }
        }

        private void ScriptCanvas_Drop(object sender, DragEventArgs e)
        {
            // 读取拖放的数据文本
            string text = null;
            if (e.Data.GetDataPresent("DragDropItem"))
            {
                text = e.Data.GetData("DragDropItem") as string;
            }
            else if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                text = e.Data.GetData(DataFormats.StringFormat) as string;
            }

            if (string.IsNullOrEmpty(text)) return;

            var canvas = sender as Canvas;
            var pos = e.GetPosition(canvas);

            var tb = new ScriptUserControl()
            {
                LabelText = text, 
            };

            _controlParamDic[tb] = ScriptService.Instance.Navigate(text, _viewModel.IsCN);

            Canvas.SetLeft(tb, Math.Min(pos.X, canvas.ActualWidth - tb.Width));
            Canvas.SetTop(tb, Math.Min(pos.Y, canvas.ActualHeight - tb.Height));

            canvas.Children.Add(tb);

            // 订阅新创建控件的连接点事件与位置变化事件（确保能绘制线并在拖动时更新）
            tb.ConnectorClicked -= Ctrl_ConnectorClicked;
            tb.ConnectorClicked += Ctrl_ConnectorClicked;

            tb.PositionChanged -= Ctrl_PositionChanged;
            tb.PositionChanged += Ctrl_PositionChanged;

            e.Handled = true;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                bool isChecked = checkBox.IsChecked ?? false;
                _viewModel.IsCN = isChecked; // 更改 ViewModel 中的属性  Listbox自动更新
                //更新canvas 显示
                foreach (var ctrl in ScriptCanvas.Children.OfType<ScriptUserControl>())
                {
                    ctrl.LabelText = ScriptService.Instance.GetMethodName(ctrl.LabelText, isChecked);
                }
            }
        }

        private void Ctrl_ConnectorClicked(object? sender, ConnectorClickedEventArgs e)
        {
            var canvas = ScriptCanvas;
            if (canvas == null) return;

            if (!_isConnecting)
            {
                // 开始连接：记录起点并添加临时折线（曼哈顿折线预览）
                _isConnecting = true;
                _startControl = e.SourceControl;
                _startConnectorName = e.ConnectorName;
                _startPoint = e.CanvasPosition;
                _tempPolyline = new Polyline
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    IsHitTestVisible = false,
                    Points = BuildManhattanPoints(_startPoint, _startPoint)
                };
                // 将临时线加到 Canvas（放在较低层，避免拦截连接点）
                canvas.Children.Add(_tempPolyline);
                Panel.SetZIndex(_tempPolyline, 0);
                canvas.MouseMove += Canvas_MouseMove;
                AttachWindowKeyHandler();
            }
            else
            {
                // 如果起点和终点是同一个控件，取消连接并提示（不允许自连）
                if (_startControl == e.SourceControl)
                {
                    // 移除临时线并清理状态
                    if (_tempPolyline != null && canvas.Children.Contains(_tempPolyline))
                    {
                        canvas.Children.Remove(_tempPolyline);
                        _tempPolyline = null;
                    }
                    canvas.MouseMove -= Canvas_MouseMove;
                    this.KeyDown -= MainWindow_KeyDown;
                    _isConnecting = false;
                    _startControl = null;
                    _startConnectorName = null;
                    return;
                }
                // 结束连接：用终点创建最终折线（曼哈顿折线），并记录连接关系
                var endPoint = e.CanvasPosition;

                var finalLine = new Polyline
                {
                    Stroke = Brushes.Blue,
                    StrokeThickness = 2,
                    IsHitTestVisible = false,
                    Points = BuildManhattanPoints(_startPoint, endPoint)
                };

                if (_tempPolyline != null) canvas.Children.Remove(_tempPolyline);
                _tempPolyline = null;
                canvas.Children.Add(finalLine);
                Panel.SetZIndex(finalLine, 0);

                // 保存连接关系（关联控件与对应连接点名字），并为该连接创建箭头
                if (_startControl != null && e.SourceControl != null)
                {
                    var conn = new Connection
                    {
                        StartControl = _startControl,
                        StartConnectorName = _startConnectorName ?? "",
                        EndControl = e.SourceControl,
                        EndConnectorName = e.ConnectorName,
                        Line = finalLine
                    };

                    // 创建箭头并关联到 connection（使用线的最后一段方向）
                    var pts = finalLine.Points;
                    if (pts.Count >= 2)
                    {
                        var tip = pts[pts.Count - 1];
                        var prev = pts[pts.Count - 2];
                        conn.Arrow = CreateOrUpdateArrow(null, tip, prev, finalLine.Stroke);
                        // 将箭头放在同一 canvas 中
                        if (conn.Arrow != null && !canvas.Children.Contains(conn.Arrow))
                        {
                            canvas.Children.Add(conn.Arrow);
                            Panel.SetZIndex(conn.Arrow, 0);
                        }
                    }

                    _connections.Add(conn);
                }

                // 清理
                canvas.MouseMove -= Canvas_MouseMove;
                // 取消订阅 Window 的键盘处理
                DetachWindowKeyHandler();

                _isConnecting = false;
                _startControl = null;
                _startConnectorName = null;
            }
        }

        // 当 控件位置改变时触发，更新相关连接线端点
        private void Ctrl_PositionChanged(object? sender, EventArgs e)
        {
            if (sender is not ScriptUserControl moved) return;

            // 查找所有包含该控件的连接并更新线段
            foreach (var conn in _connections)
            {
                if (conn.StartControl == moved || conn.EndControl == moved)
                {
                    UpdateConnectionLine(conn);
                }
            }
        }

        private void UpdateConnectionLine(Connection conn)
        {
            if (conn.StartControl == null || conn.EndControl == null || conn.Line == null) return;

            // 通过控件提供的方法计算连接点在 Canvas 的坐标
            var startPt = conn.StartControl.GetConnectorPosition(conn.StartConnectorName, ScriptCanvas);
            var endPt = conn.EndControl.GetConnectorPosition(conn.EndConnectorName, ScriptCanvas);

            // 更新折线点
            conn.Line.Points = BuildManhattanPoints(startPt, endPt);

            // 更新/创建箭头（基于最后两点方向）
            var pts = conn.Line.Points;
            if (pts.Count >= 2)
            {
                var tip = pts[pts.Count - 1];
                var prev = pts[pts.Count - 2];
                conn.Arrow = CreateOrUpdateArrow(conn.Arrow, tip, prev, conn.Line.Stroke);
                if (conn.Arrow != null && !ScriptCanvas.Children.Contains(conn.Arrow))
                {
                    ScriptCanvas.Children.Add(conn.Arrow);
                    Panel.SetZIndex(conn.Arrow, 0);
                }
            }
        }

        // 订阅/退订宿主 Window 的键盘事件（使用 PreviewKeyDown 更可靠）
        private void AttachWindowKeyHandler()
        {
            var wnd = System.Windows.Window.GetWindow(this);
            if (wnd == null) return;

            // 先移除再添加，防止重复订阅
            wnd.PreviewKeyDown -= MainWindow_KeyDown;
            wnd.PreviewKeyDown += MainWindow_KeyDown;

            // 确保 Window 获得焦点（使键盘事件能到达）
            if (!wnd.IsActive)
            {
                wnd.Activate();
            }
            // 强制把键盘焦点设置到 Window（或可设置到特定元素）
            Keyboard.Focus(wnd);
        }

        private void DetachWindowKeyHandler()
        {
            var wnd = System.Windows.Window.GetWindow(this);
            if (wnd == null) return;
            wnd.PreviewKeyDown -= MainWindow_KeyDown;
        }

        // 创建或更新箭头 Path；箭头在几何上以 (0,0) 为箭尖（指向 +X），再旋转平移到目标位置
        private System.Windows.Shapes.Path CreateOrUpdateArrow(System.Windows.Shapes.Path? arrow, System.Windows.Point tip, System.Windows.Point prev, Brush? fill)
        {
            const double arrowLength = 12.0;
            const double arrowHalfWidth = 6.0;

            if (arrow == null)
            {
                arrow = new System.Windows.Shapes.Path
                {
                    Fill = (fill as SolidColorBrush) ?? Brushes.Black,
                    Stroke = null,
                    IsHitTestVisible = false
                };
            }

            // 创建三角形几何： 顶点在 (0,0)，底边两点在 (-arrowLength, -arrowHalfWidth) 和 (-arrowLength, arrowHalfWidth)
            var geom = new StreamGeometry();
            using (var ctx = geom.Open())
            {
                ctx.BeginFigure(new System.Windows.Point(0, 0), true, true);
                ctx.LineTo(new System.Windows.Point(-arrowLength, -arrowHalfWidth), true, false);
                ctx.LineTo(new System.Windows.Point(-arrowLength, arrowHalfWidth), true, false);
            }
            geom.Freeze();
            arrow.Data = geom;

            // 计算方向角度（基于 tip - prev）
            double dx = tip.X - prev.X;
            double dy = tip.Y - prev.Y;
            double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;

            // 先旋转，再平移到 tip（箭尖位置）
            var tg = new TransformGroup();
            tg.Children.Add(new RotateTransform(angle));
            tg.Children.Add(new TranslateTransform(tip.X, tip.Y));
            arrow.RenderTransform = tg;

            return arrow;
        }

        private void Canvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_isConnecting || _tempPolyline == null) return;
            var canvas = ScriptCanvas;
            var pos = e.GetPosition(canvas);

            // 更新临时折线为曼哈顿折线（根据鼠标当前位置作为终点）
            _tempPolyline.Points = BuildManhattanPoints(_startPoint, pos);
        }

        private void MainWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _isConnecting)
            {
                // 取消连接
                if (_tempPolyline != null) ScriptCanvas.Children.Remove(_tempPolyline);
                _tempPolyline = null;
                ScriptCanvas.MouseMove -= Canvas_MouseMove;
                DetachWindowKeyHandler();
                _isConnecting = false;
                _startControl = null;
                _startConnectorName = null;
            }
        }

        // 生成简单的“曼哈顿”折线路径（水平/垂直折线）
        private static PointCollection BuildManhattanPoints(System.Windows.Point start, System.Windows.Point end)
        {
            var pts = new PointCollection();

            // 如果水平或垂直重合，直接用两点
            if (Math.Abs(start.X - end.X) < 1e-6 || Math.Abs(start.Y - end.Y) < 1e-6)
            {
                pts.Add(start);
                pts.Add(end);
                return pts;
            }

            pts.Add(start);
            pts.Add(new System.Windows.Point(start.X, (start.Y + end.Y) / 2));
            pts.Add(new System.Windows.Point(end.X, (start.Y + end.Y) / 2));
            pts.Add(end);

            return pts;
        }

        /// <summary>
        /// 获取所有连接链路（每条链路为按顺序的 Connection 列表）。
        /// 算法：构建有向图（StartControl -> EndControl），
        /// 从入度为0的节点开始 DFS，得到所有从源到汇的路径；若无入度为0的源（存在环），则从每个节点开始搜索并在遇到回路时收集该回路路径。
        /// </summary>
        public List<List<Connection>> GetAllConnectionPaths()
        {
            // 构建邻接表（StartControl -> list of Connection）
            var adj = new Dictionary<ScriptUserControl, List<Connection>>();
            var nodes = new HashSet<ScriptUserControl>();
            var indegree = new Dictionary<ScriptUserControl, int>();

            foreach (var c in _connections)
            {
                if (c.StartControl == null || c.EndControl == null) continue;

                nodes.Add(c.StartControl);
                nodes.Add(c.EndControl);

                if (!adj.TryGetValue(c.StartControl, out var list))
                {
                    list = new List<Connection>();
                    adj[c.StartControl] = list;
                }
                list.Add(c);

                if (!indegree.ContainsKey(c.StartControl)) indegree[c.StartControl] = 0;
                if (!indegree.ContainsKey(c.EndControl)) indegree[c.EndControl] = 0;
                indegree[c.EndControl] = indegree[c.EndControl] + 1;
            }

            // 确保所有节点在 indegree 中存在
            foreach (var n in nodes)
            {
                if (!indegree.ContainsKey(n)) indegree[n] = 0;
            }

            var results = new List<List<Connection>>();

            // 找到所有入度为0的源节点
            var sources = nodes.Where(n => indegree.TryGetValue(n, out var d) && d == 0).ToList();

            // 如果没有源（可能全部在环中），使用所有节点作为起点以便发现环
            var startNodes = sources.Count > 0 ? sources : nodes.ToList();

            foreach (var start in startNodes)
            {
                var visited = new HashSet<ScriptUserControl> { start };
                DFSPaths(start, new List<Connection>(), visited, adj, results);
            }

            // 去重（不同起点或路径可能产生重复）
            var unique = new List<List<Connection>>();
            var seen = new HashSet<string>();
            foreach (var path in results)
            {
                var key = PathKey(path);
                if (!seen.Contains(key))
                {
                    seen.Add(key);
                    unique.Add(path);
                }
            }

            return unique;
        }

        // 递归 DFS，path 中保存已经走过的 Connection
        private void DFSPaths(ScriptUserControl current,
                              List<Connection> path,
                              HashSet<ScriptUserControl> visited,
                              Dictionary<ScriptUserControl, List<Connection>> adj,
                              List<List<Connection>> results)
        {
            if (!adj.TryGetValue(current, out var outs) || outs.Count == 0)
            {
                // 到达末端节点（无出边），把当前 path 作为一条完整链路
                if (path.Count > 0)
                {
                    results.Add(new List<Connection>(path));
                }
                return;
            }

            foreach (var edge in outs)
            {
                var next = edge.EndControl;
                if (next == null) continue;

                if (visited.Contains(next))
                {
                    // 遇到回路：把当前路径 + 该边视作一条“回路链”
                    var cycle = new List<Connection>(path) { edge };
                    results.Add(cycle);
                    continue;
                }

                visited.Add(next);
                path.Add(edge);

                DFSPaths(next, path, visited, adj, results);

                path.RemoveAt(path.Count - 1);
                visited.Remove(next);
            }
        }

        // 生成路径唯一键（便于去重）：用控件 LabelText 和 连接点名串联
        private static string PathKey(List<Connection> path)
        {
            var sb = new StringBuilder();
            foreach (var c in path)
            {
                var a = c.StartControl?.LabelText ?? "null";
                var b = c.EndControl?.LabelText ?? "null";
                sb.Append($"{a}[{c.StartConnectorName}]->{b}[{c.EndConnectorName}]|");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 返回可读的字符串链路列表，例如：
        /// "A --(Top->Left)-> B --(Right->Bottom)-> C"
        /// </summary>
        public List<string> GetAllConnectionPathsAsStrings()
        {
            var paths = GetAllConnectionPaths();
            var result = new List<string>();
            foreach (var path in paths)
            {
                if (path.Count == 0) continue;
                var sb = new StringBuilder();
                // 起点
                sb.Append(path[0].StartControl?.LabelText ?? "null");
                foreach (var edge in path)
                {
                    sb.Append($" --({edge.StartConnectorName}->{edge.EndConnectorName})-> ");
                    sb.Append(edge.EndControl?.LabelText ?? "null");
                }
                result.Add(sb.ToString());
            }
            return result;
        }

        public List<List<string>> GetConnectionsNames()
        {
            var paths = GetAllConnectionPaths();
            var result = new List<List<string>>();
            foreach (var path in paths)
            {
                var tempList = new List<string>();
                if (path.Count == 0) continue;
                var sb = new StringBuilder();
                // 起点
                tempList.Add(path[0].StartControl?.LabelText ?? "null");
                foreach (var edge in path)
                {
                    tempList.Add(edge.EndControl?.LabelText ?? "null");
                }
                result.Add(tempList);
            }
            return result;
        }

        // 获取每一条路径上 每个方法及其对应的参数列表
        public List<List<(string, List<ScriptParamModel>)>> GetConnectionModel()
        {
            var paths = GetAllConnectionPaths();
            var result = new List<List<(string, List<ScriptParamModel>)>>();
            foreach (var connectionList in paths)
            {
                var tempList = new List<(string, List<ScriptParamModel>)>();
                if (connectionList.Count == 0)
                {
                    continue;
                }
                // 起点
                tempList.Add((connectionList[0].StartControl?.LabelText ?? "null", _controlParamDic[connectionList[0].StartControl]));
                foreach (var connection in connectionList)
                {
                    tempList.Add((connection.EndControl?.LabelText ?? "null", _controlParamDic[connection.EndControl]));
                }
                result.Add(tempList);
            }
            return result;
        }

        //执行脚本
        private async void ExcuteButton_Click(object sender, RoutedEventArgs e)
        {  
            if (_viewModel.SrcImagePath == null || !File.Exists(_viewModel.SrcImagePath))
            {
                MessageBox.Show("请先选择源图像。");
                return;
            }
            var path = GetConnectionModel();
            if (path.Count == 0)
            {
                MessageBox.Show("没有连接路径可执行。");
                return;
            }
            if (path.Count > 1)
            {
                MessageBox.Show("多条路径。");
                return;
            }
            // 仅执行第一条路径  第一条路径上的方法名及其参数列表
            List<(string, List<ScriptParamModel>)> methodsList = path[0];

            if (methodsList == null)
            {
                return;
            }
            Type type = typeof(ImageOperateMethods);

            Mat tempMat = new Mat(_viewModel.SrcImagePath);
            for (int i = 0; i < methodsList.Count; i++)
            {
                string methodName = methodsList[i].Item1;
                if (_viewModel.IsCN == true)
                {
                    methodName = ScriptService.Instance.GetMethodName(methodName, false);
                }
                // 获取公共静态方法
                MethodInfo publicMethod = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                if (publicMethod == null)
                {
                    MessageBox.Show($"找不到方法: {methodsList[i].Item1}");
                    return;
                }
                // 所有方法的参数都是  一张Mat 加上若干其他参数
                // Mat 为输入图像 或者上一个方法的输出图像
                List<object> paramList = new List<object>();
                paramList.Add(tempMat);
                for (int j = 0; j < methodsList[i].Item2.Count; j++)
                {
                    var paramModel = methodsList[i].Item2[j];
                    paramList.Add(paramModel.GetValue());
                }
                try
                {
                    object invokeResult = new object();
                    await Task.Run(() =>
                    {
                        invokeResult = publicMethod.Invoke(null, paramList.ToArray());
                    });

                    if (invokeResult is Mat matResult)
                    {
                        //_viewModel.DstMat = matResult;
                        //tempMat = _viewModel.DstMat;

                        // 克隆一份确保底层内存独立，强制在 UI 线程设置属性
                        var clone = matResult.Clone();
                        Dispatcher.Invoke(() => _viewModel.DstMat = clone);
                        tempMat = clone;
                    }
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"调用方法 {publicMethod.Name} 失败: {ex.ToString()}", "执行异常", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            MessageBox.Show("完成");
        }

        private void ClearScriptButton_Click(object sender, RoutedEventArgs e)
        {
            ScriptCanvas.Children.Clear();
            _connections.Clear();
            _controlParamDic.Clear();
        }

        private void CleanControlButton_Click(object sender, RoutedEventArgs e)
        {
            var paths = GetAllConnectionPaths();
            if (paths == null || paths.Count == 0)
            {
                MessageBox.Show("没有路径，无法清理控件。");
                return;
            }
            if (paths.Count > 1)
            {
                MessageBox.Show("存在多条路径，无法清理控件。");
                return;
            }
            List<Connection> path = paths[0];
            // 设置第一个控件位置不变，为基准点，后续控件垂直排列，间隔100
            double left = Canvas.GetLeft(path[0].StartControl);
            double top = Canvas.GetTop(path[0].StartControl);
            for (int i = 0; i < path.Count; i++)
            {
                Canvas.SetLeft(path[i].EndControl, left);
                Canvas.SetTop(path[i].EndControl, top + (i + 1) * 100);
            }
            ScriptCanvas.UpdateLayout();
            foreach (var conn in _connections)
            {
                // 重置所有连接点为上下连接，强制更新线段
                conn.StartConnectorName = "BottomConnector";
                conn.EndConnectorName = "TopConnector";
                UpdateConnectionLine(conn);
            }
            ScriptCanvas.UpdateLayout();
        }
    }

    public class Connection
    {
        public ScriptUserControl? StartControl { get; set; }
        public string StartConnectorName { get; set; } = "";
        public ScriptUserControl? EndControl { get; set; }
        public string EndConnectorName { get; set; } = "";
        public Polyline? Line { get; set; }
        public System.Windows.Shapes.Path? Arrow { get; set; }
    }
}