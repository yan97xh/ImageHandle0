using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageHandle.Helpers;
using ImageHandle.Models;
using OpenCvSharp;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ImageHandle.ViewModels
{
    public partial class FeatureMatchViewModel : ObservableObject
    {
        public FeatureMatchViewModel()
        {
        }

        [ObservableProperty]
        private string _srcImagePath1;

        [ObservableProperty]
        private string _srcImagePath2;

        [ObservableProperty]
        private FeatureMatchResModel _featureMatchResModel = new FeatureMatchResModel();

        [ObservableProperty]
        private double _matchDisParam = 10.0;

        partial void OnMatchDisParamChanged(double value)
        {
            if (value <= 0 && Math.Abs(_matchDisParam - 1) > 0.0001)
            {
                // 重新赋值为 1
                MatchDisParam = 1;
            }
        }

        [ObservableProperty]
        private string _matcherMode; // 匹配器类型

        [RelayCommand]
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