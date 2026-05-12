using ImageHandle.Attributes;
using System.Windows.Controls;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// TextUserControl.xaml 的交互逻辑
    /// </summary>
    [NavigationPage(Models.PageEnum.TextPage)]
    public partial class TextUserControl : UserControl
    {
        public TextUserControl()
        {
            InitializeComponent();
        }
    }
}