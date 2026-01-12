using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImageHandle.Scripts
{
    /// <summary>
    /// ParamWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ParamWindow : Window
    {
        public ParamWindow(List<ScriptParamModel> paramModels)
        {
            InitializeComponent();
            _paramModel = paramModels ?? new List<ScriptParamModel>();
            SetControls();
        }

        private List<ScriptParamModel> _paramModel = new List<ScriptParamModel>();

        private void SetControls()
        {
            mainGrid.Children.Clear();
            mainGrid.RowDefinitions.Clear();

            // 顶部滚动区
            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(10)
            };

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
            };

            // 为每个参数生成一行控件
            foreach (var pm in _paramModel)
            {
                var rowGrid = new Grid
                {
                    Margin = new Thickness(0, 4, 0, 4)
                };
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // label
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // control

                // 标签（显示名 + 提示）
                var lbl = new TextBlock
                {
                    Text = pm.ParamDisplayName ?? pm.Name,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(4, 0, 8, 0),
                    ToolTip = pm.Remark
                };
                Grid.SetColumn(lbl, 0);
                rowGrid.Children.Add(lbl);

                FrameworkElement inputControl = null;

                switch (pm.Type)
                {
                    case ParamType.String:
                    case ParamType.Interger:
                    case ParamType.Double:
                        var tb = new TextBox
                        {
                            Text = pm.Value ?? string.Empty,
                            MinWidth = 120,
                            Tag = pm,
                            TextAlignment = TextAlignment.Center
                        };
                        // 输入限制（数字类型简单限制）
                        if (pm.Type == ParamType.Interger)
                        {
                            tb.PreviewTextInput += (s, e) =>
                            {
                                e.Handled = !e.Text.All(c => char.IsDigit(c) || c == '-');
                            };
                        }
                        else if (pm.Type == ParamType.Double)
                        {
                            tb.PreviewTextInput += (s, e) =>
                            {
                                e.Handled = !e.Text.All(c => char.IsDigit(c) || c == '-' || c == '.');
                            };
                        }
                        tb.LostFocus += (s, e) =>
                        {
                            var t = s as TextBox;
                            var model = t?.Tag as ScriptParamModel;
                            if (model != null)
                            {
                                model.Value = t.Text ?? "";
                                SetValidationVisual(t, model.CheckValueValid());
                            }
                        };
                        inputControl = tb;
                        break;

                    case ParamType.Boolean:
                        var cb = new CheckBox
                        {
                            IsChecked = bool.TryParse(pm.Value, out var b) && b,
                            Tag = pm,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        cb.Checked += (s, e) =>
                        {
                            var model = (s as CheckBox)?.Tag as ScriptParamModel;
                            if (model != null) model.Value = "True";
                        };
                        cb.Unchecked += (s, e) =>
                        {
                            var model = (s as CheckBox)?.Tag as ScriptParamModel;
                            if (model != null) model.Value = "False";
                        };
                        inputControl = cb;
                        break;

                    case ParamType.Enum:
                        var combo = new ComboBox
                        {
                            ItemsSource = pm.ParamArrays ?? Array.Empty<string>(),
                            Tag = pm,
                            MinWidth = 100,
                            IsEditable = false
                        };
                        if (!string.IsNullOrEmpty(pm.Value) && pm.ParamArrays != null && pm.ParamArrays.Contains(pm.Value))
                        {
                            combo.SelectedItem = pm.Value;
                        }
                        else if (pm.ParamArrays != null && pm.ParamArrays.Length > 0)
                        {
                            combo.SelectedIndex = 0;
                            pm.Value = combo.SelectedItem?.ToString() ?? "";
                        }
                        combo.SelectionChanged += (s, e) =>
                        {
                            var model = (s as ComboBox)?.Tag as ScriptParamModel;
                            if (model != null)
                                model.Value = (s as ComboBox)?.SelectedItem?.ToString() ?? "";
                        };
                        inputControl = combo;
                        break;

                    default:
                        var defaultTb = new TextBox
                        {
                            Text = pm.Value ?? string.Empty,
                            Tag = pm
                        };
                        defaultTb.LostFocus += (s, e) =>
                        {
                            var model = (s as TextBox)?.Tag as ScriptParamModel;
                            if (model != null) model.Value = (s as TextBox)?.Text ?? "";
                        };
                        inputControl = defaultTb;
                        break;
                }

                // 初始验证显示
                if (inputControl is TextBox itb)
                {
                    SetValidationVisual(itb, pm.CheckValueValid());
                }

                Grid.SetColumn(inputControl, 1);
                rowGrid.Children.Add(inputControl);

                // 将整行加入 StackPanel
                stack.Children.Add(rowGrid);
            }

            scroll.Content = stack;

            // 底部按钮区域
            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };

            var okBtn = new Button
            {
                Content = "确定",
                Width = 80,
                Margin = new Thickness(4, 0, 0, 0)
            };
            okBtn.Click += (s, e) =>
            {
                // 验证所有参数
                foreach (var pm in _paramModel)
                {
                    if (!pm.CheckValueValid())
                    {
                        MessageBox.Show(this, $"参数 \"{pm.ParamDisplayName}\" 的值不合法。", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                // 所有验证通过，关闭窗口（DialogResult true）
                this.DialogResult = true;
                this.Close();
            };

            var cancelBtn = new Button
            {
                Content = "取消",
                Width = 80,
                Margin = new Thickness(8, 0, 0, 0)
            };
            cancelBtn.Click += (s, e) =>
            {
                this.DialogResult = false;
                this.Close();
            };

            btnPanel.Children.Add(okBtn);
            btnPanel.Children.Add(cancelBtn);

            // 将 Scroll 与 按钮放到 mainGrid
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid.SetRow(scroll, 0);
            Grid.SetRow(btnPanel, 1);

            mainGrid.Children.Add(scroll);
            mainGrid.Children.Add(btnPanel);
        }

        private static void SetValidationVisual(Control control, bool valid)
        {
            if (valid)
            {
                control.ClearValue(BorderBrushProperty);
                control.ClearValue(BorderThicknessProperty);
            }
            else
            {
                control.BorderBrush = Brushes.Red;
                control.BorderThickness = new Thickness(1.5);
            }
        }

        public new List<ScriptParamModel> Show()
        {
            base.ShowDialog();
            return _paramModel;
        }
    }
}