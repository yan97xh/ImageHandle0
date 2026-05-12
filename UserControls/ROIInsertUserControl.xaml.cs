using ImageHandle.Attributes;
using ImageHandle.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// ROIInsertUserControl.xaml 的交互逻辑
    /// </summary>
    [NavigationPage(Models.PageEnum.ROIInsertPage)]
    public partial class ROIInsertUserControl : UserControl
    {
        private ROIInsertViewModel _viewModel;

        public ROIInsertUserControl()
        {
            InitializeComponent();
            ROIInsertViewModel viewModel = new ROIInsertViewModel();
            this.DataContext = viewModel;
            _viewModel = viewModel;
            this.Loaded += OnLoaded;
            TargetImage.SizeChanged += ImgDisplay_SizeChanged;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e) => UpdateSize();

        private void ImgDisplay_SizeChanged(object? sender, SizeChangedEventArgs e) => UpdateSize();

        private void UpdateSize()
        {
            if (this.DataContext is ImageHandle.ViewModels.ROIInsertViewModel vm)
            {
                vm.ImageDisplayWidth = TargetImage.ActualWidth;
                vm.ImageDisplayHeight = TargetImage.ActualHeight;
            }
        }
    }
}