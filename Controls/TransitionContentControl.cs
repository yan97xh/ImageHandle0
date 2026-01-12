using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ImageHandle.Controls
{
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    public class TransitionContentControl : ContentControl
    {
        private ContentPresenter? _presenter;

        // 动画时长，单位毫秒
        public double FadeDuration
        {
            get => (double)GetValue(FadeDurationProperty);
            set => SetValue(FadeDurationProperty, value);
        }

        public static readonly DependencyProperty FadeDurationProperty =
            DependencyProperty.Register(nameof(FadeDuration), typeof(double), typeof(TransitionContentControl), new PropertyMetadata(250.0));

        static TransitionContentControl()
        {
            // 不覆盖 DefaultStyleKey，这样在没有 Themes/Generic.xaml 时也会使用 ContentControl 的默认模板，
            // 避免因缺失默认样式导致内容不显示的问题。
            // DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitionContentControl), new FrameworkPropertyMetadata(typeof(TransitionContentControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _presenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            // 尝试取得模板中的 presenter（如果模板存在）
            if (_presenter == null)
                _presenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;

            double durationMs = Math.Max(0, FadeDuration);
            var anim = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // 优先对 presenter 做动画（更细粒度），否则对整个控件做淡入动画
            if (_presenter != null)
            {
                _presenter.Opacity = 0.0;
                _presenter.BeginAnimation(UIElement.OpacityProperty, anim);
            }
            else
            {
                this.Opacity = 0.0;
                this.BeginAnimation(UIElement.OpacityProperty, anim);
            }
        }
    }
}