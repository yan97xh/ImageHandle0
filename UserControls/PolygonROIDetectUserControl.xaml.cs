using ImageHandle.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// ROIDetectUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class PolygonROIDetectUserControl : UserControl
    {
        private PolygonROIDetectViewModel _viewModel;

        public PolygonROIDetectUserControl()
        {
            InitializeComponent();
            PolygonROIDetectViewModel viewModel = new PolygonROIDetectViewModel();
            this.DataContext = viewModel;
            _viewModel = viewModel;
            this.Loaded += OnLoaded;
            TargetImage.SizeChanged += ImgDisplay_SizeChanged;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e) => UpdateSize();

        private void ImgDisplay_SizeChanged(object? sender, SizeChangedEventArgs e) => UpdateSize();

        private void UpdateSize()
        {
            if (this.DataContext is ImageHandle.ViewModels.PolygonROIDetectViewModel vm)
            {
                vm.ImageDisplayWidth = TargetImage.ActualWidth;
                vm.ImageDisplayHeight = TargetImage.ActualHeight;
            }
        }
    }
}