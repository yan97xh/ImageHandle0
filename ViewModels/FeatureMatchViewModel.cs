using ImageHandle.Helpers;
using ImageHandle.Models;
using OpenCvSharp;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ImageHandle.ViewModels
{
    public class FeatureMatchViewModel : ViewModelBase
    {
        public FeatureMatchViewModel()
        {
            MatcherModeArr = Enum.GetNames(typeof(MatcherMode));

            FeatureMatchCommand = new Commands.Command<FeatureMatchMode>(ProcessFeatureMatch);
        }

        #region 输入图像

        private string _srcImagePath1;

        public string SrcImagePath1
        {
            get => _srcImagePath1;
            set
            {
                _srcImagePath1 = value;
                OnPropertyChanged();
            }
        }

        private string _srcImagePath2;

        public string SrcImagePath2
        {
            get => _srcImagePath2;
            set
            {
                _srcImagePath2 = value;
                OnPropertyChanged();
            }
        }

        #endregion 输入图像

        #region 输出图像

        private FeatureMatchResModel _featureMatchResModel = new FeatureMatchResModel();

        public FeatureMatchResModel FeatureMatchResModel
        {
            get => _featureMatchResModel;
            set
            {
                _featureMatchResModel = value;
                OnPropertyChanged();
            }
        }

        #endregion 输出图像

        #region 匹配距离参数

        private double _matchDisParam = 10.0;

        public double MatchDisParam
        {
            get => _matchDisParam;
            set
            {
                if (value <= 0)
                {
                    _matchDisParam = 1;
                }
                else
                {
                    _matchDisParam = value;
                }

                OnPropertyChanged();
            }
        }

        #endregion 匹配距离参数

        #region 匹配器类型

        private string _matcherMode;

        public string MatcherMode
        {
            get => _matcherMode;
            set
            {
                _matcherMode = value;
                OnPropertyChanged();
            }
        }

        public string[] MatcherModeArr { get; }

        #endregion 匹配器类型

        public ICommand FeatureMatchCommand { get; }

        private void ProcessFeatureMatch(FeatureMatchMode operation)
        {
            if (string.IsNullOrEmpty(_srcImagePath1))
            {
                MessageBox.Show("未选择输入图像1");
                return;
            }

            if (string.IsNullOrEmpty(_srcImagePath2))
            {
                MessageBox.Show("未选择输入图像2");
                return;
            }

            if (File.Exists(_srcImagePath1) == false)
            {
                MessageBox.Show($"图像:{_srcImagePath1}不存在");
                return;
            }

            if (File.Exists(_srcImagePath2) == false)
            {
                MessageBox.Show($"图像:{_srcImagePath1}不存在");
                return;
            }

            Mat srcImg1 = new Mat(_srcImagePath1);
            Mat srcImg2 = new Mat(_srcImagePath2);

            if (Enum.TryParse(typeof(MatcherMode), _matcherMode, true, out object result) == false)
            {
                MessageBox.Show($"匹配器选择有误");
                return;
            }

            MatcherMode _matcher = (MatcherMode)result;

            switch (operation)
            {
                case FeatureMatchMode.SIFT:
                {
                    FeatureMatchResModel = ImageOperateMethods.SIFTMatch(srcImg1, srcImg2, _matcher, _matchDisParam);
                }
                    break;

                case FeatureMatchMode.SURF:
                {
                    FeatureMatchResModel = ImageOperateMethods.SURFMatch(srcImg1, srcImg2, _matcher, _matchDisParam);
                }
                    break;
            }
        }
    }
}
