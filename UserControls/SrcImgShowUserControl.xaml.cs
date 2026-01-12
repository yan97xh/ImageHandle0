using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// SrcImgShowUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class SrcImgShowUserControl : UserControl
    {
        #region 依赖属性

        /// <summary>
        /// 图像路径
        /// </summary>
        public string SrcImageSource
        {
            get { return (string)GetValue(SrcImageSourceProperty); }
            set { SetValue(SrcImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SrcImageSourceProperty =
            DependencyProperty.Register("SrcImageSource", typeof(string), typeof(SrcImgShowUserControl));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(SrcImgShowUserControl), new PropertyMetadata("输入图像"));

        #endregion 依赖属性

        public SrcImgShowUserControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "请选择图片";
            openFileDialog.Filter = "所有图片文件(*.jpg;*.bmp;*.jpeg;*.png;*.tif)|*.jpg;*.bmp;*.jpeg;*.png;*.tif";

            if (openFileDialog.ShowDialog() == true)
            {
                SrcImageSource = openFileDialog.FileName;
            }
        }
    }
}