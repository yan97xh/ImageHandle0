using ImageHandle.Attributes;
using ImageHandle.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// FloodFillUserControl.xaml 的交互逻辑
    /// </summary>
    [NavigationPage(Models.PageEnum.FloodFillPage)]
    public partial class FloodFillUserControl : UserControl
    {
        private FloodFillViewModel _viewModel;

        public FloodFillUserControl()
        {
            InitializeComponent();
            _viewModel = new FloodFillViewModel();
            this.DataContext = _viewModel;
            TargetImage.Loaded += OnImageLoaded;
        }

        private void OnImageLoaded(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            // 移除旧的事件处理
            image.MouseMove -= OnImageMouseMove;
            image.MouseLeftButtonDown -= OnImageMouseClick;
            // 添加新的事件处理
            image.MouseMove += OnImageMouseMove;
            image.MouseLeftButtonDown += OnImageMouseClick;
        }

        private void OnImageMouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel.IsCoordinateModeEnabled && _viewModel.LastMat != null)
            {
                Point position = e.GetPosition(TargetImage);
                var imageSize = new Size(TargetImage.ActualWidth, TargetImage.ActualHeight);
                _viewModel.HandleMouseMove(position, imageSize);
            }
        }

        private void OnImageMouseClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.IsCoordinateModeEnabled && _viewModel.LastMat != null)
            {
                var position = e.GetPosition(TargetImage);
                var controlSize = new Size(TargetImage.ActualWidth, TargetImage.ActualHeight);
                _viewModel.HandleMouseClick(position, controlSize);
            }
        }
    }
}