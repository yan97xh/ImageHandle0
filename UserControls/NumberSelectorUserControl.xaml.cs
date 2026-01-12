using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// NumberSelectorUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class NumberSelectorUserControl : UserControl
    {
        public NumberSelectorUserControl()
        {
            InitializeComponent();
            UpdateButtonStates();
        }

        #region 依赖属性

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumberSelectorUserControl),
                new FrameworkPropertyMetadata(0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged,
                    CoerceValue));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(NumberSelectorUserControl),
                new PropertyMetadata(0, OnLimitChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(NumberSelectorUserControl),
                new PropertyMetadata(100, OnLimitChanged));

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register("Step", typeof(int), typeof(NumberSelectorUserControl),
                new PropertyMetadata(1));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(NumberSelectorUserControl),
                new PropertyMetadata(new CornerRadius(0)));

        #endregion 依赖属性

        #region 属性包装器

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public int Step
        {
            get => (int)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion 属性包装器

        #region 属性变更回调

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumberSelectorUserControl)d;
            control.UpdateButtonStates();
        }

        private static void OnLimitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumberSelectorUserControl)d;
            control.CoerceValue(ValueProperty);
            control.UpdateButtonStates();
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var control = (NumberSelectorUserControl)d;
            var value = (int)baseValue;

            if (value < control.Minimum)
                return control.Minimum;
            if (value > control.Maximum)
                return control.Maximum;

            return value;
        }

        #endregion 属性变更回调

        #region 按钮事件处理

        private void DecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            var newValue = Value - Step;
            Value = Math.Max(newValue, Minimum);
        }

        private void IncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            var newValue = Value + Step;
            Value = Math.Min(newValue, Maximum);
        }

        private void UpdateButtonStates()
        {
            DecreaseButton.IsEnabled = Value > Minimum;
            IncreaseButton.IsEnabled = Value < Maximum;
        }

        #endregion 按钮事件处理

        #region 文本框输入验证

        private void ValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 只允许输入数字
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void ValueTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool IsTextAllowed(string text)
        {
            // 允许空字符串（用于退格）
            if (string.IsNullOrEmpty(text))
                return true;

            // 检查是否为数字
            return int.TryParse(text, out _);
        }

        #endregion 文本框输入验证
    }
}