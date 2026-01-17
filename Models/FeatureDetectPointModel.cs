using ImageHandle.ViewModels;
using OpenCvSharp;
using OpenCvSharp.Features2D;
using OpenCvSharp.XFeatures2D;

namespace ImageHandle.Models
{
    /// <summary>
    /// 特征点提取模型
    /// </summary>
    public class FeatureDetectPointModel : ViewModelBase
    {
        public FeatureDetectPointModel()
        {
            ItemsArray = Enum.GetNames(typeof(FeatureDetectPointMode));
        }

        private string[] _itemsArray = Array.Empty<string>();

        public string[] ItemsArray
        {
            get => _itemsArray;
            set
            {
                _itemsArray = value;
                OnPropertyChanged();
            }
        }

        private string _inputParam;

        public string InputParam
        {
            get => _inputParam;
            set
            {
                _inputParam = value;
                OnPropertyChanged();
            }
        }

        private int _pointsCount;

        public int PointsCount
        {
            get { return _pointsCount; }
            set
            {
                _pointsCount = value;
                OnPropertyChanged();
            }
        }

        public Mat Excute(Mat srcImg)
        {
            Mat dstImg = new Mat();
            PointsCount = 0;
            Enum.TryParse(InputParam, out FeatureDetectPointMode mode);
            KeyPoint[] keyPoints = new KeyPoint[2]; 
            Feature2D feature2D = mode switch
                                  {
                                      FeatureDetectPointMode.SIFT => SIFT.Create(1000),
                                      FeatureDetectPointMode.SURF => SURF.Create(1000),
                                      FeatureDetectPointMode.Star => StarDetector.Create(),
                                      FeatureDetectPointMode.ORB_FERAK=> ORB.Create(500),
                                      FeatureDetectPointMode.BRISK=> BRISK.Create(),
                                      FeatureDetectPointMode.MSER=> MSER.Create(),
                                      FeatureDetectPointMode.GFTT=> GFTTDetector.Create(500, 0.01, 15, 3, false, 0.04)
                                  };
            keyPoints = feature2D.Detect(srcImg);  
            PointsCount = keyPoints.Count();
            if (PointsCount != 0)
            {
                Random r = new Random();
                for (int i = 0; i < keyPoints.Count(); i++)
                {
                    OpenCvSharp.Point pp = new OpenCvSharp.Point();
                    pp.X = (int)keyPoints[i].Pt.X;
                    pp.Y = (int)keyPoints[i].Pt.Y;
                    Cv2.Circle(srcImg, pp, 3, new OpenCvSharp.Scalar(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)));
                }

                dstImg = srcImg;
                return dstImg;
            }
            else
            {
                return srcImg;
            }
        }
    }
}
