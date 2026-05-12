using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageHandle.Commands;
using Microsoft.Win32;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace ImageHandle.ViewModels
{
    public partial class ROIInsertViewModel : ObservableObject
    {
        private readonly double defaultROIWidth = 100;
        private readonly double defaultROIHeight = 100;

        public ROIInsertViewModel()
        {
            _matCollection = new ObservableCollection<Mat>();
            _matCollection.CollectionChanged += OnCollectionChanged;
        }

        [ObservableProperty]
        private System.Windows.Point topLeftP;

        [ObservableProperty]
        private System.Windows.Point bottomRightP;

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
            TopLeftP = new System.Windows.Point(center.X - defaultROIWidth / 2, center.Y - defaultROIHeight / 2);
            BottomRightP = new System.Windows.Point(center.X + defaultROIWidth / 2, center.Y + defaultROIHeight / 2);
        }

        [RelayCommand]
        private void ClearROI()
        {
            IsROIDetectEnabled = false;
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

        [ObservableProperty]
        private Mat _insertMat;

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

        [RelayCommand]
        private void SelectInsertImg()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "请选择图片";
            openFileDialog.Filter = "所有图片文件(*.jpg;*.bmp;*.jpeg;*.png;*.tif)|*.jpg;*.bmp;*.jpeg;*.png;*.tif";
            // dialog.InitialDirectory = @"E:\myPictures\Lena";

            if (openFileDialog.ShowDialog() == true)
            {
                InsertMat = new Mat(openFileDialog.FileName);
            }
        }

        [ObservableProperty]
        private Brush _rOIRectangleColor = Brushes.Red;

        [RelayCommand]
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

        [RelayCommand]
        private void ProcessRevocation()
        {
            Pop();
            IsROIDetectEnabled = false;
        }

        [RelayCommand]
        private void Recover()
        {
            if (_matCollection.Count <= 0)
            {
                MessageBox.Show("未选择图像");
                return;
            }
            Mat mat = _matCollection[0];
            _matCollection.Clear();
            _matCollection.Add(mat);

            IsROIDetectEnabled = false;
        }

        // 显示控件上的图像展示尺寸（由视图层设置：通常是 Image 或 ROICanvas 的实际显示尺寸）
        [ObservableProperty]
        private double _imageDisplayWidth;

        [ObservableProperty]
        private double _imageDisplayHeight;

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

        [RelayCommand]
        private void InsertImage()
        {
            if (LastMat == null)
            {
                MessageBox.Show("未选择输入图像");
                return;
            }
            if (InsertMat == null)
            {
                MessageBox.Show("未选择插入图像");
                return;
            }
            if (!IsROIDetectEnabled)
            {
                MessageBox.Show("请先设置ROI区域");
                return;
            }
            Mat resultMat = LastMat.Clone();
            var roiRect = GetImageRect();
            if (roiRect == null)
            {
                MessageBox.Show("无法将 ROI 转换到图像坐标（可能未提供显示尺寸或 ROI 在显示区域外）");
                return;
            }

            // 使用 OpenCvSharp.Rect 提取子图
            var rect = roiRect.Value;
            Mat roi = new Mat(resultMat, rect);
            Mat resizedInsertMat = new Mat();
            Cv2.Resize(InsertMat, resizedInsertMat, new OpenCvSharp.Size(roi.Width, roi.Height));
            resizedInsertMat.CopyTo(roi);
            Push(resultMat);
            IsROIDetectEnabled = false;
        }
    }
}