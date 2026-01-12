using Microsoft.Win32;
using OpenCvSharp;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ImageHandle.ViewModels
{
    public class TemplateMatchViewModel : ViewModelBase
    {
        public TemplateMatchViewModel()
        {
            SelectSrcImgCommand = new Commands.RelayCommand(SelectSrcImg);
            SaveImageCommand = new Commands.RelayCommand(SaveImage);
            SelectTemplateImgCommand = new Commands.RelayCommand(SelectTemplateImg);

            TemplateMatchModeArray = Enum.GetNames(typeof(OpenCvSharp.TemplateMatchModes));
            TemplateMatchSingle = new Commands.RelayCommand(ProcessTemplateMatchSingle);
            TemplateMatchMultiCommand = new Commands.RelayCommand(ProcessTemplateMatchMulti);
            TemplateMatchCircleCommand = new Commands.RelayCommand(ProcessTemplateMatchCircle);
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

        private string _saveImagePath;

        public string SaveImagePath
        {
            get => _saveImagePath;
            set
            {
                _saveImagePath = value;
                OnPropertyChanged();
            }
        }

        #endregion 输出图像

        #region 选择输入图像命令

        public ICommand SelectSrcImgCommand { get; }

        private void SelectSrcImg()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "请选择图片";
            openFileDialog.Filter = "所有图片文件(*.jpg;*.bmp;*.jpeg;*.png;*.tif)|*.jpg;*.bmp;*.jpeg;*.png;*.tif";
            // dialog.InitialDirectory = @"E:\myPictures\Lena";

            if (openFileDialog.ShowDialog() == true)
            {
                SrcImagePath = openFileDialog.FileName;
                Mat mat = new Mat(SrcImagePath);
                DstMat = mat;
            }
        }

        #endregion 选择输入图像命令

        #region 保存输出图像命令

        public ICommand SaveImageCommand { get; }

        private void SaveImage()
        {
            if (DstMat == null)
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

                bool success = Cv2.ImWrite(savePath, DstMat);
                if (!success)
                {
                    MessageBox.Show("图像保存失败");
                    return;
                }
                MessageBox.Show("保存成功");
                SaveImagePath = savePath;
            }
        }

        #endregion 保存输出图像命令

        #region 模版图像

        private Mat _templateMat;

        public Mat TemplateMat
        {
            get => _templateMat;
            set
            {
                _templateMat = value;
                OnPropertyChanged();
            }
        }

        #endregion 模版图像

        #region 选择模版图像命令

        public ICommand SelectTemplateImgCommand { get; }

        private void SelectTemplateImg()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "请选择图片";
            openFileDialog.Filter = "所有图片文件(*.jpg;*.bmp;*.jpeg;*.png;*.tif)|*.jpg;*.bmp;*.jpeg;*.png;*.tif";
            // dialog.InitialDirectory = @"E:\myPictures\Lena";

            if (openFileDialog.ShowDialog() == true)
            {
                Mat mat = new Mat(openFileDialog.FileName);
                TemplateMat = mat;
            }
        }

        #endregion 选择模版图像命令

        public string[] TemplateMatchModeArray { get; }

        private string _templateMatchMode;

        public string TemplateMatchMode
        {
            get => _templateMatchMode;
            set
            {
                _templateMatchMode = value;
                OnPropertyChanged();
            }
        }

        private double _multiMatchThreshold = 0.8;

        public double MultiMatchThreshold
        {
            get => _multiMatchThreshold;
            set
            {
                if (value < 0)
                {
                    _multiMatchThreshold = 0;
                }
                else if (value > 1)
                {
                    _multiMatchThreshold = 1;
                }
                else
                {
                    _multiMatchThreshold = value;
                }
                OnPropertyChanged();
            }
        }

        #region 单模版匹配

        public ICommand TemplateMatchSingle { get; }

        private void ProcessTemplateMatchSingle()
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
            if (TemplateMat == null)
            {
                MessageBox.Show("请输入模板图像!");
                return;
            }
            Mat dstImg = srcImg.Clone();
            int rows = srcImg.Rows - TemplateMat.Rows + 1;
            int cols = srcImg.Cols - TemplateMat.Cols + 1;
            if (rows <= 0 || cols <= 0)
            {
                MessageBox.Show("所选模板大于匹配原图!");
                return;
            }
            if (Enum.TryParse(TemplateMatchMode, out TemplateMatchModes mode) == false)
            {
                MessageBox.Show("匹配方法选择有误");
                return;
            }
            Mat res = new Mat(rows, cols, MatType.CV_32FC1);
            Cv2.MatchTemplate(srcImg, TemplateMat, res, mode);
            double minValue = 1000, maxValue = -1;
            OpenCvSharp.Point minLoc = new OpenCvSharp.Point(0, 0);
            OpenCvSharp.Point maxLoc = new OpenCvSharp.Point(0, 0);
            Cv2.MinMaxLoc(res, out minValue, out maxValue, out minLoc, out maxLoc);
            OpenCvSharp.Point point = new OpenCvSharp.Point();
            switch (mode)
            {
                //最小
                case TemplateMatchModes.SqDiff:
                case TemplateMatchModes.SqDiffNormed:
                    point = minLoc;
                    break;
                //最大
                case TemplateMatchModes.CCorr:
                case TemplateMatchModes.CCorrNormed:
                case TemplateMatchModes.CCoeff:
                case TemplateMatchModes.CCoeffNormed:
                    point = maxLoc;
                    break;
            }
            Cv2.Rectangle(dstImg, new OpenCvSharp.Rect(point.X, point.Y, TemplateMat.Width, TemplateMat.Height),
                                             new Scalar(0, 255, 0), 2);
            DstMat = dstImg;
            return;
        }

        #endregion 单模版匹配

        #region 多模版匹配

        public ICommand TemplateMatchMultiCommand { get; }

        private void ProcessTemplateMatchMulti()
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
            if (TemplateMat == null)
            {
                MessageBox.Show("请输入模板图像!");
                return;
            }
            Mat dstImg = srcImg.Clone();
            int rows = srcImg.Rows - TemplateMat.Rows + 1;
            int cols = srcImg.Cols - TemplateMat.Cols + 1;
            if (rows <= 0 || cols <= 0)
            {
                MessageBox.Show("所选模板大于匹配原图!");
                return;
            }
            if (Enum.TryParse(TemplateMatchMode, out TemplateMatchModes mode) == false)
            {
                MessageBox.Show("匹配方法选择有误");
                return;
            }
            Mat res = new Mat(rows, cols, MatType.CV_32FC1);

            Mat gray = new Mat();
            Mat tempGray = new Mat();
            Cv2.CvtColor(srcImg, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(TemplateMat, tempGray, ColorConversionCodes.BGR2GRAY);
            Cv2.MatchTemplate(gray, tempGray, res, mode);
            Cv2.Normalize(res, res, 0, 1, NormTypes.MinMax, res.Depth());

            int tempW = 0, tempH = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    float matchValue = res.Get<float>(i, j);
                    if (matchValue >= MultiMatchThreshold && (Math.Abs(j - tempW) > 10) && (Math.Abs(i - tempH) > 10))
                    {
                        Cv2.Rectangle(dstImg, new OpenCvSharp.Rect(j, i, TemplateMat.Width, TemplateMat.Height), new Scalar(0, 255, 0), 2);
                        tempW = j;
                        tempH = i;
                    }
                }
            }

            DstMat = dstImg;
        }

        #endregion 多模版匹配

        #region 角度模版匹配

        public ICommand TemplateMatchCircleCommand { get; }

        private void ProcessTemplateMatchCircle()
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
            if (TemplateMat == null)
            {
                MessageBox.Show("请输入模板图像!");
                return;
            }
            Mat dstImg = srcImg.Clone();
            int rows = srcImg.Rows - TemplateMat.Rows + 1;
            int cols = srcImg.Cols - TemplateMat.Cols + 1;
            if (rows <= 0 || cols <= 0)
            {
                MessageBox.Show("所选模板大于匹配原图!");
                return;
            }
            if (Enum.TryParse(TemplateMatchMode, out TemplateMatchModes mode) == false)
            {
                MessageBox.Show("匹配方法选择有误");
                return;
            }
            Mat res = new Mat(rows, cols, MatType.CV_32FC1);

            double angleStart = 0;
            double angleRange = 360;
            double angleStep = 1;
            int numLevels = 3;
            double thresScore = 0.7;

            double step = angleRange / ((angleRange / angleStep) / 100);
            double start = angleStart;
            double range = angleRange;

            //定义图片匹配所需要的参数
            int resultCols = srcImg.Cols - TemplateMat.Cols + 1;
            int resultRows = srcImg.Rows - TemplateMat.Cols + 1;
            Mat result = new Mat(resultCols, resultRows, MatType.CV_8U);
            Mat src = new Mat();
            Mat model = new Mat();
            srcImg.CopyTo(src);
            TemplateMat.CopyTo(model);

            //对模板图像和待检测图像分别进行图像金字塔下采样
            for (int i = 0; i < numLevels; i++)
            {
                Cv2.PyrDown(src, src, new OpenCvSharp.Size(src.Cols / 2, src.Rows / 2));
                Cv2.PyrDown(model, model, new OpenCvSharp.Size(model.Cols / 2, model.Rows / 2));
            }

            //在没有旋转的情况下进行第一次匹配
            Cv2.MatchTemplate(src, model, result, mode);
            Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc, new Mat());

            OpenCvSharp.Point location = maxLoc;
            double temp = maxVal;
            double angle = 0;
            Mat newImg;

            ResultPoint resultPoint = new ResultPoint(-1, -1, 0, 0);

            //以最佳匹配点左右十倍角度步长进行循环匹配，直到角度步长小于参数角度步长
            if (mode == TemplateMatchModes.SqDiff || mode == TemplateMatchModes.SqDiffNormed)
            {
                do
                {
                    for (int i = 0; i <= (int)range / step; i++)
                    {
                        newImg = ImgRotate(model, start + step * i);
                        Cv2.MatchTemplate(src, newImg, result, mode);
                        Cv2.MinMaxLoc(result, out double minval, out double maxval, out OpenCvSharp.Point minloc, out OpenCvSharp.Point maxloc, new Mat());
                        if (maxval < temp)
                        {
                            location = maxloc;
                            temp = maxval;
                            angle = start + step * i;
                        }
                    }
                    range = step * 2;
                    start = angle - step;
                    step = step / 10;
                } while (step > angleStep);

                resultPoint = new ResultPoint(location.X * Math.Pow(2, numLevels) + TemplateMat.Width / 2, location.Y * Math.Pow(2, numLevels) + TemplateMat.Height / 2, -angle, temp);
            }
            else
            {
                do
                {
                    for (int i = 0; i <= (int)range / step; i++)
                    {
                        newImg = ImgRotate(model, start + step * i);
                        Cv2.MatchTemplate(src, newImg, result, mode);
                        Cv2.MinMaxLoc(result, out double minval, out double maxval, out OpenCvSharp.Point minloc, out OpenCvSharp.Point maxloc, new Mat());
                        if (maxval > temp)
                        {
                            location = maxloc;
                            temp = maxval;
                            angle = start + step * i;
                        }
                    }
                    range = step * 2;
                    start = angle - step;
                    step = step / 10;
                } while (step > angleStep);
                if (temp > thresScore)
                {
                    resultPoint = new ResultPoint(location.X * Math.Pow(2, numLevels), location.Y * Math.Pow(2, numLevels), -angle, temp);
                }
            }

            OpenCvSharp.Rect rect = new OpenCvSharp.Rect((int)resultPoint.X, (int)resultPoint.Y, TemplateMat.Width, TemplateMat.Height);
            OpenCvSharp.Point[] pts = GetRotatePoints(TemplateMat, rect, -resultPoint.T);
            OpenCvSharp.Point ptStart = new OpenCvSharp.Point(rect.X, rect.Y);
            Cv2.Line(dstImg, pts[0] + ptStart, pts[1] + ptStart, new Scalar(0, 255, 0), 2);
            Cv2.Line(dstImg, pts[1] + ptStart, pts[2] + ptStart, new Scalar(0, 255, 0), 2);
            Cv2.Line(dstImg, pts[2] + ptStart, pts[3] + ptStart, new Scalar(0, 255, 0), 2);
            Cv2.Line(dstImg, pts[3] + ptStart, pts[0] + ptStart, new Scalar(0, 255, 0), 2);

            DstMat = dstImg;
        }

        private Mat ImgRotate(Mat image, double angle)
        {
            Mat newImg = new Mat();
            Point2f pt = new Point2f((float)image.Cols / 2, (float)image.Rows / 2);
            Mat r = Cv2.GetRotationMatrix2D(pt, angle, 1.0);
            Cv2.WarpAffine(image, newImg, r, image.Size());
            return newImg;
        }

        private OpenCvSharp.Point[] GetRotatePoints(Mat img, OpenCvSharp.Rect inRect, double angle)
        {
            OpenCvSharp.Rect rect = inRect;
            OpenCvSharp.Point[] pts = new OpenCvSharp.Point[4];
            Point2f center = new Point2f(img.Width / 2, img.Height / 2);
            Mat M = Cv2.GetRotationMatrix2D(center, angle, 1.0);

            Mat ptMat = Mat.Ones(3, 4, MatType.CV_32FC1);
            ptMat.At<float>(0, 0) = 0;
            ptMat.At<float>(0, 1) = (float)rect.Width - 1;
            ptMat.At<float>(0, 2) = (float)rect.Width - 1;
            ptMat.At<float>(0, 3) = 0;
            ptMat.At<float>(1, 0) = 0;
            ptMat.At<float>(1, 1) = 0;
            ptMat.At<float>(1, 2) = (float)rect.Height - 1;
            ptMat.At<float>(1, 3) = (float)rect.Height - 1;

            M.ConvertTo(M, MatType.CV_32F);

            Mat result = M * ptMat;
            pts[0] = new OpenCvSharp.Point((int)result.At<float>(0, 0), (int)result.At<float>(1, 0));
            pts[1] = new OpenCvSharp.Point((int)result.At<float>(0, 1), (int)result.At<float>(1, 1));
            pts[2] = new OpenCvSharp.Point((int)result.At<float>(0, 2), (int)result.At<float>(1, 2));
            pts[3] = new OpenCvSharp.Point((int)result.At<float>(0, 3), (int)result.At<float>(1, 3));
            return pts;
        }

        #endregion 角度模版匹配
    }

    internal class ResultPoint
    {
        public double X;   // 长度
        public double Y;  // 宽度
        public double T;   // 高度
        public double Score;

        public ResultPoint()
        {
        }

        public ResultPoint(double x, double y, double t, double score)
        {
            X = (int)x;
            Y = (int)y;
            T = t;
            Score = score;
        }
    }
}