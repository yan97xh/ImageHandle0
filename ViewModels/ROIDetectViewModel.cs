using Microsoft.Win32;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageHandle.ViewModels
{
    internal class ROIDetectViewModel : ViewModelBase
    {
        private readonly double defaultROIWidth = 100;
        private readonly double defaultROIHeight = 100;

        public ROIDetectViewModel()
        {
            InitROICommand = new Commands.Command<System.Windows.Point>(InitROI);

            _matCollection = new ObservableCollection<Mat>();
            _matCollection.CollectionChanged += OnCollectionChanged;

            SelectSrcImgCommand = new Commands.RelayCommand(SelectSrcImg);
            SaveImageCommand = new Commands.RelayCommand(SaveImage);

            ROIRectangleColorCommand = new Commands.RelayCommand(ProcessROIRectangleColor);
            ROIDetectCommand = new Commands.RelayCommand(ROIDetect);
            ClearROICommand = new Commands.RelayCommand(ClearROI);
        }

        #region ROI

        private System.Windows.Point topLeftP;

        public System.Windows.Point TopLeftP
        {
            get { return topLeftP; }
            set
            {
                topLeftP = value;
                OnPropertyChanged();
            }
        }

        private System.Windows.Point bottomRightP;

        public System.Windows.Point BottomRightP
        {
            get { return bottomRightP; }
            set
            {
                bottomRightP = value;
                OnPropertyChanged();
            }
        }

        private bool isROIDetectEnabled = false;

        public bool IsROIDetectEnabled
        {
            get { return isROIDetectEnabled; }
            set
            {
                isROIDetectEnabled = value;
                OnPropertyChanged();
            }
        }

        public ICommand InitROICommand { get; }

        private void InitROI(System.Windows.Point center)
        {
            if (string.IsNullOrEmpty(SrcImgPath))
            {
                MessageBox.Show("请先选择输入图像");
                return;
            }
            if (File.Exists(SrcImgPath) == false)
            {
                MessageBox.Show("输入图像路径无效，请重新选择");
                return;
            }
            IsROIDetectEnabled = true;
            TopLeftP = new System.Windows.Point(center.X - defaultROIWidth / 2, center.Y - defaultROIHeight / 2);
            BottomRightP = new System.Windows.Point(center.X + defaultROIWidth / 2, center.Y + defaultROIHeight / 2);
        }

        public ICommand ClearROICommand { get; }

        private void ClearROI()
        {
            IsROIDetectEnabled = false;
        }

        #endregion ROI

        #region 输入输出图像路径

        private string _srcImgPath;

        public string SrcImgPath
        {
            get => _srcImgPath;
            set
            {
                _srcImgPath = value;
                OnPropertyChanged();
            }
        }

        private string _saveImagePath;

        public string SaveImagePath
        {
            get => _saveImagePath;
            set
            {
                _saveImagePath = value;
                OnPropertyChanged();
            }
        }

        #endregion 输入输出图像路径

        #region 图像集合 用于保存每步操作 可撤销

        private ObservableCollection<Mat> _matCollection = new ObservableCollection<Mat>();
        public ObservableCollection<Mat> MatCollection => _matCollection;
        private Mat _lastMat;

        public Mat LastMat
        {
            get => _lastMat;
            set
            {
                _lastMat = value;
                OnPropertyChanged();
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LastMat = _matCollection.LastOrDefault();
        }

        private void Push(Mat mat)
        {
            _matCollection.Add(mat);
        }

        private Mat Pop()
        {
            if (_matCollection.Count <= 1)
            {
                return null;
            }
            var last = _matCollection.Last();
            _matCollection.RemoveAt(_matCollection.Count - 1);
            return last;
        }

        #endregion 图像集合 用于保存每步操作 可撤销

        #region 选择输入图像命令

        public ICommand SelectSrcImgCommand { get; }

        private void SelectSrcImg()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "请选择图片";
            openFileDialog.Filter = "所有图片文件(*.jpg;*.bmp;*.jpeg;*.png;*.tif)|*.jpg;*.bmp;*.jpeg;*.png;*.tif";
            // dialog.InitialDirectory = @"E:\myPictures\Lena";

            if (openFileDialog.ShowDialog() == true)
            {
                SrcImgPath = openFileDialog.FileName;
                _matCollection.Clear();
                Mat mat = new Mat(SrcImgPath);
                Push(mat);
                IsROIDetectEnabled = false;
            }
        }

        #endregion 选择输入图像命令

        #region 保存输出图像命令

        public ICommand SaveImageCommand { get; }

        private void SaveImage()
        {
            if (LastMat == null)
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

                bool success = Cv2.ImWrite(savePath, LastMat);
                if (!success)
                {
                    MessageBox.Show("图像保存失败");
                    return;
                }
                MessageBox.Show("保存成功");
                SaveImagePath = savePath;
            }
        }

        #endregion 保存输出图像命令

        #region 选择颜色的命令

        private Brush _rOIRectangleColor = Brushes.Red;

        public Brush ROIRectangleColor
        {
            get { return _rOIRectangleColor; }
            set
            {
                _rOIRectangleColor = value;
                OnPropertyChanged();
            }
        }

        public ICommand ROIRectangleColorCommand { get; }

        private void ProcessROIRectangleColor()
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedColor = colorDialog.Color;
                System.Windows.Media.Color mediaColor = System.Windows.Media.Color.FromArgb(
                  selectedColor.A,
                  selectedColor.R,
                  selectedColor.G,
                  selectedColor.B);
                ROIRectangleColor = new SolidColorBrush(mediaColor);
            }
        }

        #endregion 选择颜色的命令

        #region 抠图

        private Mat _rOIDetectMat;

        public Mat ROIDetectMat
        {
            get { return _rOIDetectMat; }
            set
            {
                _rOIDetectMat = value;
                OnPropertyChanged();
            }
        }

        public ICommand ROIDetectCommand { get; }

        // 显示控件上的图像展示尺寸（由视图层设置：通常是 Image 或 ROICanvas 的实际显示尺寸）
        private double _imageDisplayWidth;

        public double ImageDisplayWidth
        {
            get => _imageDisplayWidth;
            set { _imageDisplayWidth = value; OnPropertyChanged(); }
        }

        private double _imageDisplayHeight;

        public double ImageDisplayHeight
        {
            get => _imageDisplayHeight;
            set { _imageDisplayHeight = value; OnPropertyChanged(); }
        }

        // 将 TopLeftP/BottomRightP（画布/显示坐标）转换为图像像素坐标（OpenCvSharp.Rect）
        private OpenCvSharp.Rect? GetImageRect()
        {
            if (LastMat == null)
                return null;

            double imgW = LastMat.Width;
            double imgH = LastMat.Height;
            if (imgW <= 0 || imgH <= 0)
                return null;

            double dispW = ImageDisplayWidth;
            double dispH = ImageDisplayHeight;
            if (dispW <= 0 || dispH <= 0)
                return null; // 视图未提供显示尺寸

            double scale = dispW / imgW;
            double scaledImgW = imgW * scale;
            double scaledImgH = imgH * scale;

            // 将 ROI 的画布坐标转换到图像坐标系
            double tlx_disp = TopLeftP.X;
            double tly_disp = TopLeftP.Y;
            double brx_disp = BottomRightP.X;
            double bry_disp = BottomRightP.Y;

            // 把显示坐标减去偏移并除以 scale 得到原图坐标
            double tlx_img = tlx_disp * imgW / dispW;
            double tly_img = tly_disp * imgH / dispH;
            double brx_img = brx_disp * imgW / dispW;
            double bry_img = bry_disp * imgH / dispH;

            // 四舍五入并裁剪到图像边界
            int x = (int)Math.Max(0, Math.Round(tlx_img));
            int y = (int)Math.Max(0, Math.Round(tly_img));
            int width = (int)Math.Max(1, Math.Round(brx_img - tlx_img));
            int height = (int)Math.Max(1, Math.Round(bry_img - tly_img));

            if (x + width > imgW) width = (int)imgW - x;
            if (y + height > imgH) height = (int)imgH - y;

            if (width <= 0 || height <= 0)
                return null;

            return new OpenCvSharp.Rect(x, y, width, height);
        }

        private void ROIDetect()
        {
            if (LastMat == null)
            {
                MessageBox.Show("没有可处理的图像");
                return;
            }
            if (IsROIDetectEnabled == false)
            {
                MessageBox.Show("无ROI区域");
                return;
            }

            var roiRect = GetImageRect();
            if (roiRect == null)
            {
                MessageBox.Show("无法将 ROI 转换到图像坐标（可能未提供显示尺寸或 ROI 在显示区域外）");
                return;
            }

            // 使用 OpenCvSharp.Rect 提取子图
            var rect = roiRect.Value;
            try
            {
                Mat roiMat = new Mat(LastMat, rect);
                ROIDetectMat = roiMat.Clone(); // clone 避免引用原 Mat 的内存问题
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("提取 ROI 失败: " + ex.Message);
            }
        }

        #endregion 抠图
    }
}