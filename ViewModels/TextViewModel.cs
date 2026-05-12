using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageHandle.Helpers;
using OpenCvSharp;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageHandle.ViewModels
{
    public partial class TextViewModel : ObservableObject
    {
        public TextViewModel()
        {
            // 文本粗细
            ThincknessItemsArray = ["1", "2", "3", "4", "5"];
        }

        [ObservableProperty]
        private string _srcImagePath;

        [ObservableProperty]
        private Mat _dstMat;

        #region 文本输入

        [ObservableProperty]
        private System.Windows.Media.Color _writeFontColor = Colors.Red;     // 写入文本的字体颜色

        [RelayCommand]
        private void ProcessFontColor()
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedColor = colorDialog.Color;
                WriteFontColor = System.Windows.Media.Color.FromArgb(
                    selectedColor.A,
                    selectedColor.R,
                    selectedColor.G,
                    selectedColor.B);
            }
        }

        [ObservableProperty]
        private string _writeText;

        [ObservableProperty]
        private double _writeFontScale = 2.0;

        [ObservableProperty]
        private int _locationX = 0;

        [ObservableProperty]
        private int _locationY = 0;

        [ObservableProperty]
        private int _writeTextThinckness = 2;

        partial void OnWriteTextThincknessChanged(int value)
        {
            ProcessFontStyleShow();
        }

        public string[] ThincknessItemsArray { get; }

        [ObservableProperty]
        private string _fontStyle;

        partial void OnFontStyleChanged(string value)
        {
            ProcessFontStyleShow();
        }

        public string[] FontStyleItemsArray
        {
            get;
        }

        [ObservableProperty]
        private Mat _fontShowMat;

        /// <summary>
        /// 当前选择的字体样式展示
        /// </summary>
        private void ProcessFontStyleShow()
        {
            if (Enum.TryParse(typeof(HersheyFonts), _fontStyle, true, out object result) == false)
            {
                MessageBox.Show($"字体选择有误");
                return;
            }
            HersheyFonts hersheyFonts = (HersheyFonts)result;
            string path = AppDomain.CurrentDomain.BaseDirectory + "Resource\\Images\\0.png";

            Mat srcImg = new Mat(path);

            //字体大小不一样 写入位置也有点区别
            OpenCvSharp.Point PP;
            switch (hersheyFonts)
            {
                case HersheyFonts.HersheySimplex:
                    break;

                case HersheyFonts.HersheyPlain:
                    break;

                case HersheyFonts.HersheyDuplex:
                    break;

                case HersheyFonts.HersheyComplex:
                    break;

                case HersheyFonts.HersheyTriplex:
                    break;

                case HersheyFonts.HersheyComplexSmall:
                    break;

                case HersheyFonts.HersheyScriptSimplex:
                    break;

                case HersheyFonts.HersheyScriptComplex:
                    break;

                case HersheyFonts.Italic:
                    break;
            }

            PP = new OpenCvSharp.Point(10, 130);
            OpenCvSharp.Cv2.PutText(srcImg, "AaBb", PP, hersheyFonts, 3, Scalar.Black, _writeTextThinckness);
            FontShowMat = srcImg;
        }

        [RelayCommand]
        private void ProcessWriteText()
        {
            if (string.IsNullOrEmpty(_srcImagePath))
            {
                MessageBox.Show("未选择输入图像");
                return;
            }
            if (File.Exists(_srcImagePath) == false)
            {
                MessageBox.Show($"图像:{_srcImagePath}不存在");
                return;
            }
            Mat srcImg = new Mat(_srcImagePath);
            if (string.IsNullOrWhiteSpace(_writeText))
            {
                MessageBox.Show("没有需要写入的文本内容");
                return;
            }

            OpenCvSharp.Point location = new OpenCvSharp.Point(_locationX, _locationY);

            if (Enum.TryParse(typeof(HersheyFonts), _fontStyle, true, out object result) == false)
            {
                MessageBox.Show($"字体选择有误");
                return;
            }
            HersheyFonts hersheyFonts = (HersheyFonts)result;

            Scalar scalar = Scalar.FromRgb(_writeFontColor.R, _writeFontColor.G, _writeFontColor.B);
            OpenCvSharp.Cv2.PutText(srcImg, _writeText, location, hersheyFonts, _writeFontScale, scalar, _writeTextThinckness);
            DstMat = srcImg;
        }

        #endregion 文本输入

        #region 文字识别

        [RelayCommand]
        private void ProcessOCR()
        {
            if (string.IsNullOrEmpty(_srcImagePath))
            {
                MessageBox.Show("未选择输入图像");
                return;
            }
            if (File.Exists(_srcImagePath) == false)
            {
                MessageBox.Show($"图像:{_srcImagePath}不存在");
                return;
            }
            Mat srcImg = new Mat(_srcImagePath);

            DstMat = ImageOperateMethods.OCRText(srcImg);
        }

        #endregion 文字识别

        #region 二维码生成

        [ObservableProperty]
        private System.Windows.Media.Color _QRCodeColor = Colors.Black;

        [RelayCommand]
        private void ProcessQRCodeColor()
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedColor = colorDialog.Color;
                QRCodeColor = System.Windows.Media.Color.FromArgb(
                    selectedColor.A,
                    selectedColor.R,
                    selectedColor.G,
                    selectedColor.B);
            }
        }

        [ObservableProperty]
        private string _QRCodeMessage;

        [ObservableProperty]
        private bool _blankSide = true;

        [ObservableProperty]
        private bool _logo = false;

        [RelayCommand]
        private void QRCodeCreate()
        {
            if (string.IsNullOrWhiteSpace(_QRCodeMessage))
            {
                MessageBox.Show("无二维码信息");
                return;
            }
            BitmapImage qrImage = null;
            if (_logo == false)
            {
                qrImage = ImageOperateMethods.GetNormalQRCode(_QRCodeMessage, 300, 256, 256, _blankSide, _QRCodeColor);
            }
            else
            {
                if (string.IsNullOrEmpty(_srcImagePath))
                {
                    MessageBox.Show("未选择输入图像");
                    return;
                }
                if (File.Exists(_srcImagePath) == false)
                {
                    MessageBox.Show($"图像:{_srcImagePath}不存在");
                    return;
                }
                BitmapImage icon = new BitmapImage(new Uri(_srcImagePath));
                qrImage = ImageOperateMethods.GetLogoQRCode(_QRCodeMessage, icon, 300, 256, 256, _blankSide, _QRCodeColor);
            }
            DstMat = MatConverters.BitmapToMat(qrImage);
        }

        #endregion 二维码生成

        #region 二维码识别

        [ObservableProperty]
        private string _QRCodeDetectMsg;

        [RelayCommand]
        private void QRCodeDetect()
        {
            if (string.IsNullOrEmpty(_srcImagePath))
            {
                MessageBox.Show("未选择输入图像");
                return;
            }
            if (File.Exists(_srcImagePath) == false)
            {
                MessageBox.Show($"图像:{_srcImagePath}不存在");
                return;
            }

            QRCodeDetector qrCode = new QRCodeDetector();

            Mat srcImg = new Mat(_srcImagePath);

            OpenCvSharp.Point2f[] pts;
            string result = qrCode.DetectAndDecode(srcImg, out pts);
            if (result.Trim() == "")
            {
                MessageBox.Show("图中无二维码或者二维码无信息");
                return;
            }
            QRCodeDetectMsg = result;
        }

        #endregion 二维码识别
    }
}