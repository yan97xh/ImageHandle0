using ImageHandle.Attributes;
using System.Windows.Controls;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// MorphologicalOperationsControl.xaml 的交互逻辑
    /// </summary>
    [NavigationPage(Models.PageEnum.MorphologicalPage)]
    public partial class MorphologicalOperationsControl : UserControl
    {
        public MorphologicalOperationsControl()
        {
            InitializeComponent();
        }
    }
}