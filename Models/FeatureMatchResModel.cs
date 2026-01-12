using ImageHandle.ViewModels;
using OpenCvSharp;

namespace ImageHandle.Models
{
    public class FeatureMatchResModel : ViewModelBase
    {
        private Mat _featureMatchDstMat;
        private Mat _featureDstMat1;
        private Mat _featureDstMat2;

        public Mat FeatureMatchDstMat
        {
            get => _featureMatchDstMat;
            set
            {
                _featureMatchDstMat = value;
                OnPropertyChanged();
            }
        }

        public Mat FeatureDstMat1
        {
            get => _featureDstMat1;
            set
            {
                _featureDstMat1 = value;
                OnPropertyChanged();
            }
        }

        public Mat FeatureDstMat2
        {
            get => _featureDstMat2;
            set
            {
                _featureDstMat2 = value;
                OnPropertyChanged();
            }
        }
    }
}