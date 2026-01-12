using ImageHandle.Views;
using Microsoft.Win32;
using OpenCvSharp;
using System.Windows;
using System.Windows.Controls;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// DstImgShowUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class DstImgShowUserControl : UserControl
    {
        public DstImgShowUserControl()
        {
            InitializeComponent();
        }

        #region 依赖属性

        /// <summary>
        /// 目的图像
        /// </summary>
        public Mat DstImageSource
        {
            get { return (Mat)GetValue(DstImageSourceProperty); }
            set { SetValue(DstImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DatImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DstImageSourceProperty =
            DependencyProperty.Register("DstImageSource", typeof(Mat), typeof(DstImgShowUserControl));

        /// <summary>
        /// 图像路径保存
        /// </summary>
        public string ImageSource_Save
        {
            get { return (string)GetValue(ImageSource_SaveProperty); }
            set { SetValue(ImageSource_SaveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSource_SaveProperty =
            DependencyProperty.Register("ImageSource_Save", typeof(string), typeof(DstImgShowUserControl));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(DstImgShowUserControl), new PropertyMetadata("输出图像"));

        #endregion 依赖属性

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DstImageSource = null;
        }

        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            if (DstImageSource == null)
            {
                MessageBox.Show("没有需要保存的图像");
                return;
            }
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "请选择图片";
            dialog.Filter = "图片文件 (*.jpg)|*.jpg |图片文件 (*.bmp)|*.bmp |图片文件 (*.jpeg)|*.jpeg |图片文件 (*.png)|*.png";

            if (dialog.ShowDialog() == true)
            {
                string savePath = dialog.FileName;
                if (savePath == "")
                {
                    MessageBox.Show("未选择保存路径");
                    return;
                }

                bool success = Cv2.ImWrite(savePath, DstImageSource);
                if (!success)
                {
                    MessageBox.Show("图像保存失败");
                    return;
                }
                MessageBox.Show("保存成功");
                ImageSource_Save = savePath;
            }
        }

        private DateTime _lastClickTime;
        private const int DoubleClickThreshold = 300; // 双击时间间隔阈值，单位为毫秒
        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < DoubleClickThreshold)
            {
                PicShowWindow picShowWindow = new PicShowWindow(DstImageSource);
                picShowWindow.Show();
                e.Handled = true;
            }
            _lastClickTime = DateTime.Now;
        }
    }
}