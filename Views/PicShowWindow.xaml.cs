using OpenCvSharp;
using System.Windows.Media.Imaging;

namespace ImageHandle.Views
{
    /// <summary>
    /// PicShowWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PicShowWindow : System.Windows.Window
    {
        private bool _isClosing = false;
        private readonly int _wheelRatio = 5;
        public PicShowWindow(Mat mat)
        {
            InitializeComponent();
            var bitmapSource = MatConvertToImageSource(mat);
            mainImage.Source = bitmapSource;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (this.Visibility == System.Windows.Visibility.Visible && this.IsLoaded && _isClosing == false)
            {
                _isClosing = true;
                this.Close();
            }
        }

        private BitmapSource MatConvertToImageSource(Mat mat)
        {
            if (mat == null || mat.Empty())
                return null;
            try
            {
                // 根据Mat类型进行相应的转换
                Mat imageToShow = mat;

                // 如果是单通道灰度图，转换为BGR
                if (mat.Channels() == 1)
                {
                    imageToShow = new Mat();
                    Cv2.CvtColor(mat, imageToShow, ColorConversionCodes.GRAY2BGR);
                }
                // 如果是4通道（带Alpha），转换为BGR
                else if (mat.Channels() == 4)
                {
                    imageToShow = new Mat();
                    Cv2.CvtColor(mat, imageToShow, ColorConversionCodes.BGRA2BGR);
                }

                var bitmapSource = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(imageToShow);

                return bitmapSource;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private DateTime _lastClickTime;
        private const int DoubleClickThreshold = 300; // 双击时间间隔阈值，单位为毫秒

        private void mainImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < DoubleClickThreshold)
            {
                if (_isClosing == false)
                {
                    _isClosing = true;
                    this.Close();
                    e.Handled = true;
                }
            }
            else
            {
                this.DragMove();
            }
            _lastClickTime = DateTime.Now;
        }

        private void mainImage_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            int width = (int)this.Width;
            int height = (int)this.Height;
            width += e.Delta / _wheelRatio;
            height += e.Delta / _wheelRatio;
            if (width <= this.MinWidth)
            {
                width = (int)this.MinWidth;
            }
            if (height <= this.MinHeight)
            {
                height = (int)this.MinHeight;
            }
            if (width >= this.MaxWidth)
            {
                width = (int)this.MaxWidth;
            }
            if (height >= this.MaxHeight)
            {
                height = (int)this.MaxHeight;
            }
            this.Width = width;
            this.Height = height;
        }
    }
}
