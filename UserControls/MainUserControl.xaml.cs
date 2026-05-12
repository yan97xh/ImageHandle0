using ImageHandle.Attributes;
using System.Windows.Controls;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// MainUserControls.xaml 的交互逻辑
    /// </summary>
    [NavigationPage(Models.PageEnum.MainPage)]
    public partial class MainUserControl : UserControl
    {
        public MainUserControl()
        {
            InitializeComponent();
        }
    }
}