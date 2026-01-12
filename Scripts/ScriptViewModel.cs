using ImageHandle.Helpers;
using ImageHandle.ViewModels;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.Reflection;

namespace ImageHandle.Scripts
{
    internal class ScriptViewModel : ViewModelBase
    {
        public ScriptViewModel()
        {
            RegisterMethodes();
            MethodNames = new ObservableCollection<string>(ScriptService.Instance.GetRegisteredMethodNames(_isCN));
        }

        private void RegisterMethodes()
        {
            static void reg(string name, params ScriptParamModel[] ps)
            {
                var list = (ps == null || ps.Length == 0) ? new List<ScriptParamModel>() : ps.ToList();
                MethodInfo methodInfo = typeof(ImageOperateMethods).GetMethod(name,
                    BindingFlags.Public | BindingFlags.Static);
                string nameCN = "";
                var attribute = methodInfo
                    .GetCustomAttributes(typeof(MethodCNNameAttribute), false)
                    .OfType<MethodCNNameAttribute>()
                    .FirstOrDefault();
                nameCN = attribute?.Name ?? "";
                ScriptService.Instance.Register(name, list, nameCN);
            }

            // 无参数的方法
            reg(nameof(ImageOperateMethods.CvtColorToGray));
            reg(nameof(ImageOperateMethods.CvtColorToHSV));
            reg(nameof(ImageOperateMethods.CvtColorToLab));
            reg(nameof(ImageOperateMethods.CvtColorToRGB));
            reg(nameof(ImageOperateMethods.USM));
            reg(nameof(ImageOperateMethods.EqualizeHist));
            reg(nameof(ImageOperateMethods.Otsu));
            reg(nameof(ImageOperateMethods.Remaping));
            reg(nameof(ImageOperateMethods.Buffing));
            reg(nameof(ImageOperateMethods.Hull));
            reg(nameof(ImageOperateMethods.Negation));
            reg(nameof(ImageOperateMethods.ComicEffect));
            reg(nameof(ImageOperateMethods.FleetingEffect));
            reg(nameof(ImageOperateMethods.FusedCastEffect));
            reg(nameof(ImageOperateMethods.FrozenEffect));
            reg(nameof(ImageOperateMethods.RetroEffect));
            reg(nameof(ImageOperateMethods.SepFilter2D));
            reg(nameof(ImageOperateMethods.Shear));
            reg(nameof(ImageOperateMethods.Smooth));
            reg(nameof(ImageOperateMethods.Transpose));
            reg(nameof(ImageOperateMethods.WaveletTransform));
            reg(nameof(ImageOperateMethods.Whitening));
            reg(nameof(ImageOperateMethods.Contours));

            // 带参数的方法
            reg(nameof(ImageOperateMethods.PyrDown),
                new ScriptParamModel("times", ParamType.Interger, "1"));
            reg(nameof(ImageOperateMethods.PyrUp),
                new ScriptParamModel("times", ParamType.Interger, "1"));
            reg(nameof(ImageOperateMethods.Repeat),
                new ScriptParamModel("ny", ParamType.Interger, "1", null, "Y重复次数"),
                new ScriptParamModel("nx", ParamType.Interger, "1", null, "X重复次数"));
            reg(nameof(ImageOperateMethods.Flip),
                new ScriptParamModel("filpMode", ParamType.Enum, "", typeof(OpenCvSharp.FlipMode), "翻转方向"));
            reg(nameof(ImageOperateMethods.Sketch),
                new ScriptParamModel("meanv", ParamType.Interger, "127", null, "平均像素"));
            reg(nameof(ImageOperateMethods.Emboss),
                new ScriptParamModel("kvalue", ParamType.Interger, "127", null, "平均像素"));
            reg(nameof(ImageOperateMethods.GroundGlass),
                new ScriptParamModel("radius", ParamType.Interger, "5", null, "模糊半径"));
            reg(nameof(ImageOperateMethods.AddPepperNoise),
                new ScriptParamModel("nums", ParamType.Interger, "500", null, "噪声点数量"));
            reg(nameof(ImageOperateMethods.AddGaussianNoise),
                new ScriptParamModel("nums", ParamType.Interger, "500", null, "噪声点数量"));
            reg(nameof(ImageOperateMethods.AddSaltNosie),
                new ScriptParamModel("nums", ParamType.Interger, "500", null, "噪声点数量"));
            reg(nameof(ImageOperateMethods.AddSaltPepperNoise),
                new ScriptParamModel("nums", ParamType.Interger, "500", null, "噪声点数量"));
            reg(nameof(ImageOperateMethods.Edge),
                new ScriptParamModel("EdgeMode", ParamType.Enum, "", typeof(ImageHandle.Models.EdegTypeEnum), "边缘检测方式"));
            reg(nameof(ImageOperateMethods.CompleteSymm),
                new ScriptParamModel("CompleteMode", ParamType.Enum, "", typeof(ImageHandle.Models.SymmDirection), "折叠方向"));
            reg(nameof(ImageOperateMethods.Blur),
                new ScriptParamModel("ksize", ParamType.Interger, "3", null, "滤波器大小"));
            reg(nameof(ImageOperateMethods.GaussianBlur),
                new ScriptParamModel("ksize", ParamType.Interger, "3", null, "滤波器大小"));
            reg(nameof(ImageOperateMethods.MedianBlur),
                new ScriptParamModel("ksize", ParamType.Interger, "3", null, "滤波器大小"));
            reg(nameof(ImageOperateMethods.ApplyColorMap),
                new ScriptParamModel("colormap", ParamType.Enum, "", typeof(OpenCvSharp.ColormapTypes), "颜色映射方式"));
            reg(nameof(ImageOperateMethods.BoxFilter),
                new ScriptParamModel("ksize", ParamType.Interger, "3", null, "滤波器大小"));
            reg(nameof(ImageOperateMethods.DistanceTranForm),
                new ScriptParamModel("distancetype", ParamType.Enum, "", typeof(OpenCvSharp.DistanceTypes), "距离变换方式"),
                   new ScriptParamModel("maskSize", ParamType.Enum, "", typeof(OpenCvSharp.DistanceTransformMasks), "掩膜大小"));
        }

        private ObservableCollection<string> _methodNames = new ObservableCollection<string>();

        public ObservableCollection<string> MethodNames
        {
            get => _methodNames;
            set
            {
                _methodNames = value;
                OnPropertyChanged();
            }
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

        private bool _isCN = false;

        public bool IsCN
        {
            get => _isCN;
            set
            {
                _isCN = value;
                OnPropertyChanged();
                MethodNames = new ObservableCollection<string>(ScriptService.Instance.GetRegisteredMethodNames(_isCN));
            }
        }
    }
}