using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageHandle.Models;
using Microsoft.Win32;
using OpenCvSharp;
using System.Windows;
using System.Windows.Input;

namespace ImageHandle.ViewModels
{
    public partial class MosaicViewModel : ObservableObject
    {
        public MosaicViewModel()
        {
            MosaicSizeItemsArray = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        }

        [ObservableProperty]
        private string _srcImgPath;

        [ObservableProperty]
        private string _saveImagePath;

        private Mat _srcOriMat = null;

        [ObservableProperty]
        private Mat _mainMat;

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
                Mat mat = new Mat(SrcImgPath);
                MainMat = mat;
                _srcOriMat = mat.Clone();
                mosaicFlagArray = new bool[MainMat.Rows, MainMat.Cols];
            }
        }

        [RelayCommand]
        private void SaveImage()
        {
            if (MainMat == null)
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

                bool success = Cv2.ImWrite(savePath, MainMat);
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
        private int _mosaicSize = 2;

        public int[] MosaicSizeItemsArray { get; }

        [ObservableProperty]
        private bool _isMosaicEnabled = false;

        private CoordinateModel _currentCoordinate = new CoordinateModel();

        public CoordinateModel CurrentCoordinate
        {
            get => _currentCoordinate;
            set
            {
                _currentCoordinate = value;
                OnPropertyChanged();
            }
        }

        private bool _isStartMosaic = false;
        private bool[,] mosaicFlagArray = null;

        [RelayCommand]
        private void ProcessMouseDown()
        {
            if (MainMat == null)
                return;
            if (_isMosaicEnabled == false)
                return;
            _isStartMosaic = true;
        }

        [RelayCommand]
        private void ProcessMouseUp()
        {
            _isStartMosaic = false;
        }

        /// <summary>
        /// 将控件坐标转换为图像坐标（Stretch="Fill"）
        /// </summary>
        private System.Windows.Point ConvertToImageCoordinates(System.Windows.Point controlPosition, System.Windows.Size controlSize)
        {
            if (MainMat == null || controlSize.Width == 0 || controlSize.Height == 0)
                return new System.Windows.Point(0, 0);

            // 获取图像原始尺寸
            double imageWidth = MainMat.Width;
            double imageHeight = MainMat.Height;

            // Stretch="Fill" 的简单计算
            // 直接按比例映射
            double imageX = (controlPosition.X / controlSize.Width) * imageWidth;
            double imageY = (controlPosition.Y / controlSize.Height) * imageHeight;

            // 确保坐标在图像范围内
            imageX = Math.Max(0, Math.Min(imageX, imageWidth));
            imageY = Math.Max(0, Math.Min(imageY, imageHeight));

            return new System.Windows.Point(imageX, imageY);
        }

        [RelayCommand]
        private void ProcessMouseMove(System.Windows.Controls.Image image)
        {
            if (!_isStartMosaic || MainMat == null || mosaicFlagArray == null) return;
            if (image == null)
                return;
            System.Windows.Point pos = Mouse.GetPosition(image);
            System.Windows.Size size = new System.Windows.Size(image.ActualWidth, image.ActualHeight);

            var imagePosition = ConvertToImageCoordinates(pos, size);
            CurrentCoordinate.X = Math.Round(imagePosition.X, 1);
            CurrentCoordinate.Y = Math.Round(imagePosition.Y, 1);

            Mat srcImg = MainMat.Clone();
            int posX = (int)CurrentCoordinate.X;
            int posY = (int)CurrentCoordinate.Y;
            if (mosaicFlagArray[posY, posX] == false)
            {
                mosaicFlagArray[posY, posX] = true;
                OpenCvSharp.Vec3b color = new OpenCvSharp.Vec3b();
                color = srcImg.Get<OpenCvSharp.Vec3b>(posY, posX);

                for (int i = posX - _mosaicSize; i <= posX + _mosaicSize; i++)
                {
                    for (int j = posY - _mosaicSize; j <= posY + _mosaicSize; j++)
                    {
                        //边界考虑
                        if (i < 0) i = 0;
                        if (i >= srcImg.Width) i = srcImg.Width - 1;
                        if (j < 0) j = 0;
                        if (j >= srcImg.Height) j = srcImg.Height - 1;
                        mosaicFlagArray[j, i] = true;
                        srcImg.Set(j, i, color);
                    }
                }
                MainMat = srcImg.Clone();
                srcImg.Dispose();
                //Thread.Sleep(50);
            }
        }

        [RelayCommand]
        private void Recover()
        {
            if (_srcOriMat == null)
            {
                MessageBox.Show("未选择图像");
                return;
            }
            MainMat = _srcOriMat.Clone();
            mosaicFlagArray = new bool[_srcOriMat.Rows, _srcOriMat.Cols];
            IsMosaicEnabled = false;

            CurrentCoordinate.X = 0;
            CurrentCoordinate.Y = 0;
        }
    }
}