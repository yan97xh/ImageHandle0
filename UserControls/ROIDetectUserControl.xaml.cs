using ImageHandle.Attributes;
using ImageHandle.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// ROIDetectUserControl.xaml 的交互逻辑
    /// </summary>
    [NavigationPage(Models.PageEnum.ROIDetectPage)]
    public partial class ROIDetectUserControl : UserControl
    {
        private ROIDetectViewModel _viewModel;

        public ROIDetectUserControl()
        {
            InitializeComponent();
            ROIDetectViewModel viewModel = new ROIDetectViewModel();
            this.DataContext = viewModel;
            _viewModel = viewModel;
            this.Loaded += OnLoaded;
            TargetImage.SizeChanged += ImgDisplay_SizeChanged;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e) => UpdateSize();

        private void ImgDisplay_SizeChanged(object? sender, SizeChangedEventArgs e) => UpdateSize();

        private void UpdateSize()
        {
            if (this.DataContext is ImageHandle.ViewModels.ROIDetectViewModel vm)
            {
                vm.ImageDisplayWidth = TargetImage.ActualWidth;
                vm.ImageDisplayHeight = TargetImage.ActualHeight;
            }
        }
    }
}