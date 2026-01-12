using ImageHandle.Helpers;
using OpenCvSharp;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageHandle.ViewModels
{
    public class TextViewModel : ViewModelBase
    {
        public TextViewModel()
        {
            FontStyleItemsArray = Enum.GetNames(typeof(HersheyFonts));
            ThincknessItemsArray = ["1", "2", "3", "4", "5"];
            FontColorCommand = new Commands.RelayCommand(ProcessFontColor);
            WriteTextCommand = new Commands.RelayCommand(ProcessWriteText);
            OCRCommand = new Commands.RelayCommand(ProcessOCR);
            QRCodeColorCommand = new Commands.RelayCommand(ProcessQRCodeColor);
            QRCodeCreateCommand = new Commands.RelayCommand(QRCodeCreate);
            QRCodeDetectCommand = new Commands.RelayCommand(QRCodeDetect);
        }

        #region 输入图像

        private string _srcImagePath;

        public string SrcImagePath
        {
            get => _srcImagePath;
            set
            {
                _srcImagePath = value;
                OnPropertyChanged();
            }
        }

        #endregion 输入图像

        #region 输出图像

        private Mat _dstMat;

        public Mat DstMat
        {
            get => _dstMat;
            set
            {
                _dstMat = value;
                OnPropertyChanged();
            }
        }

        #endregion 输出图像

        #region 文本输入

        private System.Windows.Media.Color _writeFontColor = Colors.Red;

        // 写入文本的字体颜色
        public System.Windows.Media.Color WriteFontColor
        {
            get { return _writeFontColor; }
            set
            {
                _writeFontColor = value;
                OnPropertyChanged();
            }
        }

        #region 选择颜色的命令

        public ICommand FontColorCommand { get; }

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

        #endregion 选择颜色的命令

        private string _text;

        //写入文本
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        private double _fontScale = 2.0;

        public double FontScale
        {
            get => _fontScale;
            set
            {
                _fontScale = value;
                OnPropertyChanged();
            }
        }

        private int _locationX = 0;

        public int LocationX
        {
            get => _locationX;
            set
            {
                _locationX = value;
                OnPropertyChanged();
            }
        }

        private int _locationY = 0;

        public int LocationY
        {
            get => _locationY;
            set
            {
                _locationY = value;
                OnPropertyChanged();
            }
        }

        private int _thinckness = 2;

        public int Thinckness
        {
            get => _thinckness;
            set
            {
                _thinckness = value;
                OnPropertyChanged();
                ProcessFontStyleShow();
            }
        }

        public string[] ThincknessItemsArray { get; }

        private string _fontStyle;

        public string FontStyle
        {
            get => _fontStyle;
            set
            {
                _fontStyle = value;
                OnPropertyChanged();
                ProcessFontStyleShow();
            }
        }

        public string[] FontStyleItemsArray
        {
            get;
        }

        private Mat _fontShowMat;

        public Mat FontShowMat
        {
            get => _fontShowMat;
            set
            {
                _fontShowMat = value;
                OnPropertyChanged();
            }
        }

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
            OpenCvSharp.Cv2.PutText(srcImg, "AaBb", PP, hersheyFonts, 3, Scalar.Black, _thinckness);
            FontShowMat = srcImg;
        }

        public ICommand WriteTextCommand { get; }

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
            if (string.IsNullOrWhiteSpace(_text))
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
            OpenCvSharp.Cv2.PutText(srcImg, _text, location, hersheyFonts, _fontScale, scalar, _thinckness);
            DstMat = srcImg;
        }

        #endregion 文本输入

        #region 文字识别

        public ICommand OCRCommand { get; }

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

        private System.Windows.Media.Color _qrCodeColor = Colors.Black;

        public System.Windows.Media.Color QRCodeColor
        {
            get => _qrCodeColor;
            set
            {
                _qrCodeColor = value;
                OnPropertyChanged();
            }
        }

        #region 选择颜色的命令

        public ICommand QRCodeColorCommand { get; }

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

        #endregion 选择颜色的命令

        private string _qrCodeMessage;

        public string QRCodeMessage
        {
            get => _qrCodeMessage;
            set
            {
                _qrCodeMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _blankSide = true;

        public bool BlankSide
        {
            get => _blankSide;
            set
            {
                _blankSide = value;
                OnPropertyChanged();
            }
        }

        private bool _logo = false;

        public bool Logo
        {
            get => _logo;
            set
            {
                _logo = value;
                OnPropertyChanged();
            }
        }

        public ICommand QRCodeCreateCommand { get; }

        private void QRCodeCreate()
        {
            if (string.IsNullOrWhiteSpace(_qrCodeMessage))
            {
                MessageBox.Show("无二维码信息");
                return;
            }
            BitmapImage qrImage = null;
            if (_logo == false)
            {
                qrImage = ImageOperateMethods.GetNormalQRCode(_qrCodeMessage, 300, 256, 256, _blankSide, _qrCodeColor);
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
                qrImage = ImageOperateMethods.GetLogoQRCode(_qrCodeMessage, icon, 300, 256, 256, _blankSide, _qrCodeColor);
            }
            DstMat = MatConverters.BitmapToMat(qrImage);
        }

        #endregion 二维码生成

        #region 二维码识别

        private string _qrCodeDetectMsg;

        public string QRCodeDetectMsg
        {
            get { return _qrCodeDetectMsg; }
            set
            {
                _qrCodeDetectMsg = value;
                OnPropertyChanged();
            }
        }

        public ICommand QRCodeDetectCommand { get; }

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