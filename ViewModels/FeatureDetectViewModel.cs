using ImageHandle.Helpers;
using ImageHandle.Models;
using ImageHandle.Views;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ImageHandle.ViewModels
{
    public class FeatureDetectViewModel : ViewModelBase
    {
        public FeatureDetectViewModel()
        {
            FeatureDetectCommand = new Commands.Command<FeatureDetectMode>(ProcessOperations);
            InitOperations();

            FeatureDetectPointCommand = new Commands.RelayCommand(ProcessFeatureDetectPoint);

            ShowHSVRangeWindowCommand = new Commands.RelayCommand(ShowHSVRangeWindow);
            HSVRangeDetectCommand = new Commands.RelayCommand(HSVRangeDetect);
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

        #region 特征检测

        public ObservableCollection<ImgFeatureDetectOp> Operations { get; } = new();

        public ICommand FeatureDetectCommand { get; }

        private void InitOperations()
        {
            Operations.Add(new ImgFeatureDetectOp()
            {
                DisplayName = "霍夫圆检测",
                operation = FeatureDetectMode.HoughCircle,
                Remark = "霍夫圆检测",
                ParamValueList = new List<ParamValueModel>()
                {
                    new ParamValueModel("dp","1.0","dp参数","累加器分辨率与图像分辨率的反比" ),
                    new ParamValueModel("param1","70","param1","第一个方法特定参数，canny边缘检测阈值" ),
                    new ParamValueModel("param2","30","param2","中心点累加器阈值" ),
                    new ParamValueModel("minDist","80","最小圆心距","检测的圆的中心的最小距离" ),
                    new ParamValueModel("minRadius","10","最小半径"),
                    new ParamValueModel("maxRadius","60","最大半径" ),
                }
            });
            Operations.Add(new ImgFeatureDetectOp()
            {
                DisplayName = "霍夫直线检测",
                operation = FeatureDetectMode.HoughLine,
                Remark = "霍夫直线检测",
                ParamValueList = new List<ParamValueModel>()
                {
                    new ParamValueModel("rho","1.6","rho","" ),
                    new ParamValueModel("maxDist","50","最大间距","检测直线最大间距" ),
                    new ParamValueModel("theta","55","角度","" ),
                    new ParamValueModel("threshold","90","阈值参数","" ),
                    new ParamValueModel("minLength","80","最小长度")
                }
            });
            Operations.Add(new ImgFeatureDetectOp()
            {
                DisplayName = "角点检测",
                operation = FeatureDetectMode.Corner,
                Remark = "角点检测",
                ParamValueList = new List<ParamValueModel>()
                {
                    new ParamValueModel("maxCorners","100","最大角点数","" ),
                    new ParamValueModel("qualityLv","0.01","质量水平","" ),
                    new ParamValueModel("minDis","10","最小距离","" ),
                    new ParamValueModel("blockSize","16","矩形大小","" ),
                    new ParamValueModel("paramK","50","探测器参数")
                },
                ParamBoolList = new List<ParamBoolModel>()
                {
                 new ParamBoolModel("useHarris",false,"探测器启用"),
                },
            });
            Operations.Add(new ImgFeatureDetectOp()
            {
                DisplayName = "亚像素矩阵",
                operation = FeatureDetectMode.RectSubPix,
                Remark = "亚像素矩阵提取",
                ParamValueList = new List<ParamValueModel>()
                {
                    new ParamValueModel("rectWidth","100","矩形宽度","" ),
                    new ParamValueModel("rectHeight","100","矩形高度","" ),
                    new ParamValueModel("rectCenterX","50","矩形中心点X","" ),
                    new ParamValueModel("rectCenterY","50","矩形中心点Y","" ),
                }
            });
        }

        private void ProcessOperations(FeatureDetectMode detectMode)
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

            ImgFeatureDetectOp op = Operations.First(x => x.operation == detectMode);
            if (op == null)
            {
                MessageBox.Show($"方法：{detectMode}未注册");
                return;
            }

            switch (detectMode)
            {
                case FeatureDetectMode.HoughCircle:
                    {
                        string dpStr = op.ParamValueList.First(x => x.ParamName == "dp")?.ParamValue;
                        if (double.TryParse(dpStr, out double dp) == false)
                        {
                            MessageBox.Show($"dp参数输入有误");
                            return;
                        }
                        string param1Str = op.ParamValueList.First(x => x.ParamName == "param1")?.ParamValue;
                        if (double.TryParse(param1Str, out double param1) == false)
                        {
                            MessageBox.Show($"参数param1输入有误");
                            return;
                        }
                        string param2Str = op.ParamValueList.First(x => x.ParamName == "param2")?.ParamValue;
                        if (double.TryParse(param2Str, out double param2) == false)
                        {
                            MessageBox.Show($"参数param2输入有误");
                            return;
                        }
                        string minDistStr = op.ParamValueList.First(x => x.ParamName == "minDist")?.ParamValue;
                        if (double.TryParse(minDistStr, out double minDist) == false)
                        {
                            MessageBox.Show($"参数最小圆心距输入有误");
                            return;
                        }
                        string minRadiusStr = op.ParamValueList.First(x => x.ParamName == "minRadius")?.ParamValue;
                        if (int.TryParse(minRadiusStr, out int minRadius) == false)
                        {
                            MessageBox.Show($"参数最小半径输入有误");
                            return;
                        }
                        string maxRadiusStr = op.ParamValueList.First(x => x.ParamName == "maxRadius")?.ParamValue;
                        if (int.TryParse(maxRadiusStr, out int maxRadius) == false)
                        {
                            MessageBox.Show($"参数最大半径输入有误");
                            return;
                        }
                        DstMat = ImageOperateMethods.HoughCircle(srcImg, dp, minDist, param1, param2, minRadius, maxRadius);
                    }
                    break;

                case FeatureDetectMode.HoughLine:
                    {
                        string rhoStr = op.ParamValueList.First(x => x.ParamName == "rho")?.ParamValue;
                        if (double.TryParse(rhoStr, out double rho) == false)
                        {
                            MessageBox.Show($"rho参数输入有误");
                            return;
                        }
                        string maxDistStr = op.ParamValueList.First(x => x.ParamName == "maxDist")?.ParamValue;
                        if (double.TryParse(maxDistStr, out double maxDist) == false)
                        {
                            MessageBox.Show($"参数最大间距输入有误");
                            return;
                        }
                        string thetaStr = op.ParamValueList.First(x => x.ParamName == "theta")?.ParamValue;
                        if (double.TryParse(thetaStr, out double theta) == false)
                        {
                            MessageBox.Show($"参数角度输入有误");
                            return;
                        }
                        string thresholdStr = op.ParamValueList.First(x => x.ParamName == "threshold")?.ParamValue;
                        if (int.TryParse(thresholdStr, out int threshold) == false)
                        {
                            MessageBox.Show($"参数阈值输入有误");
                            return;
                        }
                        string minLengthStr = op.ParamValueList.First(x => x.ParamName == "minLength")?.ParamValue;
                        if (double.TryParse(minLengthStr, out double minLength) == false)
                        {
                            MessageBox.Show($"参数最小长度输入有误");
                            return;
                        }
                        DstMat = ImageOperateMethods.HoughLines(srcImg, rho, theta, threshold, minLength, maxDist);
                    }
                    break;

                case FeatureDetectMode.Corner:
                    {
                        string maxCornersStr = op.ParamValueList.First(x => x.ParamName == "maxCorners")?.ParamValue;
                        if (int.TryParse(maxCornersStr, out int maxCorners) == false)
                        {
                            MessageBox.Show($"最大角点数输入有误");
                            return;
                        }
                        string qualityLvStr = op.ParamValueList.First(x => x.ParamName == "qualityLv")?.ParamValue;
                        if (double.TryParse(qualityLvStr, out double qualityLv) == false)
                        {
                            MessageBox.Show($"参数质量水平输入有误");
                            return;
                        }
                        string minDisStr = op.ParamValueList.First(x => x.ParamName == "minDis")?.ParamValue;
                        if (double.TryParse(minDisStr, out double minDis) == false)
                        {
                            MessageBox.Show($"参数最小距离输入有误");
                            return;
                        }
                        string blockSizeStr = op.ParamValueList.First(x => x.ParamName == "blockSize")?.ParamValue;
                        if (int.TryParse(blockSizeStr, out int blockSize) == false)
                        {
                            MessageBox.Show($"参数矩形大小输入有误");
                            return;
                        }
                        string paramKStr = op.ParamValueList.First(x => x.ParamName == "paramK")?.ParamValue;
                        if (double.TryParse(paramKStr, out double paramK) == false)
                        {
                            MessageBox.Show($"探测器参数输入有误");
                            return;
                        }
                        bool useHarris = (bool)op.ParamBoolList.First(x => x.ParamName == "useHarris")?.ParamValue;
                        DstMat = ImageOperateMethods.ConnerGoodFeatures(srcImg, maxCorners, qualityLv, minDis, blockSize, useHarris, paramK);
                    }
                    break;

                case FeatureDetectMode.RectSubPix:
                    {
                        string rectWidthStr = op.ParamValueList.First(x => x.ParamName == "rectWidth")?.ParamValue;
                        if (int.TryParse(rectWidthStr, out int rectWidth) == false)
                        {
                            MessageBox.Show($"矩形宽度输入有误");
                            return;
                        }
                        string rectHeightStr = op.ParamValueList.First(x => x.ParamName == "rectHeight")?.ParamValue;
                        if (int.TryParse(rectHeightStr, out int rectHeight) == false)
                        {
                            MessageBox.Show($"矩形高度输入有误");
                            return;
                        }
                        string rectCenterXStr = op.ParamValueList.First(x => x.ParamName == "rectCenterX")?.ParamValue;
                        if (double.TryParse(rectCenterXStr, out double rectCenterX) == false)
                        {
                            MessageBox.Show($"矩形中心点X输入有误");
                            return;
                        }
                        string rectCenterYStr = op.ParamValueList.First(x => x.ParamName == "rectCenterY")?.ParamValue;
                        if (double.TryParse(rectCenterYStr, out double rectCenterY) == false)
                        {
                            MessageBox.Show($"矩形中心点Y输入有误");
                            return;
                        }

                        if (rectCenterX <= 0 || rectCenterX > srcImg.Width)
                        {
                            MessageBox.Show("提取矩形区域中心X必须在原图中!");
                            return;
                        }
                        if (rectCenterY <= 0 || rectCenterY > srcImg.Height)
                        {
                            MessageBox.Show("提取矩形区域中心Y必须在原图中!");
                            return;
                        }

                        DstMat = ImageOperateMethods.RectSubPix(srcImg, rectWidth, rectHeight, rectCenterX, rectCenterY);
                    }
                    break;
            }
        }

        #endregion 特征检测

        #region 特征点提取

        public FeatureDetectPointModel featureDetectPointModel { get; set; } = new FeatureDetectPointModel();

        public ICommand FeatureDetectPointCommand { get; }

        private void ProcessFeatureDetectPoint()
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

            DstMat = featureDetectPointModel.Excute(srcImg);
        }

        #endregion 特征点提取

        #region HSV范围提取

        private int _h_min = 0;
        private int _h_max = 180;
        private int _s_min = 0;
        private int _s_max = 255;
        private int _v_min = 0;
        private int _v_max = 255;

        public int H_min
        {
            get => _h_min;
            set
            {
                if (value <= _h_max)
                {
                    _h_min = value;
                    OnPropertyChanged();
                }
            }
        }

        public int H_max
        {
            get => _h_max;
            set
            {
                if (value >= _h_min)
                {
                    _h_max = value;
                    OnPropertyChanged();
                }
            }
        }

        public int S_min
        {
            get => _s_min;
            set
            {
                if (value <= _s_max)
                {
                    _s_min = value;
                    OnPropertyChanged();
                }
            }
        }

        public int S_max
        {
            get => _s_max;
            set
            {
                if (value >= _s_min)
                {
                    _s_max = value;
                    OnPropertyChanged();
                }
            }
        }

        public int V_min
        {
            get => _v_min;
            set
            {
                if (value <= _v_max)
                {
                    _v_min = value;
                    OnPropertyChanged();
                }
            }
        }

        public int V_max
        {
            get => _v_max;
            set
            {
                if (value >= _v_min)
                {
                    _v_max = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ShowHSVRangeWindowCommand { get; }

        private void ShowHSVRangeWindow()
        {
            HSVRangeWindow rangeWindow = new HSVRangeWindow();
            rangeWindow.ShowDialog();
        }

        public ICommand HSVRangeDetectCommand { get; }

        private void HSVRangeDetect()
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

            Scalar minScalar = new Scalar(_h_min, _s_min, _v_min);
            Scalar maxScalar = new Scalar(_h_max, _s_max, _v_max);

            DstMat = ImageOperateMethods.HSVRecogn(srcImg, minScalar, maxScalar);
        }

        #endregion HSV范围提取
    }

    public class ImgFeatureDetectOp
    {
        public string DisplayName { get; set; }

        //值参数集合
        public List<ParamValueModel> ParamValueList { get; set; } = new List<ParamValueModel>();

        public List<ParamBoolModel> ParamBoolList { get; set; } = new List<ParamBoolModel>();

        public FeatureDetectMode operation { get; set; }

        public string Remark { get; set; }
    }
}