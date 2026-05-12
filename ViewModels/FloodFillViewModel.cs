using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageHandle.Models;
using Microsoft.Win32;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageHandle.ViewModels
{
    public partial class FloodFillViewModel : ObservableObject
    {
        public FloodFillViewModel()
        {
            _matCollection = new ObservableCollection<Mat>();
            _matCollection.CollectionChanged += OnCollectionChanged;
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
                Push(new Mat(SrcImgPath));

                IsCoordinateModeEnabled = false;
                SelectedCoordinate.X = 0;
                SelectedCoordinate.Y = 0;
                SelectedCoordinate.IsSelected = false;
                CurrentCoordinate.X = 0;
                CurrentCoordinate.Y = 0;
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
        private System.Windows.Media.Color _floodFillColor = Colors.Red;

        [RelayCommand]
        private void ProcessFloodFillColor()
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color selectedColor = colorDialog.Color;
                FloodFillColor = System.Windows.Media.Color.FromArgb(
                    selectedColor.A,
                    selectedColor.R,
                    selectedColor.G,
                    selectedColor.B);
            }
        }

        [ObservableProperty]
        private int _downDiff = 0;

        [ObservableProperty]
        private int _upDiff = 0;

        private bool _isCoordinateModeEnabled = false;
        private CoordinateModel _currentCoordinate = new CoordinateModel();
        private CoordinateModel _selectedCoordinate = new CoordinateModel();
        private string _statusMessage;

        public bool IsCoordinateModeEnabled
        {
            get => _isCoordinateModeEnabled;
            set
            {
                _isCoordinateModeEnabled = value;
                OnPropertyChanged();

                // 更新状态信息
                if (value)
                {
                    StatusMessage = "坐标模式已启用 - 在图片上移动鼠标查看坐标，点击选择坐标";
                }
                else
                {
                    StatusMessage = "坐标模式已禁用";
                    // 重置当前坐标显示
                    CurrentCoordinate.X = 0;
                    CurrentCoordinate.Y = 0;
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public CoordinateModel CurrentCoordinate
        {
            get => _currentCoordinate;
            set
            {
                _currentCoordinate = value;
                OnPropertyChanged();
            }
        }

        public CoordinateModel SelectedCoordinate
        {
            get => _selectedCoordinate;
            set
            {
                _selectedCoordinate = value;
                OnPropertyChanged();
            }
        }

        [RelayCommand]
        private void ToggleCoordinateMode()
        {
            IsCoordinateModeEnabled = !IsCoordinateModeEnabled;

            // 重置坐标显示
            if (!IsCoordinateModeEnabled)
            {
                CurrentCoordinate.X = 0;
                CurrentCoordinate.Y = 0;
            }
        }

        /// <summary>
        /// 将控件坐标转换为图像坐标（Stretch="Fill"）
        /// </summary>
        private System.Windows.Point ConvertToImageCoordinates(System.Windows.Point controlPosition, System.Windows.Size controlSize)
        {
            if (LastMat == null || controlSize.Width == 0 || controlSize.Height == 0)
                return new System.Windows.Point(0, 0);

            // 获取图像原始尺寸
            double imageWidth = LastMat.Width;
            double imageHeight = LastMat.Height;

            // Stretch="Fill" 的简单计算
            // 直接按比例映射
            double imageX = (controlPosition.X / controlSize.Width) * imageWidth;
            double imageY = (controlPosition.Y / controlSize.Height) * imageHeight;

            // 确保坐标在图像范围内
            imageX = Math.Max(0, Math.Min(imageX, imageWidth));
            imageY = Math.Max(0, Math.Min(imageY, imageHeight));

            return new System.Windows.Point(imageX, imageY);
        }

        // 处理鼠标移动
        public void HandleMouseMove(System.Windows.Point position, System.Windows.Size imageSize)
        {
            if (!IsCoordinateModeEnabled || LastMat == null) return;

            var imagePosition = ConvertToImageCoordinates(position, imageSize);
            CurrentCoordinate.X = Math.Round(imagePosition.X, 1);
            CurrentCoordinate.Y = Math.Round(imagePosition.Y, 1);

            //StatusMessage = $"当前坐标: X={CurrentCoordinate.X}, Y={CurrentCoordinate.Y}";
            StatusMessage = $"坐标模式开启中";
        }

        // 处理鼠标点击
        public void HandleMouseClick(System.Windows.Point position, System.Windows.Size imageSize)
        {
            if (!IsCoordinateModeEnabled || LastMat == null) return;

            var imagePosition = ConvertToImageCoordinates(position, imageSize);
            SelectedCoordinate.X = Math.Round(imagePosition.X, 1);
            SelectedCoordinate.Y = Math.Round(imagePosition.Y, 1);
            SelectedCoordinate.IsSelected = true;

            //  StatusMessage = $"已选择坐标: X={SelectedCoordinate.X}, Y={SelectedCoordinate.Y}";
            StatusMessage = $"已选择坐标";

            // 可选：短暂显示选中效果
            Task.Delay(1000).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SelectedCoordinate.IsSelected = false;
                });
            });
        }

        [RelayCommand]
        private void ProcessFloodFill()
        {
            if (LastMat == null)
            {
                MessageBox.Show("未选择图像");
                return;
            }
            if (SelectedCoordinate.X == 0 || SelectedCoordinate.Y == 0)
            {
                MessageBox.Show("未选择坐标");
                return;
            }

            Mat srcImg = LastMat.Clone();
            Mat dstImg = new Mat();
            // 水漫图像需要 BGR三通道图片
            Cv2.CvtColor(srcImg, dstImg, ColorConversionCodes.BGRA2BGR);
            OpenCvSharp.Rect re;

            // 正负差最大值对应的颜色
            Scalar loDiffColor = new OpenCvSharp.Scalar(_downDiff, _downDiff, _downDiff);
            Scalar upDiffColor = new OpenCvSharp.Scalar(_upDiff, _upDiff, _upDiff);

            Scalar fillScalar = Scalar.FromRgb(_floodFillColor.R, _floodFillColor.G, _floodFillColor.B);

            OpenCvSharp.Point location = new OpenCvSharp.Point(_selectedCoordinate.X, _selectedCoordinate.Y);
            Cv2.FloodFill(dstImg, location, fillScalar, out re, loDiffColor, upDiffColor, FloodFillFlags.Link8);

            // 判断图像水漫前后是否有变化，有变化才加入栈
            if (MatEquall(srcImg, dstImg) == false)
            {
                Push(dstImg);
            }
        }

        // 判断两张多通道的图像是否相等
        private bool MatEquall(Mat m1, Mat m2)
        {
            if (m1.Empty() && m2.Empty())
            {
                return true;
            }
            if (m1.Cols != m2.Cols || m1.Rows != m2.Rows || m1.Dims != m2.Dims)
            {
                return false;
            }
            if (m1.Size() != m2.Size() || m1.Channels() != m2.Channels() || m1.Type() != m2.Type())
            {
                return false;
            }
            if (m1.Total() * m1.ElemSize() != m2.Total() * m2.ElemSize())
            {
                return false;
            }
            Mat tempMat = new Mat();
            Cv2.BitwiseXor(m1, m2, tempMat);

            Mat[] mm = Cv2.Split(tempMat);
            foreach (Mat mm1 in mm)
            {
                if (Cv2.CountNonZero(mm1) != 0)
                    return false;
            }
            return true;
        }

        [RelayCommand]
        private void ProcessRevocation()
        {
            Pop();
            IsCoordinateModeEnabled = false;
            SelectedCoordinate.X = 0;
            SelectedCoordinate.Y = 0;
            SelectedCoordinate.IsSelected = false;
            CurrentCoordinate.X = 0;
            CurrentCoordinate.Y = 0;
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

            IsCoordinateModeEnabled = false;
            SelectedCoordinate.X = 0;
            SelectedCoordinate.Y = 0;
            SelectedCoordinate.IsSelected = false;
            CurrentCoordinate.X = 0;
            CurrentCoordinate.Y = 0;
        }
    }
}