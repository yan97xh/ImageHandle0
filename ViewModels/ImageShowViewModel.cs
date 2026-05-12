using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageHandle.Attributes;
using ImageHandle.Helpers;
using ImageHandle.Models;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Windows;

namespace ImageHandle.ViewModels
{
    public partial class ImageShowViewModel : ObservableObject
    {
        public ImageShowViewModel()
        {
            InitImgOpNoneParam();
            InitImgOpOneParamNum();
            InitImgOpOneParamEnum();
        }

        [ObservableProperty]
        private string _srcImagePath;

        [ObservableProperty]
        private Mat dstMat;

        #region 图像单操作 无参

        /// <summary>
        /// 图像操作集合
        /// </summary>
        public ObservableCollection<ImgOperateNoneParam> OperationsNoneParam { get; } = new();

        private void InitImgOpNoneParam()
        {
            List<MethodInfo> methods = GetMethodsWithAttribute<NoneParamAttribute>(typeof(ImageOperateMethods));
            foreach (ImgOpNoneParamEnum item in Enum.GetValues<ImgOpNoneParamEnum>())
            {
                MethodInfo method = methods.FirstOrDefault(m => m.Name == item.ToString());
                if (method != null)
                {
                    Func<Mat, Mat> func = (Func<Mat, Mat>)Delegate.CreateDelegate(typeof(Func<Mat, Mat>), method);
                    OperationsNoneParam.Add(new ImgOperateNoneParam(item, func));
                }
                else
                {
                    OperationsNoneParam.Add(new ImgOperateNoneParam(item, null));
                }
            }
        }

        /// <summary>
        /// 图像处理 无参数方法
        /// </summary>
        /// <param name="operation"></param>
        [RelayCommand]
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
            if (singalOperate == null || singalOperate.Func == null)
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
            List<MethodInfo> methods = GetMethodsWithAttribute<NumberParamAttribute>(typeof(ImageOperateMethods));
            foreach (ImgOpOneParamNumEnum item in Enum.GetValues<ImgOpOneParamNumEnum>())
            {
                MethodInfo method = methods.FirstOrDefault(m => m.Name == item.ToString());
                if (method == null)
                    continue;
                NumberParamAttribute attr = method.GetCustomAttribute<NumberParamAttribute>();
                if (attr == null)
                    continue;
                if (attr.Paramtype == NumberParamTypeEnum.Integer)
                {
                    Func<Mat, int, Mat> func = (Func<Mat, int, Mat>)Delegate.CreateDelegate(typeof(Func<Mat, int, Mat>), method);
                    OperationsOneParamNum.Add(new ImgOperateOneParamNum<int>(attr.DiaplayText, item, func, attr.TipText, attr.DefaultValue));
                }
                else if (attr.Paramtype == NumberParamTypeEnum.Double)
                {
                    Func<Mat, double, Mat> func = (Func<Mat, double, Mat>)Delegate.CreateDelegate(typeof(Func<Mat, double, Mat>), method);
                    OperationsOneParamNum.Add(new ImgOperateOneParamNum<double>(attr.DiaplayText, item, func, attr.TipText, attr.DefaultValue));
                }
            }
        }

        /// <summary>
        /// 图像处理 一个参数方法 数值类
        /// </summary>
        /// <param name="operation"></param>
        [RelayCommand]
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
            List<MethodInfo> methods = GetMethodsWithAttribute<EnumParamAttribute>(typeof(ImageOperateMethods));
            foreach (ImgOpOneParamEnumEnum item in Enum.GetValues<ImgOpOneParamEnumEnum>())
            {
                MethodInfo method = methods.FirstOrDefault(m => m.Name == item.ToString());
                if (method == null)
                    continue;
                EnumParamAttribute attr = method.GetCustomAttribute<EnumParamAttribute>();
                if (attr == null)
                    continue;
                var paramters = method.GetParameters();
                if (paramters.Length < 2)
                    continue;
                Type paramType = paramters[1].ParameterType;
                if (!paramType.IsEnum)
                    continue;
                Type funcType = typeof(Func<,,>).MakeGenericType(typeof(Mat), paramType, typeof(Mat));
                Delegate func = Delegate.CreateDelegate(funcType, null, method);
                Type genericType = typeof(ImgOperateOneParamEnum<>).MakeGenericType(paramType);
                object instance = Activator.CreateInstance(genericType, attr.DiaplayText, item, func, attr.TipText, attr.DefaultValue);

                OperationsOneParamEnum.Add((ImgOpOneParamEnumBase)instance);
            }
        }

        /// <summary>
        /// 图像处理 一个参数方法 数值类
        /// </summary>
        /// <param name="operation"></param>
        [RelayCommand]
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

        public ObservableCollection<DataModel> DataCollection { get; } = new();

        private List<MethodInfo> GetMethodsWithAttribute<TAttribute>(Type type) where TAttribute : Attribute
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                       .Where(m => m.GetCustomAttribute<TAttribute>() != null)
                       .ToList();
        }

        public ImgOpOneParamEnumBase Create(MethodInfo method, object target, string labelText, ImgOpOneParamEnumEnum operationType, string toolTips = "", string defeatValue = "0")
        {
            var parameters = method.GetParameters();
            Type enumType = parameters[1].ParameterType;
            Type funcType = typeof(Func<,,>).MakeGenericType(typeof(Mat), enumType, typeof(Mat));
            Delegate func = Delegate.CreateDelegate(funcType, target, method);
            Type genericType = typeof(ImgOperateOneParamEnum<>).MakeGenericType(enumType);
            object instance = Activator.CreateInstance(genericType, labelText, operationType, func, toolTips, defeatValue);
            return (ImgOpOneParamEnumBase)instance;
        }
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
            EnumType = typeof(T);
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

    public class DataModel
    {
        public Type EnumType { get; set; }
    }
}