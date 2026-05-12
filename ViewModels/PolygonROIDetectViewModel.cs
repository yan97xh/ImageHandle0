using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace ImageHandle.ViewModels
{
    internal partial class PolygonROIDetectViewModel : ObservableObject
    {
        private readonly double defaultROIWidth = 100;
        private readonly double defaultROIHeight = 100;

        public PolygonROIDetectViewModel()
        {
            _matCollection = new ObservableCollection<Mat>();
            _matCollection.CollectionChanged += OnCollectionChanged;
        }

        [ObservableProperty]
        private bool isROIDetectEnabled = false;

        [RelayCommand]
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
        }

        [ObservableProperty]
        private ObservableCollection<System.Windows.Point> _pointList = new ObservableCollection<System.Windows.Point>();

        [RelayCommand]
        private void ClearROI()
        {
            IsROIDetectEnabled = false;
            PointList.Clear();
        }

        [ObservableProperty]
        private string _srcImgPath;

        [ObservableProperty]
        private string _saveImagePath;

        #region 图像集合 用于保存每步操作 可撤销

        private ObservableCollection<Mat> _matCollection = new ObservableCollection<Mat>();
        public ObservableCollection<Mat> MatCollection => _matCollection;

        [ObservableProperty]
        private Mat _lastMat;

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

        [RelayCommand]
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
                ClearROI();
            }
        }

        [RelayCommand]
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

        [ObservableProperty]
        private Brush _rOIColor = Brushes.Red;

        [RelayCommand]
        private void ProcessROIColor()
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
                ROIColor = new SolidColorBrush(mediaColor);
            }
        }

        [ObservableProperty]
        private Brush _detectBackColor = Brushes.Black;

        [RelayCommand]
        private void ProcessDetectBackColor()
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
                DetectBackColor = new SolidColorBrush(mediaColor);
            }
        }

        [ObservableProperty]
        private Mat _rOIDetectMat;

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

        [RelayCommand]
        private void PolygonROIDetect()
        {
            if (LastMat == null)
            {
                MessageBox.Show("没有可处理的图像");
                return;
            }
            if (isROIDetectEnabled == true)
            {
                MessageBox.Show("ROI未选择结束");
                return;
            }
            if (PointList.Count <= 2)
            {
                MessageBox.Show("无ROI区域");
                return;
            }
            try
            {
                // 将 PointList（显示坐标）转换为图像像素坐标
                List<OpenCvSharp.Point> pointList = PointList.Select(
                point => new OpenCvSharp.Point((int)(point.X * LastMat.Width / ImageDisplayWidth),
                                                (int)(point.Y * LastMat.Height / ImageDisplayHeight)))
                                                .ToList();

                Mat mask = GetMaskFloodFill(LastMat.Size(), pointList);
                OpenCvSharp.Rect rect = Cv2.BoundingRect(pointList);
                Mat src1 = new Mat(LastMat, rect);
                Mat maskRoI = new Mat(mask, rect);
                Cv2.CvtColor(maskRoI, maskRoI, ColorConversionCodes.BGR2GRAY);
                Mat dstImg = new Mat();

                Scalar backScalar = new Scalar(((SolidColorBrush)DetectBackColor).Color.B,
                                               ((SolidColorBrush)DetectBackColor).Color.G,
                                               ((SolidColorBrush)DetectBackColor).Color.R);
                dstImg = new Mat(src1.Size(), src1.Type(), backScalar);
                src1.CopyTo(dstImg, maskRoI);
                ROIDetectMat = dstImg.Clone();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("提取 ROI 失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取指定大小的ROI部分的掩膜单通道图像
        /// </summary>
        /// <param name="size">最小大小为 BoundingRect.Size </param>
        /// <returns></returns>
        private Mat GetMaskFloodFill(OpenCvSharp.Size size, List<OpenCvSharp.Point> points)
        {
            OpenCvSharp.Rect BoundingRect = Cv2.BoundingRect(points);
            if (size.Width < BoundingRect.Size.Width || size.Height < BoundingRect.Size.Height)
                size = BoundingRect.Size;
            Mat mask = Mat.Zeros(size, MatType.CV_8UC3);
            mask = DrawPolygon(mask, points);
            //水漫的种子点 要求在ROI区域内部
            OpenCvSharp.Point Center = GetPointInsideConvexPolygon(points);
            OpenCvSharp.Point pt = new OpenCvSharp.Point(Center.X, Center.Y);
            Cv2.FloodFill(mask, pt, Scalar.Red);
            mask.ConvertTo(mask, MatType.CV_8UC1);
            return mask;
        }

        // 在图像上绘制多边形
        private OpenCvSharp.Mat DrawPolygon(Mat src, List<OpenCvSharp.Point> points)
        {
            Mat dstImg = src.Clone();

            try
            {
                for (int i = 0; i <= points.Count - 1; i++)
                {
                    if (i != points.Count - 1)
                    {
                        Cv2.Line(dstImg, points[i], points[i + 1], Scalar.Red, 2, LineTypes.AntiAlias);
                    }
                    else
                    {
                        Cv2.Line(dstImg, points[points.Count - 1], points[0], Scalar.Red, 2, LineTypes.AntiAlias);
                    }
                }
            }
            catch
            {
                dstImg = src.Clone();
            }
            return dstImg;
        }

        // 获取凸多边形内部的一个点（质心法）
        private OpenCvSharp.Point GetPointInsideConvexPolygon(List<OpenCvSharp.Point> points)
        {
            if (points == null || points.Count < 3)
                throw new ArgumentException("需要至少3个点构成凸多边形");

            double sumX = 0;
            double sumY = 0;

            foreach (OpenCvSharp.Point p in points)
            {
                sumX += p.X;
                sumY += p.Y;
            }

            return new OpenCvSharp.Point(sumX / points.Count, sumY / points.Count);
        }
    }
}