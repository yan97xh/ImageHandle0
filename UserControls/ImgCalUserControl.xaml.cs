using ImageHandle.Attributes;
using System.Windows.Controls;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// ImgCalUserControl.xaml 的交互逻辑
    /// </summary>
    [NavigationPage(Models.PageEnum.ImgCalPage)]
    public partial class ImgCalUserControl : UserControl
    {
        public ImgCalUserControl()
        {
            InitializeComponent();
        }
    }
}