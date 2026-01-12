using ImageHandle.Helpers;
using ImageHandle.Models;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Input;

namespace ImageHandle.ViewModels
{
    public class ImageShowViewModel : ViewModelBase
    {
        public ImageShowViewModel()
        {
            NoneParamOpCommand = new Commands.Command<ImgOpNoneParamEnum>(ProcessImageNoneParam);
            InitImgOpNoneParam();

            OneParamNumOpCommand = new Commands.Command<ImgOpOneParamNumEnum>(ProcessImageOneParamNum);
            InitImgOpOneParamNum();

            OneParamEnumOpCommand = new Commands.Command<ImgOpOneParamEnumEnum>(ProcessImageOneParamEnum);
            InitImgOpOneParamEnum();
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

        #region 图像单操作 无参

        /// <summary>
        /// 图像操作集合
        /// </summary>
        public ObservableCollection<ImgOperateNoneParam> OperationsNoneParam { get; } = new();

        private void InitImgOpNoneParam()
        {
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.CvtColorToGray, ImageOperateMethods.CvtColorToGray));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.CvtColorToLab, ImageOperateMethods.CvtColorToLab));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.CvtColorToHSV, ImageOperateMethods.CvtColorToHSV));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.CvtColorToRGB, ImageOperateMethods.CvtColorToRGB));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.EqualizeHist, ImageOperateMethods.EqualizeHist));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.Negation, ImageOperateMethods.Negation));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.Otsu, ImageOperateMethods.Otsu));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.RetroEffect, ImageOperateMethods.RetroEffect));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.FusedCastEffect, ImageOperateMethods.FusedCastEffect));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.FrozenEffect, ImageOperateMethods.FrozenEffect));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.ComicEffect, ImageOperateMethods.ComicEffect));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.FleetingEffect, ImageOperateMethods.FleetingEffect));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.USM, ImageOperateMethods.USM));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.AutoWhithBalance, ImageOperateMethods.AutoWhithBalance));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.Buffing, ImageOperateMethods.Buffing));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.Whitening, ImageOperateMethods.Whitening));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.LowIlluminationEnhance, ImageOperateMethods.LowIlluminationEnhance));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.Transpose, ImageOperateMethods.Transpose));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.Smooth, ImageOperateMethods.Smooth));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.WaveletTransform, ImageOperateMethods.WaveletTransform));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.SepFilter2D, ImageOperateMethods.SepFilter2D));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.Remaping, ImageOperateMethods.Remaping));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.Perspect, ImageOperateMethods.Perspect));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.Shear, ImageOperateMethods.Shear));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.LineCorrect, ImageOperateMethods.LineCorrect));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.DCT, ImageOperateMethods.DCT));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.Contours, ImageOperateMethods.Contours));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.Hull, ImageOperateMethods.Hull));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.ConvexityDefects, ImageOperateMethods.ConvexityDefects));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.DNN_Caffe, ImageOperateMethods.DNN_Caffe));
            OperationsNoneParam.Add(new ImgOperateNoneParam(ImgOpNoneParamEnum.DNN_TensorFlow, ImageOperateMethods.DNN_TensorFlow));
        }

        public ICommand NoneParamOpCommand { get; }

        /// <summary>
        /// 图像处理 无参数方法
        /// </summary>
        /// <param name="operation"></param>
        private void ProcessImageNoneParam(ImgOpNoneParamEnum operation)
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
            ImgOperateNoneParam singalOperate = OperationsNoneParam.First(x => x.OperationType == operation);
            if (singalOperate == null)
            {
                MessageBox.Show($"方法:{operation}未注册");
                return;
            }
            DstMat = singalOperate.Func(srcImg);
        }

        #endregion 图像单操作 无参

        #region 图像单操作 一个参数 数值类

        /// <summary>
        /// 图像操作集合 一个参数 数值类
        /// </summary>
        public ObservableCollection<ImgOpOneParamNumBase> OperationsOneParamNum { get; } = new();

        private void InitImgOpOneParamNum()
        {
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(
                 "像素值",
                ImgOpOneParamNumEnum.Sketch, ImageOperateMethods.Sketch,
                "Int类型", "128"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(
                 "像素值",
                ImgOpOneParamNumEnum.Emboss, ImageOperateMethods.Emboss,
                "", "128"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<double>(
                 "去雾因子",
                ImgOpOneParamNumEnum.FastDehazing, ImageOperateMethods.FastDehazing,
                "double类型", "0.85"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(
                 "邻域大小",
                ImgOpOneParamNumEnum.GroundGlass, ImageOperateMethods.GroundGlass,
                "int类型，奇数，大于3", "5"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(
                 "噪声点数",
                ImgOpOneParamNumEnum.AddSaltNoise, ImageOperateMethods.AddSaltNosie,
                "", "2500"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(
                "噪声点数",
                ImgOpOneParamNumEnum.AddPepperNoise, ImageOperateMethods.AddPepperNoise,
                "", "2500"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(
                "噪声点数",
                ImgOpOneParamNumEnum.AddSaltPepperNoise, ImageOperateMethods.AddSaltPepperNoise,
                "", "2500"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(
                "滤波器大小",
                ImgOpOneParamNumEnum.Blur, ImageOperateMethods.Blur,
                "int类型，奇数", "3"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(
                "滤波器大小",
                ImgOpOneParamNumEnum.MedianBlur, ImageOperateMethods.MedianBlur,
                "int类型，奇数", "3"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(
                "滤波器大小",
                ImgOpOneParamNumEnum.BilateralFilter, ImageOperateMethods.BilateralFilter,
                "int类型，奇数", "3"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(
                "滤波器大小",
                ImgOpOneParamNumEnum.GaussianBlur, ImageOperateMethods.GaussianBlur,
                "int类型，奇数", "3"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(
                "滤波器大小",
                ImgOpOneParamNumEnum.BoxFilter, ImageOperateMethods.BoxFilter,
                "int类型，奇数", "3"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(
                "采样次数",
                ImgOpOneParamNumEnum.PyrUp, ImageOperateMethods.PyrUp,
                "int类型，采样次数不易过大", "1"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(
                "采样次数",
                ImgOpOneParamNumEnum.PyrDown, ImageOperateMethods.PyrDown,
                "int类型，采样次数不易过大", "1"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<double>(
                "最小面积",
                ImgOpOneParamNumEnum.BoundingRect, ImageOperateMethods.BoundingRect,
                "double类型", "200.0"));
            OperationsOneParamNum.Add(new ImgOperateOneParamNum<double>(
                "最小面积",
                ImgOpOneParamNumEnum.BoundingCircle, ImageOperateMethods.BoundingCircle,
                "double类型", "200.0"));
        }

        public ICommand OneParamNumOpCommand { get; }

        /// <summary>
        /// 图像处理 一个参数方法 数值类
        /// </summary>
        /// <param name="operation"></param>
        private void ProcessImageOneParamNum(ImgOpOneParamNumEnum operation)
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
            ImgOpOneParamNumBase operate = OperationsOneParamNum.First(x => x.OperationType == operation);
            if (operate == null)
            {
                MessageBox.Show($"方法:{operation}未注册");
                return;
            }
            if (operate.CheckParamFormat() == false)
            {
                MessageBox.Show($"参数:{operate.LabelText} 格式错误");
                return;
            }
            DstMat = operate.Execute(srcImg);
        }

        #endregion 图像单操作 一个参数 数值类

        #region 图像单操作 一个参数 枚举

        /// <summary>
        /// 图像操作集合 一个参数 枚举
        /// </summary>
        public ObservableCollection<ImgOpOneParamEnumBase> OperationsOneParamEnum { get; } = new();

        private void InitImgOpOneParamEnum()
        {
            OperationsOneParamEnum.Add(new ImgOperateOneParamEnum<FlipMode>(
                 "方向",
                ImgOpOneParamEnumEnum.Flip, ImageOperateMethods.Flip,
                ""));
            OperationsOneParamEnum.Add(new ImgOperateOneParamEnum<ColormapTypes>(
                 "方式",
                ImgOpOneParamEnumEnum.ApplyColorMap, ImageOperateMethods.ApplyColorMap,
                ""));
            OperationsOneParamEnum.Add(new ImgOperateOneParamEnum<EdegTypeEnum>(
                "检测算法",
                ImgOpOneParamEnumEnum.Edge, ImageOperateMethods.Edge,
                ""));
            OperationsOneParamEnum.Add(new ImgOperateOneParamEnum<ClassifierEnum>(
                "分类器",
                ImgOpOneParamEnumEnum.FeatureRecogn, ImageOperateMethods.FeatureRecogn,
                ""));
            OperationsOneParamEnum.Add(new ImgOperateOneParamEnum<SkinDefectEnum>(
                "算法",
                ImgOpOneParamEnumEnum.SkinDefect, ImageOperateMethods.SkinDefect,
                ""));
            OperationsOneParamEnum.Add(new ImgOperateOneParamEnum<SymmDirection>(
                "方向",
                ImgOpOneParamEnumEnum.CompleteSymm, ImageOperateMethods.CompleteSymm,
                ""));
            OperationsOneParamEnum.Add(new ImgOperateOneParamEnum<BGREnum>(
                "通道",
                ImgOpOneParamEnumEnum.BGRSingle, ImageOperateMethods.BGRSingle,
                ""));
        }

        public ICommand OneParamEnumOpCommand { get; }

        /// <summary>
        /// 图像处理 一个参数方法 数值类
        /// </summary>
        /// <param name="operation"></param>
        private void ProcessImageOneParamEnum(ImgOpOneParamEnumEnum operation)
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
            ImgOpOneParamEnumBase operate = OperationsOneParamEnum.First(x => x.OperationType == operation);
            if (operate == null)
            {
                MessageBox.Show($"方法:{operation}未注册");
                return;
            }
            if (operate.CheckParamFormat() == false)
            {
                MessageBox.Show($"参数:{operate.LabelText} 格式错误");
                return;
            }
            DstMat = operate.Execute(srcImg);
        }

        #endregion 图像单操作 一个参数 枚举
    }

    #region 无其他参数

    /// <summary>
    // 图像操作 无其他参数
    /// </summary>
    public class ImgOperateNoneParam
    {
        public string DisplayName { get; }
        public ImgOpNoneParamEnum OperationType { get; }
        public Func<Mat, Mat> Func { get; }

        public ImgOperateNoneParam(ImgOpNoneParamEnum operationType, Func<Mat, Mat> func)
        {
            // DisplayName = displayName;
            OperationType = operationType;
            Func = func;
            var attribute = func.Method
          .GetCustomAttributes(typeof(MethodCNNameAttribute), false)
          .OfType<MethodCNNameAttribute>()
          .FirstOrDefault();
            DisplayName = attribute?.Name ?? "";
        }
    }

    #endregion 无其他参数

    #region 一个参数 int double类型

    public class ImgOperateOneParamNum<T> : ImgOpOneParamNumBase where T : INumber<T>
    {
        public ImgOperateOneParamNum(string labelText, ImgOpOneParamNumEnum operationType, Func<Mat, T, Mat> func, string toolTips = "", string defeatValue = "0")
            : base()
        {
            LabelText = labelText;
            Func = func;
            OperationType = operationType;
            InputParam = defeatValue;
            ParamToolText = toolTips;
            var attribute = func.Method
.GetCustomAttributes(typeof(MethodCNNameAttribute), false)
.OfType<MethodCNNameAttribute>()
.FirstOrDefault();
            ButtonText = attribute?.Name ?? "";
        }

        public Func<Mat, T, Mat> Func { get; }

        public override bool CheckParamFormat()
        {
            if (string.IsNullOrWhiteSpace(InputParam))
            {
                InputParam = "0";
                return true;
            }

            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(InputParam, out int intValue))
                {
                    return intValue >= 0;
                }
            }
            else if (typeof(T) == typeof(double))
            {
                if (double.TryParse(InputParam, out double doubleValue))
                {
                    return doubleValue >= 0;
                }
            }
            else if (typeof(T) == typeof(float))
            {
                if (float.TryParse(InputParam, out float floatValue))
                {
                    return floatValue >= 0;
                }
            }
            return false;
        }

        public override Mat Execute(Mat srcImg)
        {
            T para = ConvertInputValue(InputParam);
            return Func(srcImg, para);
        }

        private T ConvertInputValue(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return T.Zero;
            }

            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(input, out int intValue))
                    return (T)(object)intValue;
            }
            else if (typeof(T) == typeof(double))
            {
                if (double.TryParse(input, out double doubleValue))
                    return (T)(object)doubleValue;
            }
            else if (typeof(T) == typeof(float))
            {
                if (float.TryParse(input, out float floatValue))
                    return (T)(object)floatValue;
            }

            throw new ArgumentException($"无法将 '{input}' 转换为 {typeof(T).Name}");
        }
    }

    #endregion 一个参数 int double类型

    #region 一个参数 枚举类型

    public class ImgOperateOneParamEnum<T> : ImgOpOneParamEnumBase where T : Enum
    {
        public ImgOperateOneParamEnum(string labelText, ImgOpOneParamEnumEnum operationType, Func<Mat, T, Mat> func, string toolTips = "", string defeatValue = "0")
            : base()
        {
            LabelText = labelText;
            Func = func;
            OperationType = operationType;
            InputParam = defeatValue;
            ParamToolText = toolTips;
            ItemsArray = Enum.GetNames(typeof(T));
            var attribute = func.Method
.GetCustomAttributes(typeof(MethodCNNameAttribute), false)
.OfType<MethodCNNameAttribute>()
.FirstOrDefault();
            ButtonText = attribute?.Name ?? "";
        }

        public Func<Mat, T, Mat> Func { get; }

        public override bool CheckParamFormat()
        {
            int index = Array.IndexOf(ItemsArray, InputParam);
            return index >= 0;
        }

        public override Mat Execute(Mat srcImg)
        {
            T para = ConvertInputValue(InputParam);
            return Func(srcImg, para);
        }

        private T ConvertInputValue(string input)
        {
            if (Enum.TryParse(typeof(T), input, true, out object result))
            {
                return (T)result;
            }
            //throw new ArgumentException($"无法将 '{input}' 转换为 {typeof(T).Name}");
            return default;
        }
    }

    #endregion 一个参数 枚举类型
}