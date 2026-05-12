using ImageHandle.Attributes;
using System.Windows.Controls;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// FeatureDetectUserControl.xaml 的交互逻辑
    /// </summary>
    [NavigationPage(Models.PageEnum.FeatureDetectPage)]
    public partial class FeatureDetectUserControl : UserControl
    {
        public FeatureDetectUserControl()
        {
            InitializeComponent();
        }
    }
}