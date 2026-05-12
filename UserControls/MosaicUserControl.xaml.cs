using ImageHandle.Attributes;
using System.Windows.Controls;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// MosaicUserControl.xaml 的交互逻辑
    /// </summary>
    [NavigationPage(Models.PageEnum.MosaicPage)]
    public partial class MosaicUserControl : UserControl
    {
        public MosaicUserControl()
        {
            InitializeComponent();
        }
    }
}