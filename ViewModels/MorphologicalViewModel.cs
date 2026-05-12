using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageHandle.Helpers;
using ImageHandle.Models;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ImageHandle.ViewModels
{
    public partial class MorphologicalViewModel : ObservableObject
    {
        public MorphologicalViewModel()
        {
            InitOperations();
        }

        [ObservableProperty]
        private string _srcImagePath;

        [ObservableProperty]
        private Mat _dstMat;

        #region 形态学操作

        public ObservableCollection<MorphologicalOperation> Operations { get; } = new ObservableCollection<MorphologicalOperation>();

        private void InitOperations()
        {
            Operations.Add(new MorphologicalOperation()
            {
                DisplayName = "形态学操作",
                operation = MorphologicalMode.MorphologyEx,
                ParamValueList = new List<ParamValueModel>()
                {
                    new ParamValueModel("structSize","3","结构元大小"),
                    new ParamValueModel("times","1","操作次数"),
                },
                ParamEnumList = new List<ParamEnumModel>()
                {
                    new ParamEnumModel("shapes",typeof(MorphShapes),paramDisplayName:"结构元类型"),
                    new ParamEnumModel("types",typeof(MorphTypes),paramDisplayName:"操作类型"),
                }
            });
            Operations.Add(new MorphologicalOperation()
            {
                DisplayName = "MeanShift分割",
                operation = MorphologicalMode.MeanShiftFilter,
                ParamValueList = new List<ParamValueModel>()
                {
                    new ParamValueModel("maxLevel","1","金字塔最高级别"),
                    new ParamValueModel("sp","30","彩色窗口半径"),
                    new ParamValueModel("sr","20","空间窗口半径"),
                },
            });
            Operations.Add(new MorphologicalOperation()
            {
                DisplayName = "图像堆叠",
                operation = MorphologicalMode.Repeat,
                ParamValueList = new List<ParamValueModel>()
                {
                    new ParamValueModel("ny","2","垂直张数"),
                    new ParamValueModel("nx","2","水平张数"),
                },
            });
            Operations.Add(new MorphologicalOperation()
            {
                DisplayName = "距离变换",
                operation = MorphologicalMode.DistanceTranForm,
                ParamEnumList = new List<ParamEnumModel>()
                {
                new ParamEnumModel("types",typeof(DistanceTypes),paramDisplayName:"距离变换方式"),
                new ParamEnumModel("masks",typeof(DistanceTransformMasks),paramDisplayName:"掩膜方式"),
                }
            });
            Operations.Add(new MorphologicalOperation()
            {
                DisplayName = "极空间变换",
                operation = MorphologicalMode.Polar,
                ParamValueList = new List<ParamValueModel>()
                {
                  new ParamValueModel("centerX","100","中心点X"),
                  new ParamValueModel("centerY","100","中心点Y"),
                  new ParamValueModel("maxRadius","100","最大变换半径"),
                },
                ParamEnumList = new List<ParamEnumModel>()
                {
                new ParamEnumModel("flag",typeof(InterpolationFlags),paramDisplayName:"差值方式"),
                new ParamEnumModel("mode",typeof(PolarMode),paramDisplayName:"变换方式"),
                }
            });
        }

        [RelayCommand]
        private void ProcessMorphologicalOperations(MorphologicalMode mode)
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

            MorphologicalOperation op = Operations.First(x => x.operation == mode);
            if (op == null)
            {
                MessageBox.Show($"方法：{mode}未注册");
                return;
            }

            switch (mode)
            {
                case MorphologicalMode.MorphologyEx:
                    {
                        #region 结构元大小

                        string structSizeStr = op.ParamValueList.Find(x => x.ParamName == "structSize")?.ParamValue;
                        if (int.TryParse(structSizeStr, out int structSize) == false)
                        {
                            MessageBox.Show("结构元大小输入有误");
                            return;
                        }

                        #endregion 结构元大小

                        #region 次数

                        string timesStr = op.ParamValueList.Find(x => x.ParamName == "times")?.ParamValue;
                        if (int.TryParse(timesStr, out int times) == false)
                        {
                            MessageBox.Show("操作次数输入有误");
                            return;
                        }
                        if (times <= 0)
                        {
                            MessageBox.Show("操作次数输入有误");
                            return;
                        }

                        #endregion 次数

                        #region 结构元类型

                        MorphShapes shapes = (MorphShapes)op.ParamEnumList.First(x => x.ParamName == "shapes")?.GetValue();

                        #endregion 结构元类型

                        #region 操作类型

                        MorphTypes types = (MorphTypes)op.ParamEnumList.First(x => x.ParamName == "types")?.GetValue();

                        #endregion 操作类型

                        OpenCvSharp.Size _size = new OpenCvSharp.Size(structSize, structSize);

                        DstMat = ImageOperateMethods.Morphology(srcImg, shapes, _size, types, times);
                    }
                    break;

                case MorphologicalMode.MeanShiftFilter:
                    {
                        #region 金字塔最高级别

                        string maxLevelStr = op.ParamValueList.Find(x => x.ParamName == "maxLevel")?.ParamValue;
                        if (int.TryParse(maxLevelStr, out int maxLevel) == false)
                        {
                            MessageBox.Show("金字塔最高级别输入有误");
                            return;
                        }

                        #endregion 金字塔最高级别

                        #region 彩色窗口半径

                        string spStr = op.ParamValueList.Find(x => x.ParamName == "sp")?.ParamValue;
                        if (double.TryParse(spStr, out double sp) == false)
                        {
                            MessageBox.Show("彩色窗口半径输入有误");
                            return;
                        }

                        #endregion 彩色窗口半径

                        #region 空间窗口半径

                        string srStr = op.ParamValueList.Find(x => x.ParamName == "sr")?.ParamValue;
                        if (double.TryParse(srStr, out double sr) == false)
                        {
                            MessageBox.Show("空间窗口半径输入有误");
                            return;
                        }

                        #endregion 空间窗口半径

                        DstMat = ImageOperateMethods.MeanShiftFilter(srcImg, sp, sr, maxLevel);
                    }
                    break;

                case MorphologicalMode.Repeat:
                    {
                        string nyStr = op.ParamValueList.Find(x => x.ParamName == "ny")?.ParamValue;
                        if (int.TryParse(nyStr, out int ny) == false)
                        {
                            MessageBox.Show("垂直张数输入有误");
                            return;
                        }
                        if (ny <= 0)
                        {
                            MessageBox.Show("垂直张数输入有误");
                            return;
                        }
                        string nxStr = op.ParamValueList.Find(x => x.ParamName == "nx")?.ParamValue;
                        if (int.TryParse(nxStr, out int nx) == false)
                        {
                            MessageBox.Show("水平张数输入有误");
                            return;
                        }
                        if (nx <= 0)
                        {
                            MessageBox.Show("水平张数输入有误");
                            return;
                        }
                        DstMat = ImageOperateMethods.Repeat(srcImg, ny, nx);
                    }
                    break;

                case MorphologicalMode.DistanceTranForm:
                    {
                        DistanceTypes types = (DistanceTypes)op.ParamEnumList.First(x => x.ParamName == "types")?.GetValue();
                        DistanceTransformMasks masks = (DistanceTransformMasks)op.ParamEnumList.First(x => x.ParamName == "masks")?.GetValue();
                        if (types != DistanceTypes.L1 && types != DistanceTypes.L2 && types != DistanceTypes.C)
                        {
                            MessageBox.Show("只支持L1 L2 C");
                            return;
                        }
                        if (types != DistanceTypes.L2 && masks != DistanceTransformMasks.Mask3)
                        {
                            MessageBox.Show("非L2的变换只能选择Mask3");
                            return;
                        }
                        DstMat = ImageOperateMethods.DistanceTranForm(srcImg, types, masks);
                    }
                    break;

                case MorphologicalMode.Polar:
                    {
                        #region 中心点X

                        string centerXStr = op.ParamValueList.Find(x => x.ParamName == "centerX")?.ParamValue;
                        if (double.TryParse(centerXStr, out double centerX) == false)
                        {
                            MessageBox.Show("中心点X输入有误");
                            return;
                        }
                        if (centerX <= 0)
                        {
                            MessageBox.Show("中心点X输入有误");
                            return;
                        }

                        #endregion 中心点X

                        #region 中心点Y

                        string centerYStr = op.ParamValueList.Find(x => x.ParamName == "centerY")?.ParamValue;
                        if (double.TryParse(centerYStr, out double centerY) == false)
                        {
                            MessageBox.Show("中心点Y输入有误");
                            return;
                        }
                        if (centerY <= 0)
                        {
                            MessageBox.Show("中心点Y输入有误");
                            return;
                        }

                        #endregion 中心点Y

                        #region 最大变换半径

                        string maxRadiusStr = op.ParamValueList.Find(x => x.ParamName == "maxRadius")?.ParamValue;
                        if (double.TryParse(maxRadiusStr, out double maxRadius) == false)
                        {
                            MessageBox.Show("最大变换半径输入有误");
                            return;
                        }
                        if (maxRadius <= 0)
                        {
                            MessageBox.Show("最大变换半径输入有误");
                            return;
                        }

                        #endregion 最大变换半径

                        #region 差值方式

                        InterpolationFlags flag = (InterpolationFlags)op.ParamEnumList.First(x => x.ParamName == "flag")?.GetValue();

                        #endregion 差值方式

                        #region 变换方式

                        PolarMode polarMode = (PolarMode)op.ParamEnumList.First(x => x.ParamName == "mode")?.GetValue();

                        #endregion 变换方式

                        OpenCvSharp.Point2f center = new Point2f((float)centerX, (float)centerY);
                        DstMat = ImageOperateMethods.Polar(srcImg, center, maxRadius, flag, polarMode);
                    }
                    break;
            }
        }

        #endregion 形态学操作
    }

    public class MorphologicalOperation
    {
        public string DisplayName { get; set; }

        //值参数集合
        public List<ParamValueModel> ParamValueList { get; set; } = new List<ParamValueModel>();

        public List<ParamBoolModel> ParamBoolList { get; set; } = new List<ParamBoolModel>();

        public List<ParamEnumModel> ParamEnumList { get; set; } = new List<ParamEnumModel>();

        public MorphologicalMode operation { get; set; }

        public string Remark { get; set; }
    }
}