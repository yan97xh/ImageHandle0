using ImageHandle.Attributes;
using ImageHandle.Models;
using ImageHandle.Scripts;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.Features2D;
using OpenCvSharp.Flann;
using OpenCvSharp.XFeatures2D;
using QRCoder;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImageHandle.Helpers
{
    /// <summary>
    /// 图像操作帮助类
    /// </summary>
    public static class ImageOperateMethods
    {
        #region 0参数

        /// <summary>
        /// CvtColor颜色空间转Gray
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("彩图转灰图"), NoneParam]
        public static Mat CvtColorToGray(Mat srcImg)
        {
            Mat dstImg = new Mat();
            Cv2.CvtColor(srcImg, dstImg, ColorConversionCodes.BGR2GRAY);
            return dstImg;
        }

        /// <summary>
        /// CvtColor颜色空间转HSV
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("彩图转HSV"), NoneParam]
        public static Mat CvtColorToHSV(Mat srcImg)
        {
            Mat dstImg = new Mat();
            Cv2.CvtColor(srcImg, dstImg, ColorConversionCodes.BGR2HSV);
            return dstImg;
        }

        /// <summary>
        /// CvtColor颜色空间转Lab
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("彩图转Lab"), NoneParam]
        public static Mat CvtColorToLab(Mat srcImg)
        {
            Mat dstImg = new Mat();
            Cv2.CvtColor(srcImg, dstImg, ColorConversionCodes.BGR2Lab);
            return dstImg;
        }

        /// <summary>
        ///  Cv2.Decolor获得灰度或彩色图像
        /// </summary>
        /// <param name="srcImg">输入图像</param>
        /// <returns></returns>
        [MethodCNName("转化彩图"), NoneParam]
        public static Mat CvtColorToRGB(Mat srcImg)
        {
            srcImg.ConvertTo(srcImg, MatType.CV_8UC3);
            Mat gray = new Mat(); // 灰度图像
            Mat boost = new Mat(); // 彩色图像
            Cv2.Decolor(srcImg, gray, boost);
            return boost;
        }

        /// <summary>
        /// 直方图均衡化
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("直方图均衡化"), NoneParam]
        public static Mat EqualizeHist(Mat srcImg)
        {
            Cv2.CvtColor(srcImg, srcImg, ColorConversionCodes.BGR2GRAY);
            Mat dstImg = new Mat();
            Cv2.EqualizeHist(srcImg, dstImg);
            return dstImg;
        }

        /// <summary>
        /// 使用大津法对灰度图像二值化
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("大津法二值化"), NoneParam]
        public static Mat Otsu(Mat srcImg)
        {
            Cv2.CvtColor(srcImg, srcImg, ColorConversionCodes.BGR2GRAY);
            Mat dstImg = new Mat();
            dstImg = srcImg.Threshold(0, 255, ThresholdTypes.Otsu);
            return dstImg;
        }

        /// <summary>
        /// 灰度图像所有像素取反  pixel = 255 - pixel
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("图像取反"), NoneParam]
        public static Mat Negation(Mat srcImg)
        {
            Cv2.CvtColor(srcImg, srcImg, ColorConversionCodes.BGR2GRAY);
            for (int i = 0; i < srcImg.Rows; i++)
            {
                for (int j = 0; j < srcImg.Cols; j++)
                {
                    // byte sexiOrig = mm.At<byte>(i, j); // 先获取原先位置像素值
                    byte sexiOrig = srcImg.Get<byte>(i, j); // 先获取原先位置像素值
                    byte sexiNow = (byte)(255 - sexiOrig); // 取反
                    srcImg.Set(i, j, sexiNow); // 新的像素值赋进去
                }
            }
            return srcImg;
        }

        #region 图像特效

        /// <summary>
        /// 图像怀旧特效处理
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("怀旧特效"), NoneParam]
        public static Mat RetroEffect(Mat srcImg)
        {
            srcImg.ConvertTo(srcImg, MatType.CV_8UC3);
            Mat dstImg = new Mat(srcImg.Size(), MatType.CV_8UC3);
            Vec3b vec = new Vec3b();
            float bb = 0, bg = 0, br = 0;
            for (int i = 0; i < srcImg.Height; i++)
            {
                for (int j = 0; j < srcImg.Width; j++)
                {
                    vec = srcImg.At<Vec3b>(i, j);
                    bb = vec.Item2 * 0.272f + vec.Item1 * 0.534f + vec.Item0 * 0.131f;
                    bg = vec.Item2 * 0.349f + vec.Item1 * 0.686f + vec.Item0 * 0.168f;
                    br = vec.Item2 * 0.393f + vec.Item1 * 0.769f + vec.Item0 * 0.189f;
                    dstImg.At<Vec3b>(i, j) = new Vec3b(SetCorrectPix(bb), SetCorrectPix(bg), SetCorrectPix(br));
                }
            }
            return dstImg;
            byte SetCorrectPix(float byte1)
            {
                if (byte1 > 255)
                    return 255;
                else if (byte1 < 0)
                    return 0;
                return (byte)byte1;
            }
        }

        /// <summary>
        /// 熔铸特效
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("熔铸特效"), NoneParam]
        public static Mat FusedCastEffect(Mat srcImg)
        {
            srcImg.ConvertTo(srcImg, MatType.CV_8UC3);
            Mat dstImg = new Mat(srcImg.Size(), MatType.CV_8UC3);
            Vec3b vec = new Vec3b();
            float bb = 0, bg = 0, br = 0;
            for (int i = 0; i < srcImg.Height; i++)
            {
                for (int j = 0; j < srcImg.Width; j++)
                {
                    vec = srcImg.At<Vec3b>(i, j);
                    bb = vec.Item0 * 128 / (vec.Item1 + vec.Item2 + 1);
                    bg = vec.Item1 * 128 / (vec.Item0 + vec.Item2 + 1);
                    br = vec.Item2 * 128 / (vec.Item0 + vec.Item1 + 1);
                    dstImg.At<Vec3b>(i, j) = new Vec3b(SetCorrectPix(bb), SetCorrectPix(bg), SetCorrectPix(br));
                }
            }
            return dstImg;
            byte SetCorrectPix(float byte1)
            {
                if (byte1 > 255)
                    return 255;
                else if (byte1 < 0)
                    return 0;
                return (byte)byte1;
            }
        }

        /// <summary>
        /// 冰冻特效
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("冰冻特效"), NoneParam]
        public static Mat FrozenEffect(Mat srcImg)
        {
            srcImg.ConvertTo(srcImg, MatType.CV_8UC3);
            Mat dstImg = new Mat(srcImg.Size(), MatType.CV_8UC3);
            Vec3b vec = new Vec3b();
            float bb = 0, bg = 0, br = 0;
            for (int i = 0; i < srcImg.Height; i++)
            {
                for (int j = 0; j < srcImg.Width; j++)
                {
                    vec = srcImg.At<Vec3b>(i, j);
                    bb = Math.Abs(vec.Item0 - vec.Item1 - vec.Item2) * 3 / 2f;
                    bg = Math.Abs(vec.Item1 - vec.Item0 - vec.Item2) * 3 / 2f;
                    br = Math.Abs(vec.Item2 - vec.Item0 - vec.Item1) * 3 / 2f;
                    dstImg.At<Vec3b>(i, j) = new Vec3b(SetCorrectPix(bb), SetCorrectPix(bg), SetCorrectPix(br));
                }
            }
            return dstImg;
            byte SetCorrectPix(float byte1)
            {
                if (byte1 > 255)
                    return 255;
                else if (byte1 < 0)
                    return 0;
                return (byte)byte1;
            }
        }

        /// <summary>
        /// 连环画特效
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("连环画特效"), NoneParam]
        public static Mat ComicEffect(Mat srcImg)
        {
            srcImg.ConvertTo(srcImg, MatType.CV_8UC3);
            Mat dstImg = new Mat(srcImg.Size(), MatType.CV_8UC3);
            Vec3b vec = new Vec3b();
            float bb = 0, bg = 0, br = 0;
            for (int i = 0; i < srcImg.Height; i++)
            {
                for (int j = 0; j < srcImg.Width; j++)
                {
                    vec = srcImg.At<Vec3b>(i, j);
                    bb = Math.Abs(vec.Item0 - vec.Item1 + vec.Item0 + vec.Item2) * vec.Item1 / 256;
                    bg = Math.Abs(vec.Item0 - vec.Item1 + vec.Item0 + vec.Item2) * vec.Item2 / 256;
                    br = Math.Abs(vec.Item1 - vec.Item0 + vec.Item1 + vec.Item2) * vec.Item2 / 256;
                    dstImg.At<Vec3b>(i, j) = new Vec3b(SetCorrectPix(bb), SetCorrectPix(bg), SetCorrectPix(br));
                }
            }
            return dstImg;
            byte SetCorrectPix(float byte1)
            {
                if (byte1 > 255)
                    return 255;
                else if (byte1 < 0)
                    return 0;
                return (byte)byte1;
            }
        }

        /// <summary>
        /// 流年特效
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="blueValue"></param>
        /// <returns></returns>
        [MethodCNName("流年特效"), NoneParam]
        public static Mat FleetingEffect(Mat srcImg)
        {
            srcImg.ConvertTo(srcImg, MatType.CV_8UC3);
            Mat dstImg = new Mat(srcImg.Size(), MatType.CV_8UC3);
            Vec3b vec = new Vec3b();
            float bb = 0, bg = 0, br = 0;
            for (int i = 0; i < srcImg.Height; i++)
            {
                for (int j = 0; j < srcImg.Width; j++)
                {
                    vec = srcImg.At<Vec3b>(i, j);
                    bb = (float)(Math.Sqrt(vec.Item0) * 14);
                    bg = vec.Item1;
                    br = vec.Item2;
                    dstImg.At<Vec3b>(i, j) = new Vec3b(SetCorrectPix(bb), SetCorrectPix(bg), SetCorrectPix(br));
                }
            }
            return dstImg;
            byte SetCorrectPix(float byte1)
            {
                if (byte1 > 255)
                    return 255;
                else if (byte1 < 0)
                    return 0;
                return (byte)byte1;
            }
        }

        /// <summary>
        /// USM锐化
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("USM锐化"), NoneParam]
        public static Mat USM(Mat srcImg)
        {
            Mat dstImg = srcImg.Clone();
            Mat blurmat = new Mat();
            Cv2.GaussianBlur(srcImg, blurmat, new OpenCvSharp.Size(5, 5), 3);
            Cv2.AddWeighted(srcImg, 1.5, blurmat, -0.5, 0, dstImg);
            return dstImg;
        }

        #endregion 图像特效

        /// <summary>
        /// 完美反射算法  自动白平衡
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("自动白平衡"), NoneParam]
        public static Mat AutoWhithBalance(Mat srcImg)
        {
            Mat dst = new Mat(srcImg.Size(), MatType.CV_8UC3);
            int[] HistRGB = new int[767];
            int MaxVal = 0;
            for (int i = 0; i < srcImg.Rows; i++)
            {
                for (int j = 0; j < srcImg.Cols; j++)
                {
                    MaxVal = Math.Max(MaxVal, (int)srcImg.At<Vec3b>(i, j)[0]);
                    MaxVal = Math.Max(MaxVal, (int)srcImg.At<Vec3b>(i, j)[1]);
                    MaxVal = Math.Max(MaxVal, (int)srcImg.At<Vec3b>(i, j)[2]);
                    int sum1 = srcImg.At<Vec3b>(i, j)[0] + srcImg.At<Vec3b>(i, j)[1] + srcImg.At<Vec3b>(i, j)[2];
                    HistRGB[sum1]++;
                }
            }
            int Threshold = 0;
            int sum = 0;
            for (int i = 766; i >= 0; i--)
            {
                sum += HistRGB[i];
                if (sum > srcImg.Rows * srcImg.Cols * 0.1)
                {
                    Threshold = i;
                    break;
                }
            }
            int AvgB = 0;
            int AvgG = 0;
            int AvgR = 0;
            int cnt = 0;
            for (int i = 0; i < srcImg.Rows; i++)
            {
                for (int j = 0; j < srcImg.Cols; j++)
                {
                    int sumP = srcImg.At<Vec3b>(i, j)[0] + srcImg.At<Vec3b>(i, j)[1] + srcImg.At<Vec3b>(i, j)[2];
                    if (sumP > Threshold)
                    {
                        AvgB += srcImg.At<Vec3b>(i, j)[0];
                        AvgG += srcImg.At<Vec3b>(i, j)[1];
                        AvgR += srcImg.At<Vec3b>(i, j)[2];
                        cnt++;
                    }
                }
            }
            if (cnt == 0)
            {
                return srcImg.Clone();
            }
            AvgB /= cnt;
            AvgG /= cnt;
            AvgR /= cnt;
            for (int i = 0; i < srcImg.Rows; i++)
            {
                for (int j = 0; j < srcImg.Cols; j++)
                {
                    int Blue = srcImg.At<Vec3b>(i, j)[0] * MaxVal / AvgB;
                    int Green = srcImg.At<Vec3b>(i, j)[1] * MaxVal / AvgG;
                    int Red = srcImg.At<Vec3b>(i, j)[2] * MaxVal / AvgR;
                    if (Red > 255)
                    {
                        Red = 255;
                    }
                    else if (Red < 0)
                    {
                        Red = 0;
                    }
                    if (Green > 255)
                    {
                        Green = 255;
                    }
                    else if (Green < 0)
                    {
                        Green = 0;
                    }
                    if (Blue > 255)
                    {
                        Blue = 255;
                    }
                    else if (Blue < 0)
                    {
                        Blue = 0;
                    }
                    dst.At<Vec3b>(i, j)[0] = (byte)Blue;
                    dst.At<Vec3b>(i, j)[1] = (byte)Green;
                    dst.At<Vec3b>(i, j)[2] = (byte)Red;
                }
            }
            return dst;
        }

        /// <summary>
        /// 磨皮
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("磨皮"), NoneParam]
        public static Mat Buffing(Mat srcImg)
        {
            Mat dstImg = new Mat();
            Mat blur_img = new Mat();
            Cv2.BilateralFilter(srcImg, blur_img, 31, 75, 75);
            Mat re = new Mat();
            Cv2.AddWeighted(srcImg, 0.3, blur_img, 0.7, 0, re);
            Cv2.DetailEnhance(re, dstImg, 1.5f, 1.5f);
            return dstImg;
        }

        // 像素亮度增强表
        private static readonly int[] _colorList = new int[256]
         {
            1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 31, 33, 35, 37, 39,
          41, 43, 44, 46, 48, 50, 52, 53, 55, 57, 59, 60, 62, 64, 66, 67, 69, 71, 73, 74,
          76, 78, 79, 81, 83, 84, 86, 87, 89, 91, 92, 94, 95, 97, 99, 100, 102, 103, 105,
         106, 108, 109, 111, 112, 114, 115, 117, 118, 120, 121, 123, 124, 126, 127, 128,
         130, 131, 133, 134, 135, 137, 138, 139, 141, 142, 143, 145, 146, 147, 149, 150,
         151, 153, 154, 155, 156, 158, 159, 160, 161, 162, 164, 165, 166, 167, 168, 170,
         171, 172, 173, 174, 175, 176, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187,
         188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203,
         204, 205, 205, 206, 207, 208, 209, 210, 211, 211, 212, 213, 214, 215, 215, 216,
         217, 218, 219, 219, 220, 221, 222, 222, 223, 224, 224, 225, 226, 226, 227, 228,
         228, 229, 230, 230, 231, 232, 232, 233, 233, 234, 235, 235, 236, 236, 237, 237,
         238, 238, 239, 239, 240, 240, 241, 241, 242, 242, 243, 243, 244, 244, 244, 245,
         245, 246, 246, 246, 247, 247, 248, 248, 248, 249, 249, 249, 250, 250, 250, 250,
         251, 251, 251, 251, 252, 252, 252, 252, 253, 253, 253, 253, 253, 254, 254, 254,
         254, 254, 254, 254, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
         255, 255, 255, 255
         };

        /// <summary>
        /// 美白  将所有像素 按照查询表 递增 实现亮度增强
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("美白"), NoneParam]
        public static Mat Whitening(Mat srcImg)
        {
            Mat blur_img = new Mat();
            Cv2.BilateralFilter(srcImg, blur_img, 19, 75, 75);

            for (int i = 0; i < blur_img.Rows; i++)
            {
                for (int j = 0; j < blur_img.Cols; j++)
                {
                    OpenCvSharp.Vec3b vec3B = blur_img.At<Vec3b>(i, j);
                    OpenCvSharp.Vec3b vec = new Vec3b((byte)_colorList[vec3B.Item0], (byte)_colorList[vec3B.Item1], (byte)_colorList[vec3B.Item2]);
                    blur_img.At<Vec3b>(i, j) = vec;
                }
            }

            Mat dstImg = new Mat();
            Cv2.DetailEnhance(blur_img, dstImg, 1.5f, 1.5f);
            return dstImg;
        }

        /// <summary>
        /// 低照度图像增强     Low illumination image enhancement
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="out_img"></param>
        [MethodCNName("低照度增强"), NoneParam]
        public static Mat LowIlluminationEnhance(Mat srcImg)
        {
            Mat dstImg = srcImg.Clone();
            zxy[,] s = new zxy[2500, 2500];
            double r, g, b;
            double lwmax = -1.0, bs = 0.75;
            for (int i = 0; i < srcImg.Rows; i++)
            {
                for (int j = 0; j < srcImg.Cols; j++)
                {
                    b = (double)srcImg.At<Vec3b>(i, j)[0] / 255.0;
                    g = (double)srcImg.At<Vec3b>(i, j)[1] / 255.0;
                    r = (double)srcImg.At<Vec3b>(i, j)[2] / 255.0;
                    s[i, j].x = (0.4124 * r + 0.3576 * g + 0.1805 * b);
                    s[i, j].y = (0.2126 * r + 0.7152 * g + 0.0722 * b);
                    s[i, j].z = (0.0193 * r + 0.1192 * g + 0.9505 * b);
                    lwmax = Math.Max(lwmax, s[i, j].y);
                }
            }
            for (int i = 0; i < srcImg.Rows; i++)
            {
                for (int j = 0; j < srcImg.Cols; j++)
                {
                    double xx = s[i, j].x / (s[i, j].x + s[i, j].y + s[i, j].z);
                    double yy = s[i, j].y / (s[i, j].x + s[i, j].y + s[i, j].z);
                    double tp = s[i, j].y;
                    //修改CIE:X,Y,Z
                    s[i, j].y = 1.0 * Math.Log(s[i, j].y + 1) / Math.Log(2 + 8.0 * Math.Pow((s[i, j].y / lwmax), Math.Log(bs) / Math.Log(0.5))) / Math.Log10(lwmax + 1);
                    double x = s[i, j].y / yy * xx;
                    double y = s[i, j].y;
                    double z = s[i, j].y / yy * (1 - xx - yy);

                    //转化为用RGB表示
                    r = 3.2410 * x - 1.5374 * y - 0.4986 * z;
                    g = -0.9692 * x + 1.8760 * y + 0.0416 * z;
                    b = 0.0556 * x - 0.2040 * y + 1.0570 * z;

                    if (r < 0) r = 0; if (r > 1) r = 1;
                    if (g < 0) g = 0; if (g > 1) g = 1;
                    if (b < 0) b = 0; if (b > 1) b = 1;

                    //修正补偿
                    r = Transform(r);
                    g = Transform(g);
                    b = Transform(b);
                    dstImg.At<Vec3b>(i, j) = new Vec3b((byte)(b * 255), (byte)(g * 255), (byte)(r * 255));
                }
            }
            return dstImg;
            double Transform(double x)
            {
                if (x <= 0.05) return x * 2.64;
                return 1.099 * Math.Pow(x, 0.9 / 2.2) - 0.099;
            }
        }

        /// <summary>
        /// 图像转置
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("图像转置"), NoneParam]
        public static Mat Transpose(Mat srcImg)
        {
            Mat dstImg = new Mat();
            Cv2.Transpose(srcImg, dstImg);
            return dstImg;
        }

        /// <summary>
        /// 图像平滑
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("图像平滑"), NoneParam]
        public static Mat Smooth(Mat srcImg)
        {
            Mat dstImg = new Mat();
            Mat lab = new Mat();
            Cv2.CvtColor(srcImg, lab, ColorConversionCodes.BGR2Lab);
            Cv2.FastNlMeansDenoisingColored(lab, dstImg);
            Cv2.CvtColor(dstImg, dstImg, ColorConversionCodes.Lab2BGR);
            return dstImg;
        }

        /// <summary>
        /// 小波变换
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("小波变换"), NoneParam]
        public static Mat WaveletTransform(Mat srcImg)
        {
            int height = srcImg.Cols;
            int width = srcImg.Rows;
            int depth = 1; //分解深度
            int depthCount = 1;
            Mat tmp = Mat.Ones(new OpenCvSharp.Size(width, height), MatType.CV_32FC1);
            Mat dstImg = Mat.Ones(new OpenCvSharp.Size(width, height), MatType.CV_32FC1);
            Mat imgTmp = srcImg.Clone();
            imgTmp.ConvertTo(imgTmp, MatType.CV_32FC1);
            while (depthCount <= depth)
            {
                width = srcImg.Rows / depthCount;
                height = srcImg.Cols / depthCount;

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height / 2; j++)
                    {
                        tmp.Set(i, j, (imgTmp.Get<float>(i, 2 * j) + imgTmp.Get<float>(i, 2 * j + 1)) / 2);
                        tmp.Set(i, j + height / 2, (imgTmp.Get<float>(i, 2 * j) - imgTmp.Get<float>(i, 2 * j + 1)) / 2);
                    }
                }

                for (int i = 0; i < width / 2; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        dstImg.Set(i, j, (tmp.Get<float>(2 * i, j) + tmp.Get<float>(2 * i + 1, j)) / 2);
                        dstImg.Set(i + width / 2, j, (tmp.Get<float>(2 * i, j) - tmp.Get<float>(2 * i + 1, j)) / 2);
                    }
                }

                imgTmp = dstImg;
                depthCount++;
            }

            //  imgTmp.Release();
            dstImg.ConvertTo(dstImg, MatType.CV_8U);

            return dstImg;
        }

        /// <summary>
        /// 可分离线性滤波
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("线性滤波"), NoneParam]
        public static Mat SepFilter2D(Mat srcImg)
        {
            Mat dstImg = new Mat();
            Mat src = new Mat(1, 3, MatType.CV_8UC1);
            src.Set<byte>(0, 0, 1);
            src.Set<byte>(0, 1, 1);
            src.Set<byte>(0, 2, 0);
            Cv2.SepFilter2D(srcImg, dstImg, -1, OpenCvSharp.InputArray.Create(src), OpenCvSharp.InputArray.Create(src));
            return dstImg;
        }

        /// <summary>
        /// 仿射变换 重映射
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("重映射"), NoneParam]
        public static Mat Remaping(Mat srcImg)
        {
            Mat xMatImg = new Mat(new OpenCvSharp.Size(srcImg.Cols, srcImg.Rows), MatType.CV_32FC1);
            Mat yMatImg = new Mat(new OpenCvSharp.Size(srcImg.Cols, srcImg.Rows), MatType.CV_32FC1);

            Mat dstImg = new Mat();
            for (int i = 0; i < srcImg.Rows; i++)
            {
                for (int j = 0; j < srcImg.Cols; j++)
                {
                    xMatImg.Set<float>(i, j, (float)j);
                    yMatImg.Set<float>(i, j, (float)(i + 5 * Math.Sin(j / 10.0)));
                }
            }
            Cv2.Remap(srcImg, dstImg, xMatImg, yMatImg, InterpolationFlags.Area);
            return dstImg;
        }

        /// <summary>
        /// 仿射变换 透视变换
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("透视变换"), NoneParam]
        public static Mat Perspect(Mat srcImg)
        {
            int hh = 20;
            Mat dstImg = new Mat();
            Mat M = Cv2.GetPerspectiveTransform(new Point2f[] { new Point2f(0, 0), new Point2f(srcImg.Width, 0), new Point2f(0, srcImg.Height), new Point2f(srcImg.Width, srcImg.Height) },
                new Point2f[] { new Point2f(hh, hh), new Point2f(srcImg.Width - hh, hh), new Point2f(0, srcImg.Height), new Point2f(srcImg.Width, srcImg.Height) });

            Cv2.WarpPerspective(srcImg, dstImg, M, new OpenCvSharp.Size(srcImg.Width, srcImg.Height));
            return dstImg;
        }

        /// <summary>
        /// 仿射变换 剪切
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("剪切变换"), NoneParam]
        public static Mat Shear(Mat srcImg)
        {
            int hh = 30;
            Mat M = Cv2.GetAffineTransform(new Point2f[] { new Point2f(0, 0), new Point2f(0, srcImg.Height), new Point2f(srcImg.Width, srcImg.Height) },
                new Point2f[] { new Point2f(hh, hh), new Point2f(0, srcImg.Height), new Point2f(srcImg.Width, srcImg.Height) });
            Mat dstImg = new Mat();
            Cv2.WarpAffine(srcImg, dstImg, M, new OpenCvSharp.Size(srcImg.Width + hh, srcImg.Height));
            return dstImg;
        }

        /// <summary>
        /// 直线矫正
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("直线矫正"), NoneParam]
        public static Mat LineCorrect(Mat srcImg)
        {
            Mat dstImg = srcImg.Clone();

            Mat gray = new Mat();
            Cv2.CvtColor(srcImg, gray, ColorConversionCodes.BGR2GRAY);

            Cv2.MedianBlur(gray, gray, 3);

            Cv2.Canny(gray, gray, 100, 200);
            Cv2.Threshold(gray, gray, 128, 255, ThresholdTypes.Binary);

            LineSegmentPoint[] lineSegmentPoint = Cv2.HoughLinesP(gray,
                                          0.8, Cv2.PI / 180,
                                          90, 100, 10);

            double angle = 0.0;

            for (int i = 0; i < lineSegmentPoint.Count(); i++)
            {
                OpenCvSharp.Point p1 = lineSegmentPoint[i].P1;
                OpenCvSharp.Point p2 = lineSegmentPoint[i].P2;

                float k = (float)(p1.Y - p2.Y) / (p1.X - p2.X);
                angle += Math.Atan(k) * 180 / Math.PI;
            }

            double avgAngle = angle / lineSegmentPoint.Count();

            OpenCvSharp.Point center = new OpenCvSharp.Point(srcImg.Cols / 2, srcImg.Rows / 2);

            Mat m2 = new Mat();
            m2 = Cv2.GetRotationMatrix2D(center, avgAngle, 1);

            Cv2.WarpAffine(srcImg, dstImg, m2, srcImg.Size(), borderValue: new Scalar(0));

            return dstImg;
        }

        /// <summary>
        /// 灰度图像DCT变换
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("DCT变换"), NoneParam]
        public static Mat DCT(Mat srcImg)
        {
            Mat dstImg = new Mat();
            Cv2.CvtColor(srcImg, srcImg, ColorConversionCodes.BGR2GRAY);
            srcImg.ConvertTo(srcImg, MatType.CV_32F);

            Cv2.Dct(srcImg, dstImg);
            dstImg.ConvertTo(dstImg, MatType.CV_8U);
            return dstImg;
        }

        /// <summary>
        /// 图像轮廓检测
        /// </summary>
        [MethodCNName("轮廓检测"), NoneParam]
        public static Mat Contours(Mat srcImg)
        {
            Mat gray = new Mat();

            Cv2.CvtColor(srcImg, gray, ColorConversionCodes.RGB2GRAY);
            //滤波
            Cv2.Blur(gray, gray, new OpenCvSharp.Size(3, 3));

            //Canny边缘检测
            Mat canny_Image = new Mat();
            Cv2.Canny(gray, canny_Image, 100, 200);

            //获得轮廓
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchly;
            Cv2.FindContours(canny_Image, out contours, out hierarchly, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple, new OpenCvSharp.Point(0, 0));

            //将结果画出并返回结果
            Mat dstImg = Mat.Zeros(canny_Image.Size(), srcImg.Type());
            Random rnd = new Random();
            for (int i = 0; i < contours.Length; i++)
            {
                Scalar color = new Scalar(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
                Cv2.DrawContours(dstImg, contours, i, color, 2, LineTypes.Link8, hierarchly);
            }
            return dstImg;
        }

        /// <summary>
        /// 凸包
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("凸包检测"), NoneParam]
        public static Mat Hull(Mat srcImg)
        {
            Mat dstImg = new Mat();

            Mat gray = new Mat();
            Cv2.CvtColor(srcImg, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(gray, gray, 128, 255, ThresholdTypes.Binary);

            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchly;
            Cv2.FindContours(gray, out contours, out hierarchly, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple, new OpenCvSharp.Point(0, 0));

            //  Cv2.DrawContours(dstImg, contours, -1, new Scalar(0, 0, 255), 2);
            srcImg.CopyTo(dstImg);
            for (int i = 0; i < contours.Length; i++)
            {
                OpenCvSharp.Point[] hull = Cv2.ConvexHull(contours[i]);
                for (int j = 0; j < hull.Length; j++)
                {
                    if (j != hull.Length - 1)
                    {
                        Cv2.Line(dstImg, hull[j], hull[j + 1], new Scalar(0, 255, 0), 3, LineTypes.AntiAlias);
                    }
                    else
                    {
                        Cv2.Line(dstImg, hull[j], hull[0], new Scalar(0, 255, 0), 3, LineTypes.AntiAlias);
                    }
                }
            }
            return dstImg;
        }

        /// <summary>
        /// 图像凸包缺陷检测
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("凸包缺陷检测"), NoneParam]
        public static Mat ConvexityDefects(Mat srcImg)
        {
            Mat dstImg = srcImg.Clone();
            Mat gray = new Mat();

            Cv2.CvtColor(srcImg, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(gray, gray, 100, 255, ThresholdTypes.Binary);

            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(gray, out contours, out hierarchy, RetrievalModes.External,
                            ContourApproximationModes.ApproxNone);
            Cv2.DrawContours(dstImg, contours, -1, new Scalar(0, 255, 0), 2);
            try
            {
                List<int> hulls = new List<int>();
                for (int i = 0; i < contours.Length; i++)
                {
                    InputArray arr = InputArray.Create<OpenCvSharp.Point>(contours[i]);
                    OutputArray outArr = OutputArray.Create(hulls);
                    Cv2.ConvexHull(arr, outArr, true, false);
                    var defects = Cv2.ConvexityDefects(contours[i], hulls);
                    for (int j = 0; j < defects.Length; j++)
                    {
                        OpenCvSharp.Point start = contours[i][defects[j].Item0];
                        OpenCvSharp.Point end = contours[i][defects[j].Item1];
                        OpenCvSharp.Point far = contours[i][defects[j].Item2];

                        Cv2.Line(dstImg, start, end, new Scalar(0, 0, 255), 2);
                        Cv2.Circle(dstImg, start, 6, new Scalar(0, 255, 0), 3);
                        Cv2.Circle(dstImg, end, 6, new Scalar(0, 255, 0), 3);
                        Cv2.Circle(dstImg, far, 6, Scalar.Red, 3);
                    }
                }
            }
            catch (Exception ex)
            {
                dstImg = srcImg.Clone();
            }
            return dstImg;
        }

        #region 人脸检测

        /// <summary>
        /// DNN 加载Caffe 框架训练好的人脸检测模型进行人脸检测
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("DNN_Caffe"), NoneParam]
        public static Mat DNN_Caffe(Mat srcImg)
        {
            Mat dstImg = new Mat();
            int frameHeight = srcImg.Rows;
            int frameWidth = srcImg.Cols;
            // ../Resource/Fonts/
            string prototxt = AppDomain.CurrentDomain.BaseDirectory + "Resource\\DNN\\Caffe\\" + "deploy.prototxt";
            string caffeModel = AppDomain.CurrentDomain.BaseDirectory + "Resource\\DNN\\Caffe\\res10_300x300_ssd_iter_140000_fp16.caffemodel";
            Net faceNet = CvDnn.ReadNetFromCaffe(prototxt, caffeModel);
            Mat blob = CvDnn.BlobFromImage(srcImg, 1.0, new OpenCvSharp.Size(300, 300),
                new Scalar(104, 117, 123), false, false);
            faceNet.SetInput(blob, "data");

            Mat detection = faceNet.Forward("detection_out");

            int numDetections = detection.Size(2);
            int featuresPerDetection = detection.Size(3);

            for (int i = 0; i < numDetections; i++)
            {
                //  float confidence = detectionMat.At<float>(i, 2);
                float confidence = detection.At<float>(0, 0, i, 2);
                if (confidence > 0.4)
                {
                    float left = (detection.Get<float>(0, 0, i, 3) * frameWidth);
                    float top = (detection.Get<float>(0, 0, i, 4) * frameHeight);
                    float right = (detection.Get<float>(0, 0, i, 5) * frameWidth);
                    float bottom = (detection.Get<float>(0, 0, i, 6) * frameHeight);

                    Cv2.Rectangle(srcImg, new OpenCvSharp.Point(left, top), new OpenCvSharp.Point(right, bottom), new Scalar(0, 255, 0), 2,
                        LineTypes.Link4);
                }
            }
            dstImg = srcImg;
            return dstImg;
        }

        /// <summary>
        /// DNN 加载TensorFlow模型进行目标检测
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("DNN_TensorFlow"), NoneParam]
        public static Mat DNN_TensorFlow(Mat srcImg)
        {
            Mat dstImg = new Mat();
            string[] classNames = { "background", "person", "bicycle", "car",
                "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant",
                "street sign", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep" }; //SSD需要加background
            const int blobSize = 300;

            string config = AppDomain.CurrentDomain.BaseDirectory + "Resource\\DNN\\TensorFlow\\mscoco_label_map.pbtxt";
            string modelPath = AppDomain.CurrentDomain.BaseDirectory + "Resource\\DNN\\TensorFlow\\frozen_inference_graph.pb";
            Net net = CvDnn.ReadNetFromTensorflow(modelPath, config);
            Mat blob = CvDnn.BlobFromImage(srcImg, 1.0, new OpenCvSharp.Size(blobSize, blobSize),
                                                                        new Scalar(), false, false);
            net.SetInput(blob);

            Mat output = net.Forward();
            int numDetections = output.Size(2);
            int featuresPerDetection = output.Size(3);
            float confThres = 0.5F;
            try
            {
                for (int i = 0; i < numDetections; i++)
                {
                    float confidence = output.Get<float>(0, 0, i, 2);
                    if (confidence > confThres)
                    {
                        int classIndex = (int)(output.Get<float>(0, 0, i, 1));
                        //Console.WriteLine("classIndex:{0}", classIndex);
                        //Console.WriteLine("confidence:{0}", confidence);
                        int xLeftTop = (int)(output.Get<float>(0, 0, i, 3) * srcImg.Cols);
                        int yLeftTop = (int)(output.Get<float>(0, 0, i, 4) * srcImg.Rows);
                        int xRightBottom = (int)(output.Get<float>(0, 0, i, 5) * srcImg.Cols);
                        int yRightBottom = (int)(output.Get<float>(0, 0, i, 6) * srcImg.Rows);

                        Rect rect = new Rect(xLeftTop, yLeftTop,
                                                              (xRightBottom - xLeftTop), (yRightBottom - yLeftTop));
                        Cv2.Rectangle(srcImg, rect, new Scalar(255, 0, 255), 2);
                        string label = classNames[classIndex];
                        string conf = String.Format("{0:F2}", confidence);
                        string strText = label + ":" + conf;
                        Cv2.PutText(srcImg, label, new OpenCvSharp.Point(xLeftTop, yLeftTop), HersheyFonts.HersheySimplex,
                                            1.5, new Scalar(0, 255, 0), 2);
                    }
                }
                dstImg = srcImg;
                return dstImg;
            }
            catch (Exception ex)
            {
                return srcImg;
            }
        }

        #endregion 人脸检测

        #endregion 0参数

        #region 1参数

        /// <summary>
        /// 图像翻转
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="mode">X, Y ,XY</param>
        /// <returns></returns>
        [MethodCNName("图像翻转"), ScriptParam("mode", ParamType.Enum, "", "翻转模式")]
        [EnumParam("方向")]
        public static Mat Flip(Mat srcImg, FlipMode mode)
        {
            Mat dstImg = new Mat();
            Cv2.Flip(srcImg, dstImg, mode);
            return dstImg;
        }

        /// <summary>
        /// 图像素描
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="meanV"></param>
        /// <returns></returns>
        [MethodCNName("图像素描")]
        [NumberParam("像素值", NumberParamTypeEnum.Integer, "Int类型", "128")]
        [ScriptParam("meanV", ParamType.Interger, "128", "像素值")]
        public static Mat Sketch(Mat srcImg, int meanV = 127)
        {
            Mat gray = new Mat();
            Cv2.CvtColor(srcImg, gray, ColorConversionCodes.BGR2GRAY);

            Mat negGray = new Mat();
            Cv2.BitwiseNot(gray, negGray);

            Mat blurMat = new Mat();
            Cv2.GaussianBlur(negGray, blurMat, new OpenCvSharp.Size(3, 3), 0);

            Mat negBlurMat = new Mat();
            Cv2.BitwiseNot(blurMat, negBlurMat);

            Mat dstImg = new Mat();
            Cv2.Divide(gray, negBlurMat, dstImg, meanV);
            return dstImg;
        }

        /// <summary>
        /// 浮雕
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="kvalue"></param>
        /// <returns></returns>
        [MethodCNName("图像浮雕")]
        [NumberParam("像素值", NumberParamTypeEnum.Integer, "Int类型", "128")]
        [ScriptParam("kvalue", ParamType.Interger, "128", "像素值")]
        public static Mat Emboss(Mat srcImg, int kvalue = 150)
        {
            Mat dstImg = Mat.Zeros(srcImg.Size(), MatType.CV_8UC1);

            Mat grayMat = new Mat();
            Cv2.CvtColor(srcImg, grayMat, ColorConversionCodes.BGR2GRAY);
            int bb = 0;
            for (int i = 0; i < srcImg.Height; i++)
            {
                for (int j = 0; j < srcImg.Width - 1; j++)
                {
                    bb = grayMat.At<byte>(i, j) - grayMat.At<byte>(i, j + 1) + kvalue;
                    if (bb > 255)
                        bb = 255;
                    else if (bb < 0)
                        bb = 0;
                    dstImg.At<byte>(i, j) = (byte)bb;
                }
            }
            return dstImg;
        }

        /// <summary>
        /// 快速去雾
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="aplha"></param>
        [MethodCNName("快速去雾")]
        [NumberParam("去雾因子", NumberParamTypeEnum.Double, "double类型", "0.85")]
        [ScriptParam("aplha", ParamType.Double, "0.85", "去雾因子")]
        public static Mat FastDehazing(Mat srcImg, double aplha)
        {
            Mat dstImg = srcImg.Clone();

            Mat M_x = GetMinValue(srcImg);

            Mat M_avgx = new Mat();
            Cv2.Blur(M_x, M_avgx, new OpenCvSharp.Size(3, 3));

            //  double m_av = GetAvegValue(M_x);

            Mat L_x = Mat.Zeros(srcImg.Size(), MatType.CV_8UC1);

            for (int i = 0; i < srcImg.Rows; i++)
            {
                for (int j = 0; j < srcImg.Cols; j++)
                {
                    byte b1 = SetCorrectPix(Math.Min(aplha, 0.9) * M_avgx.At<byte>(i, j));
                    L_x.At<byte>(i, j) = Math.Min(b1, M_x.At<byte>(i, j));
                }
            }

            double A = (GetMaxValue(srcImg) + GetMaxValue(M_avgx)) / 2.0;
            int tt;
            double vb, vg, vr;
            for (int i = 0; i < srcImg.Rows; i++)
            {
                for (int j = 0; j < srcImg.Cols; j++)
                {
                    tt = L_x.At<byte>(i, j);
                    vb = A * (srcImg.At<Vec3b>(i, j).Item0 - tt) / (A - tt);
                    vg = A * (srcImg.At<Vec3b>(i, j).Item1 - tt) / (A - tt);
                    vr = A * (srcImg.At<Vec3b>(i, j).Item2 - tt) / (A - tt);
                    dstImg.At<Vec3b>(i, j) = new Vec3b(SetCorrectPix(vb), SetCorrectPix(vg), SetCorrectPix(vr));
                }
            }
            return dstImg;

            byte SetCorrectPix(double byte1)
            {
                if (byte1 > 255)
                    return 255;
                else if (byte1 < 0)
                    return 0;
                return (byte)byte1;
            }
        }

        /// <summary>
        /// 获取 BGR图像 每个位置最小通道的图像
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        private static Mat GetMinValue(Mat srcImg)
        {
            Mat dstImg = new Mat(srcImg.Size(), MatType.CV_8UC1);

            srcImg.ConvertTo(srcImg, MatType.CV_8UC3);

            for (int i = 0; i < srcImg.Rows; i++)
            {
                for (int j = 0; j < srcImg.Cols; j++)
                {
                    Vec3b vec3B = srcImg.At<Vec3b>(i, j);
                    dstImg.At<byte>(i, j) = Math.Min(Math.Min(vec3B.Item0, vec3B.Item1), vec3B.Item2);
                }
            }
            return dstImg;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        private static double GetMaxValue(Mat srcImg)
        {
            srcImg.ConvertTo(srcImg, MatType.CV_8UC1);
            Cv2.MinMaxIdx(srcImg, out double min, out double max);
            return max;
        }

        /// <summary>
        /// 毛玻璃特效
        /// 毛玻璃效果的实现通过用像素点邻域内随机一个像素点的颜色替代当前像素点的颜色实现
        /// 注意处理邻域大小与边界问题
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="ksize">毛玻璃大小  2n+1 </param>
        /// <returns></returns>
        [MethodCNName("毛玻璃特效")]
        [NumberParam("毛玻璃大小", NumberParamTypeEnum.Integer, "int类型，，奇数，大于0", "5")]
        [ScriptParam("ksize", ParamType.Interger, "5", "毛玻璃大小")]
        public static Mat GroundGlass(Mat srcImg, int ksize = 5)
        {
            Mat dstImg = srcImg.Clone();
            if (ksize < 3)
                return dstImg;
            if (ksize % 2 != 1)
                return dstImg;
            int a = ksize / 2;
            Random r = new Random((int)DateTime.Now.Ticks);
            int x = 0, y = 0;
            // 将图像划分成九个区域
            /*
            // 左上角 -- 上边 -- 右上角
            //   |        |        |
            //  左边  -- 中间 -- 右边
            //   |        |        |
            // 左下角 -- 下边 -- 右下角
            */
            // 每个区域进行分块讨论
            // 左上角
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < a; j++)
                {
                    x = r.Next(0, a);
                    y = r.Next(0, a);
                    dstImg.At<Vec3b>(i, j) = srcImg.At<Vec3b>(x, y);
                }
            }
            // 上边
            for (int i = 0; i < a; i++)
            {
                for (int j = a; j < srcImg.Cols - a; j++)
                {
                    x = r.Next(0, a);
                    y = r.Next(-a, a);
                    dstImg.At<Vec3b>(i, j) = srcImg.At<Vec3b>(x, y + j);
                }
            }
            // 右上角
            for (int i = 0; i < a; i++)
            {
                for (int j = srcImg.Cols - a; j < srcImg.Cols; j++)
                {
                    x = r.Next(0, a);
                    y = r.Next(srcImg.Cols - a, srcImg.Cols);
                    dstImg.At<Vec3b>(i, j) = srcImg.At<Vec3b>(x, y);
                }
            }
            // 左边
            for (int i = a; i < srcImg.Rows - a; i++)
            {
                for (int j = 0; j < a; j++)
                {
                    x = r.Next(-a, a);
                    y = r.Next(0, a);
                    dstImg.At<Vec3b>(i, j) = srcImg.At<Vec3b>(x + i, y);
                }
            }
            // 中间
            for (int i = a; i < srcImg.Rows - a; i++)
            {
                for (int j = a; j < srcImg.Cols - a; j++)
                {
                    x = r.Next(-a, a);
                    y = r.Next(-a, a);
                    dstImg.At<Vec3b>(i, j) = srcImg.At<Vec3b>(x + i, y + j);
                }
            }
            // 右边
            for (int i = a; i < srcImg.Rows - a; i++)
            {
                for (int j = srcImg.Cols - a; j < srcImg.Cols; j++)
                {
                    x = r.Next(-a, a);
                    y = r.Next(srcImg.Cols - a, srcImg.Cols);
                    dstImg.At<Vec3b>(i, j) = srcImg.At<Vec3b>(x + i, y);
                }
            }
            // 左下角
            for (int i = srcImg.Rows - a; i < srcImg.Rows; i++)
            {
                for (int j = srcImg.Cols - a; j < srcImg.Cols; j++)
                {
                    x = r.Next(srcImg.Rows - a, srcImg.Rows);
                    y = r.Next(0, a);
                    dstImg.At<Vec3b>(i, j) = srcImg.At<Vec3b>(x, y);
                }
            }
            // 下边
            for (int i = srcImg.Rows - a; i < srcImg.Rows; i++)
            {
                for (int j = a; j < srcImg.Cols - a; j++)
                {
                    x = r.Next(srcImg.Rows - a, srcImg.Rows);
                    y = r.Next(-a, a);
                    dstImg.At<Vec3b>(i, j) = srcImg.At<Vec3b>(x, y + j);
                }
            }
            // 右下角
            for (int i = srcImg.Rows - a; i < srcImg.Rows; i++)
            {
                for (int j = srcImg.Cols - a; j < srcImg.Cols; j++)
                {
                    x = r.Next(srcImg.Rows - a, srcImg.Rows);
                    y = r.Next(srcImg.Cols - a, srcImg.Cols);
                    dstImg.At<Vec3b>(i, j) = srcImg.At<Vec3b>(x, y);
                }
            }
            return dstImg;
        }

        /// <summary>
        /// 图像透明度更改 即将 BGR 图像转化成 BGRA 图像
        /// 注意保存 BGRA 图像的时候 要选择支持 BGRA 的格式 如 .png
        /// 把图像拆分成三个通道 再添加一个通道重新组合成新的图像
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="lu"></param>
        /// <returns></returns>
        [MethodCNName("图像透明化")]
        [NumberParam("透明度", NumberParamTypeEnum.Double, "double类型", "1")]
        [ScriptParam("lu", ParamType.Double, "1", "透明度")]
        public static Mat Lucency(Mat srcImg, double lu)
        {
            Mat dstImg = new Mat();

            srcImg.ConvertTo(srcImg, MatType.CV_8UC3);
            Mat[] mms = srcImg.Split();
            double k;
            if (lu == 0)
            {
                k = 1;
            }
            else if (lu == 1)
            {
                k = 255;
            }
            else
            {
                k = 256 * lu;
            }

            Mat s = Mat.Ones(mms[0].Size(), mms[0].Type()) * k;

            Mat[] mms1 = { mms[0], mms[1], mms[2], s };
            Cv2.Merge(mms1, dstImg);
            return dstImg;
        }

        /// <summary>
        /// 图像伪颜色加强
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        [MethodCNName("伪颜色增强")]
        [EnumParam("方式")]
        [ScriptParam("types", ParamType.Enum, "", "方式")]
        public static Mat ApplyColorMap(Mat srcImg, OpenCvSharp.ColormapTypes types)
        {
            Mat dstImg = new Mat();
            Cv2.ApplyColorMap(srcImg, dstImg, types);
            return dstImg;
        }

        /// <summary>
        /// 图像添加指定数量的盐噪声点
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        [MethodCNName("加盐噪声")]
        [NumberParam("噪声点数", NumberParamTypeEnum.Integer, "int类型", "2500")]
        [ScriptParam("num", ParamType.Interger, "2500", "噪声点数")]
        public static Mat AddSaltNosie(Mat srcImg, int num)
        {
            Mat dstImg = srcImg.Clone();
            Random r = new Random();
            for (int i = 0; i < num; i++)
            {
                int x = r.Next(srcImg.Rows);
                int y = r.Next(srcImg.Cols);
                dstImg.Set(x, y, new OpenCvSharp.Vec3b(255, 255, 255));
            }
            return dstImg;
        }

        /// <summary>
        /// 图像添加指定数量的椒噪声点
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="num"></param>
        /// <returns></returns>、
        [MethodCNName("加椒噪声")]
        [NumberParam("噪声点数", NumberParamTypeEnum.Integer, "int类型", "2500")]
        [ScriptParam("num", ParamType.Interger, "2500", "噪声点数")]
        public static Mat AddPepperNoise(Mat srcImg, int num)
        {
            Mat dstImg = srcImg.Clone();
            Random r = new Random();
            for (int i = 0; i < num; i++)
            {
                int x = r.Next(srcImg.Rows);
                int y = r.Next(srcImg.Cols);
                dstImg.Set(x, y, new OpenCvSharp.Vec3b(0, 0, 0));
            }
            return dstImg;
        }

        /// <summary>
        /// 图像添加指定数量的椒盐噪声点
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        [MethodCNName("加椒盐噪声")]
        [NumberParam("噪声点数", NumberParamTypeEnum.Integer, "int类型", "2500")]
        [ScriptParam("num", ParamType.Interger, "2500", "噪声点数")]
        public static Mat AddSaltPepperNoise(Mat srcImg, int num)
        {
            OpenCvSharp.Vec3b[] SaltPepperNoises = new OpenCvSharp.Vec3b[2]
                                                    {  new OpenCvSharp.Vec3b(0, 0, 0), // 椒噪声
                                                       new OpenCvSharp.Vec3b(255, 255, 255) // 盐噪声
                                                    };
            Mat dstImg = srcImg.Clone();
            Random r = new Random();
            for (int i = 0; i < num; i++)
            {
                int x = r.Next(srcImg.Rows);
                int y = r.Next(srcImg.Cols);
                int index = r.Next(0, 2);
                Vec3b nosie = SaltPepperNoises[index];
                dstImg.Set(x, y, nosie);
            }
            return dstImg;
        }

        /// <summary>
        /// 图像添加指定数量的高斯噪声点
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        [MethodCNName("加高斯噪声")]
        [NumberParam("噪声点数", NumberParamTypeEnum.Integer, "int类型", "2500")]
        [ScriptParam("num", ParamType.Interger, "2500", "噪声点数")]
        public static Mat AddGaussianNoise(Mat srcImg, int num)
        {
            Mat dstImg = srcImg.Clone();
            Random r = new Random();
            for (int i = 0; i < num; i++)
            {
                int x = r.Next(srcImg.Rows);
                int y = r.Next(srcImg.Cols);
                double gtnum = GaussNiose() * 128;
                if (gtnum < 0)
                    gtnum = 0;
                if (gtnum > 255)
                    gtnum = 255;
                byte nn = Convert.ToByte(gtnum);
                Vec3b nosie = new Vec3b(nn, nn, nn);
                dstImg.Set(x, y, nosie);
            }
            return dstImg;
        }

        private static double GaussNiose()//用box muller的方法产生均值为0，方差为1的正太分布随机数
        {
            Random ran = new Random((int)DateTime.Now.Ticks);
            double r1 = ran.NextDouble();
            double r2 = ran.NextDouble();
            double result = Math.Sqrt((-2) * Math.Log(r2)) * Math.Sin(2 * Math.PI * r1);
            return result;//返回随机数
        }

        /// <summary>
        /// 均值滤波
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="ksize"></param>
        /// <returns></returns>
        [MethodCNName("均值滤波")]
        [NumberParam("滤波器大小", NumberParamTypeEnum.Integer, "int类型,奇数", "3")]
        [ScriptParam("ksize", ParamType.Interger, "3", "滤波器大小")]
        public static Mat Blur(Mat srcImg, int ksize = 3)
        {
            Mat dstImg = new Mat();
            OpenCvSharp.Size opSize = new OpenCvSharp.Size(ksize, ksize);
            Cv2.Blur(srcImg, dstImg, opSize);
            return dstImg;
        }

        /// <summary>
        /// 中值滤波
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="ksize"></param>
        /// <returns></returns>
        [MethodCNName("中值滤波")]
        [NumberParam("滤波器大小", NumberParamTypeEnum.Integer, "int类型,奇数", "3")]
        [ScriptParam("ksize", ParamType.Interger, "3", "滤波器大小")]
        public static Mat MedianBlur(Mat srcImg, int ksize = 3)
        {
            Mat dstImg = new Mat();
            Cv2.MedianBlur(srcImg, dstImg, ksize);
            return dstImg;
        }

        /// <summary>
        /// 双边滤波
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="ksize"></param>
        /// <returns></returns>
        [MethodCNName("双边滤波")]
        [NumberParam("滤波器大小", NumberParamTypeEnum.Integer, "int类型,奇数", "3")]
        [ScriptParam("ksize", ParamType.Interger, "3", "滤波器大小")]
        public static Mat BilateralFilter(Mat srcImg, int ksize = 3)
        {
            Mat dstImg = new Mat();
            Cv2.BilateralFilter(srcImg, dstImg, ksize, 128, 128);
            return dstImg;
        }

        /// <summary>
        /// 高斯滤波
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="ksize"></param>
        /// <returns></returns>
        [MethodCNName("高斯滤波")]
        [NumberParam("滤波器大小", NumberParamTypeEnum.Integer, "int类型,奇数", "3")]
        [ScriptParam("ksize", ParamType.Interger, "3", "滤波器大小")]
        public static Mat GaussianBlur(Mat srcImg, int ksize = 3)
        {
            Mat dstImg = new Mat();
            OpenCvSharp.Size opSize = new OpenCvSharp.Size(ksize, ksize);

            Cv2.GaussianBlur(srcImg, dstImg, opSize, 0);
            return dstImg;
        }

        /// <summary>
        /// 方盒滤波
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="ksize"></param>
        /// <returns></returns>
        [MethodCNName("方盒滤波")]
        [NumberParam("滤波器大小", NumberParamTypeEnum.Integer, "int类型,奇数", "3")]
        [ScriptParam("ksize", ParamType.Interger, "3", "滤波器大小")]
        public static Mat BoxFilter(Mat srcImg, int ksize = 3)
        {
            Mat dstImg = new Mat();
            OpenCvSharp.Size opSize = new OpenCvSharp.Size(ksize, ksize);

            Cv2.BoxFilter(srcImg, dstImg, MatType.CV_8U, opSize);
            return dstImg;
        }

        /// <summary>
        ///  金字塔向下取样
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        [MethodCNName("金字塔向下取样")]
        [NumberParam("取样次数", NumberParamTypeEnum.Integer, "int类型,不宜太大", "1")]
        [ScriptParam("times", ParamType.Interger, "1", "取样次数")]
        public static Mat PyrDown(Mat srcImg, int times)
        {
            Mat tempMat = srcImg.Clone();
            Mat dstImg = Mat.Zeros(srcImg.Size(), srcImg.Type());
            try
            {
                for (int i = 0; i < times; i++)
                {
                    Cv2.PyrDown(tempMat, dstImg);
                    tempMat = dstImg;
                }
            }
            catch (Exception ex)
            {
                dstImg = Mat.Zeros(srcImg.Size(), srcImg.Type());
            }
            return dstImg;
        }

        /// <summary>
        ///  金字塔向上取样
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        [MethodCNName("金字塔向上取样")]
        [NumberParam("取样次数", NumberParamTypeEnum.Integer, "int类型,不宜太大", "1")]
        [ScriptParam("times", ParamType.Interger, "1", "取样次数")]
        public static Mat PyrUp(Mat srcImg, int times)
        {
            Mat tempMat = srcImg.Clone();
            Mat dstImg = Mat.Zeros(srcImg.Size(), srcImg.Type());
            try
            {
                for (int i = 0; i < times; i++)
                {
                    Cv2.PyrUp(tempMat, dstImg);
                    tempMat = dstImg;
                }
            }
            catch (Exception ex)
            {
                dstImg = Mat.Zeros(srcImg.Size(), srcImg.Type());
            }
            return dstImg;
        }

        /// <summary>
        /// 外接矩形
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="minArea"></param>
        /// <returns></returns>
        [MethodCNName("外接矩形")]
        [NumberParam("最小面积", NumberParamTypeEnum.Double, "double类型", "300")]
        [ScriptParam("minArea", ParamType.Double, "300", "最小面积")]
        public static Mat BoundingRect(Mat srcImg, double minArea = 300)
        {
            Mat dstImg = new Mat();
            Mat gray = new Mat();

            Cv2.CvtColor(srcImg, gray, ColorConversionCodes.BGR2GRAY);

            Cv2.Threshold(gray, gray, 100, 255, ThresholdTypes.Binary);

            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchly;
            Cv2.FindContours(gray, out contours, out hierarchly, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple, new OpenCvSharp.Point(0, 0));

            srcImg.CopyTo(dstImg);
            for (int i = 0; i < contours.Length; i++)
            {
                double area = Cv2.ContourArea(contours[i]);
                if (area < minArea)
                    continue;
                OpenCvSharp.Rect rect = Cv2.BoundingRect(contours[i]);
                //if (rect.Width > 200 || rect.Height > 200 || rect.Width < 10 || rect.Height < 10)
                //    continue;
                Cv2.Rectangle(dstImg, rect, new Scalar(0, 255, 0), 2);
            }

            return dstImg;
        }

        /// <summary>
        /// 外接圆
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="minArea"></param>
        /// <returns></returns>
        [MethodCNName("外接圆")]
        [NumberParam("最小面积", NumberParamTypeEnum.Double, "double类型", "300")]
        [ScriptParam("minArea", ParamType.Double, "300", "最小面积")]
        public static Mat BoundingCircle(Mat srcImg, double minArea = 300)
        {
            Mat dstImg = new Mat();
            Mat gray = new Mat();

            Cv2.CvtColor(srcImg, gray, ColorConversionCodes.BGR2GRAY);

            Cv2.Threshold(gray, gray, 100, 255, ThresholdTypes.Binary);

            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchly;
            Cv2.FindContours(gray, out contours, out hierarchly, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple, new OpenCvSharp.Point(0, 0));

            srcImg.CopyTo(dstImg);
            for (int i = 0; i < contours.Length; i++)
            {
                double area = Cv2.ContourArea(contours[i]);
                if (area < minArea)
                    continue;
                float radious;
                OpenCvSharp.Point2f center;
                Cv2.MinEnclosingCircle(contours[i], out center, out radious);
                Cv2.Circle(dstImg, new OpenCvSharp.Point(center.X, center.Y), (int)radious, new Scalar(0, 255, 0), 1, LineTypes.AntiAlias);
            }
            return dstImg;
        }

        #region 边缘检测

        /// <summary>
        /// 图像边缘检测
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [MethodCNName("边缘检测")]
        [EnumParam("检测算法")]
        [ScriptParam("type", ParamType.Enum, "", "检测算法")]
        public static Mat Edge(Mat srcImg, EdegTypeEnum type)
        {
            Mat dstImg = new Mat();
            Mat gray = new Mat();
            Cv2.CvtColor(srcImg, gray, ColorConversionCodes.RGB2GRAY);
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(3, 3), 0, 0);

            switch (type)
            {
                case EdegTypeEnum.Canny:
                    dstImg = EdgeCanny(gray);
                    break;

                case EdegTypeEnum.Sobel:
                    dstImg = EdgeSobel(gray);
                    break;

                case EdegTypeEnum.Sobel_X:
                    dstImg = EdgeSobelX(gray);
                    break;

                case EdegTypeEnum.Sobel_Y:
                    dstImg = EdgeSobelY(gray);
                    break;

                case EdegTypeEnum.Laplacian:
                    dstImg = EdgeLaplacian(gray);
                    break;

                case EdegTypeEnum.Scharr:
                    dstImg = EdgeScharr(gray);
                    break;

                case EdegTypeEnum.Scharr_X:
                    dstImg = EdgeScharrX(gray);
                    break;

                case EdegTypeEnum.Scharr_Y:
                    dstImg = EdgeScharrY(gray);
                    break;

                default:
                    break;
            }

            return dstImg;
        }

        private static Mat EdgeCanny(Mat srcImg, double firstThreshold = 100, double secondThreshold = 200)
        {
            Mat dstImg = new Mat();
            Cv2.Canny(srcImg, dstImg, firstThreshold, secondThreshold);
            return dstImg;
        }

        private static Mat EdgeSobel(Mat srcImg, int ksize = 3)
        {
            Mat dstImg = new Mat();
            Mat grad_x = new Mat();
            Mat grad_y = new Mat();
            Mat abs_x = new Mat();
            Mat abs_y = new Mat();

            Cv2.Sobel(srcImg, grad_x, MatType.CV_16S, 0, 1, ksize);
            Cv2.ConvertScaleAbs(grad_x, abs_x, 1, 0);
            Cv2.Sobel(srcImg, grad_y, MatType.CV_16S, 1, 0, ksize);
            Cv2.ConvertScaleAbs(grad_y, abs_y, 1, 0);
            Cv2.AddWeighted(abs_x, 0.5, abs_y, 0.5, 0, dstImg);

            return dstImg;
        }

        private static Mat EdgeSobelX(Mat srcImg, int ksize = 3)
        {
            Mat dstImg = new Mat();
            Mat grad_x = new Mat();
            Mat abs_x = new Mat();

            Cv2.Sobel(srcImg, grad_x, MatType.CV_16S, 0, 1, ksize);
            Cv2.ConvertScaleAbs(grad_x, abs_x, 1, 0);
            dstImg = abs_x;
            return dstImg;
        }

        private static Mat EdgeSobelY(Mat srcImg, int ksize = 3)
        {
            Mat dstImg = new Mat();
            Mat grad_y = new Mat();
            Mat abs_y = new Mat();

            Cv2.Sobel(srcImg, grad_y, MatType.CV_16S, 1, 0, ksize);
            Cv2.ConvertScaleAbs(grad_y, abs_y, 1, 0);
            dstImg = abs_y;
            return dstImg;
        }

        private static Mat EdgeLaplacian(Mat srcImg, int ksize = 3)
        {
            Mat dstImg = new Mat();
            Cv2.Laplacian(srcImg, dstImg, MatType.CV_8U, ksize);
            return dstImg;
        }

        private static Mat EdgeScharr(Mat srcImg)
        {
            Mat dstImg = new Mat();

            Mat grad_x = new Mat();
            Mat grad_y = new Mat();
            Mat abs_x = new Mat();
            Mat abs_y = new Mat();

            Cv2.Scharr(srcImg, grad_x, MatType.CV_16S, 0, 1);
            Cv2.ConvertScaleAbs(grad_x, abs_x, 1, 0);
            Cv2.Scharr(srcImg, grad_y, MatType.CV_16S, 1, 0);
            Cv2.ConvertScaleAbs(grad_y, abs_y, 1, 0);
            Cv2.AddWeighted(abs_x, 0.5, abs_y, 0.5, 0, dstImg);

            return dstImg;
        }

        private static Mat EdgeScharrX(Mat srcImg)
        {
            Mat dstImg = new Mat();

            Mat grad_x = new Mat();
            Mat abs_x = new Mat();

            Cv2.Scharr(srcImg, grad_x, MatType.CV_16S, 0, 1);
            Cv2.ConvertScaleAbs(grad_x, abs_x, 1, 0);
            dstImg = abs_x;

            return dstImg;
        }

        private static Mat EdgeScharrY(Mat srcImg)
        {
            Mat dstImg = new Mat();

            Mat grad_y = new Mat();
            Mat abs_y = new Mat();

            Cv2.Scharr(srcImg, grad_y, MatType.CV_16S, 1, 0);
            Cv2.ConvertScaleAbs(grad_y, abs_y, 1, 0);
            dstImg = abs_y;
            return dstImg;
        }

        #endregion 边缘检测

        /// <summary>
        /// 特征检测 根据不同分类器
        /// </summary>
        [MethodCNName("特征检测")]
        [EnumParam("分类器")]
        [ScriptParam("classifierEnum", ParamType.Enum, "", "分类器")]
        public static Mat FeatureRecogn(Mat srcImg, ClassifierEnum classifierEnum)
        {
            Mat dstImg = new Mat();
            string xmlFilePath = "";
            string classifierPath = "";
            switch (classifierEnum)
            {
                case ClassifierEnum.Haarcascades_ALL:
                    {
                        xmlFilePath = "haarcascade_frontalface_alt.xml";
                        classifierPath = AppDomain.CurrentDomain.BaseDirectory + "Resource\\xml\\haarcascades\\" + xmlFilePath;
                    }
                    break;

                case ClassifierEnum.Haarcascades_EyeGlass:
                    {
                        xmlFilePath = "haarcascade_eye_tree_eyeglasses.xml";
                        classifierPath = AppDomain.CurrentDomain.BaseDirectory + "Resource\\xml\\haarcascades\\" + xmlFilePath;
                    }
                    break;

                case ClassifierEnum.Haarcascades_Nose:
                    {
                        xmlFilePath = "haarcascade_mcs_nose.xml";
                        classifierPath = AppDomain.CurrentDomain.BaseDirectory + "Resource\\xml\\haarcascades\\" + xmlFilePath;
                    }
                    break;

                case ClassifierEnum.Haarcascades_Mouth:
                    {
                        xmlFilePath = "haarcascade_mcs_mouth.xml";
                        classifierPath = AppDomain.CurrentDomain.BaseDirectory + "Resource\\xml\\haarcascades\\" + xmlFilePath;
                    }
                    break;

                case ClassifierEnum.Lbpcascade_Face:
                    {
                        xmlFilePath = "lbpcascade_frontalface.xml";
                        classifierPath = AppDomain.CurrentDomain.BaseDirectory + "Resource\\xml\\lbpcascades\\" + xmlFilePath;
                    }
                    break;
            }

            CascadeClassifier classifier = new CascadeClassifier(classifierPath);
            OpenCvSharp.Rect[] rect = classifier.DetectMultiScale(srcImg);
            if (rect.Count() > 0)
            {
                foreach (Rect vv in rect)
                {
                    Cv2.Rectangle(srcImg, vv, Scalar.Red, 1, LineTypes.AntiAlias);
                }
                dstImg = srcImg;
                return dstImg;
            }
            else
            {
                return srcImg;
            }
        }

        #region 皮肤检测

        [MethodCNName("皮肤检测")]
        [EnumParam("算法")]
        [ScriptParam("skinDefectEnum", ParamType.Enum, "", "算法")]
        public static Mat SkinDefect(Mat srcImg, SkinDefectEnum skinDefectEnum)
        {
            Mat dstImg = new Mat();
            switch (skinDefectEnum)
            {
                case SkinDefectEnum.BGR:
                    dstImg = SkinDefect_BGR(srcImg);
                    break;

                case SkinDefectEnum.HSV_Range:
                    dstImg = SkinDefect_HSVRange(srcImg);
                    break;

                case SkinDefectEnum.YCrCb_Range:
                    dstImg = SkinDefect_YCrCbRange(srcImg);
                    break;

                case SkinDefectEnum.YCrCb_Thresh:
                    dstImg = SkinDefect_YCrCbThresh(srcImg);
                    break;

                case SkinDefectEnum.Ellipse:
                    dstImg = SkinDefect_Ellipse(srcImg);
                    break;
            }
            return dstImg;
        }

        /// <summary>
        /// BGR皮肤检测
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        private static Mat SkinDefect_BGR(Mat srcImg)
        {
            Mat dstImg = Mat.Zeros(srcImg.Size(), MatType.CV_8UC3);
            if (srcImg.Empty() == true || srcImg.Type() != MatType.CV_8UC3)
            {
                return srcImg;
            }
            bool en, em;
            /*
               R>95 AND G>40 B>20 AND MAX(R,G,B)-MIN(R,G,B)>15 AND ABS(R-G)>15 AND R>G AND R>B
                OR
                R>220 AND G>210 AND B>170 AND ABS(R-G)<=15 AND R>B AND G>B
            */
            for (int i = 0; i < srcImg.Rows; i++)
            {
                for (int j = 0; j < srcImg.Cols; j++)
                {
                    Vec3b vec = srcImg.At<Vec3b>(i, j);
                    en = vec.Item2 > 95 && vec.Item1 > 40 && vec.Item0 > 20;
                    en = en && Math.Max(Math.Max(vec.Item0, vec.Item1), vec.Item2) - Math.Min(Math.Min(vec.Item0, vec.Item1), vec.Item2) > 15;
                    en = en && Math.Abs(vec.Item2 - vec.Item1) > 15 && vec.Item2 > vec.Item1 && vec.Item2 > vec.Item0;

                    em = vec.Item2 > 220 && vec.Item1 > 210 && vec.Item0 > 170;
                    em = em && Math.Abs(vec.Item2 - vec.Item1) <= 15;
                    em = em && vec.Item2 > vec.Item0 && vec.Item1 > vec.Item0;
                    if (en || em)
                    {
                        dstImg.At<Vec3b>(i, j) = vec;
                    }
                }
            }
            return dstImg;
        }

        /// <summary>
        /// 椭圆模型皮肤检测
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        private static Mat SkinDefect_Ellipse(Mat srcImg)
        {
            Mat dstImg = Mat.Zeros(srcImg.Size(), MatType.CV_8UC3);

            Mat img = srcImg.Clone();
            Mat skinCrCbHist = Mat.Zeros(new OpenCvSharp.Size(256, 256), MatType.CV_8UC1);
            Cv2.Ellipse(skinCrCbHist, new OpenCvSharp.Point(113, 155.6), new OpenCvSharp.Size(23.4, 15.2), 43.0, 0.0, 360.0, new OpenCvSharp.Scalar(255, 255, 255), -1);
            Mat ycrcb_image = new Mat();
            Mat output_mask = Mat.Zeros(img.Size(), MatType.CV_8UC1);
            Cv2.CvtColor(srcImg, ycrcb_image, ColorConversionCodes.BGR2YCrCb);
            for (int i = 0; i < img.Cols; i++)   //利用椭圆皮肤模型进行皮肤检测
            {
                for (int j = 0; j < img.Rows; j++)
                {
                    Vec3b ycrcb = ycrcb_image.At<Vec3b>(j, i);
                    if (skinCrCbHist.At<byte>(ycrcb[1], ycrcb[2]) > 0)   //如果该落在皮肤模型椭圆区域内，该点就是皮肤像素点
                        output_mask.At<byte>(j, i) = 255;
                }
            }
            img.CopyTo(dstImg, output_mask);  //返回肤色图
            return dstImg;
        }

        /// <summary>
        /// YCrCb 阈值检测
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        private static Mat SkinDefect_YCrCbThresh(Mat srcImg)
        {
            Mat dstImg = Mat.Zeros(srcImg.Size(), MatType.CV_8UC3);
            /*YCrCb颜色空间Cr分量+Otsu法*/
            Mat ycrcb_image = new Mat();
            Cv2.CvtColor(srcImg, ycrcb_image, ColorConversionCodes.BGR2YCrCb);
            Mat[] channels = new Mat[3];
            Cv2.Split(ycrcb_image, out channels);
            Mat output_mask = channels[1];
            Cv2.Threshold(output_mask, output_mask, 128, 255, ThresholdTypes.Binary);
            srcImg.CopyTo(dstImg, output_mask);
            return dstImg;
        }

        /// <summary>
        /// YCrCbRange
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        private static Mat SkinDefect_YCrCbRange(Mat srcImg)
        {
            Mat dstImg = Mat.Zeros(srcImg.Size(), MatType.CV_8UC3);
            Mat ycrcb_image = new Mat();
            Cv2.CvtColor(srcImg, ycrcb_image, ColorConversionCodes.BGR2YCrCb);
            Mat mask = Mat.Zeros(srcImg.Size(), MatType.CV_8UC1);

            for (int i = 0; i < srcImg.Rows; i++)
            {
                for (int j = 0; j < srcImg.Cols; j++)
                {
                    Vec3b vec = ycrcb_image.At<Vec3b>(i, j);
                    if (vec.Item1 > 133 && vec.Item1 < 173 && vec.Item2 > 77 && vec.Item2 < 127)
                    {
                        mask.At<byte>(i, j) = 255;
                    }
                }
            }
            srcImg.CopyTo(dstImg, mask);
            return dstImg;
        }

        private static Mat SkinDefect_HSVRange(Mat srcImg)
        {
            Mat dstImg = Mat.Zeros(srcImg.Size(), MatType.CV_8UC3);
            Mat ycrcb_image = new Mat();
            Cv2.CvtColor(srcImg, ycrcb_image, ColorConversionCodes.BGR2HSV);
            Mat mask = Mat.Zeros(srcImg.Size(), MatType.CV_8UC1);

            for (int i = 0; i < srcImg.Rows; i++)
            {
                for (int j = 0; j < srcImg.Cols; j++)
                {
                    Vec3b vec = ycrcb_image.At<Vec3b>(i, j);
                    if (vec.Item0 >= 0 && vec.Item0 <= 20 && vec.Item1 >= 48 && vec.Item2 >= 50)
                    {
                        mask.At<byte>(i, j) = 255;
                    }
                }
            }
            srcImg.CopyTo(dstImg, mask);
            return dstImg;
        }

        #endregion 皮肤检测

        /// <summary>
        /// 图像向上或向下翻转半
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        [MethodCNName("图像折叠")]
        [EnumParam("折叠方向")]
        [ScriptParam("direction", ParamType.Enum, "", "折叠方向")]
        public static Mat CompleteSymm(Mat srcImg, SymmDirection direction)
        {
            //转化成方阵
            int ss = Math.Min(srcImg.Rows, srcImg.Cols);
            OpenCvSharp.Size size = new OpenCvSharp.Size(ss, ss);
            Cv2.Resize(srcImg, srcImg, size);

            bool direct = direction == SymmDirection.LowerToUpper;
            Cv2.CompleteSymm(srcImg, direct);
            return srcImg;
        }

        /// <summary>
        /// RGB单通道显示 保留一个通道 将其他两个通道都设置为0
        /// </summary>
        /// <param name="src"></param>
        /// <param name="bGREnum"></param>
        /// <returns></returns>
        [MethodCNName("单通道显示")]
        [EnumParam("通道")]
        [ScriptParam("bGREnum", ParamType.Enum, "", "通道")]
        public static Mat BGRSingle(Mat src, BGREnum bGREnum)
        {
            float ratio = 1; // 增强比例
            // BGR 012
            Mat dstImg = src.Clone();
            float rf, gf, bf;
            for (int i = 0; i < src.Rows; i++)
            {
                for (int j = 0; j < src.Cols; j++)
                {
                    Vec3b vec = src.At<Vec3b>(i, j);
                    bf = vec.Item0;
                    gf = vec.Item1;
                    rf = vec.Item2;
                    switch (bGREnum)
                    {
                        case BGREnum.B:
                            {
                                gf = 0;
                                rf = 0;
                                bf *= ratio;
                            }
                            break;

                        case BGREnum.G:
                            {
                                bf = 0;
                                rf = 0;
                                gf *= ratio;
                            }
                            break;

                        case BGREnum.R:
                            {
                                bf = 0;
                                gf = 0;
                                rf *= ratio;
                            }
                            break;
                    }
                    dstImg.At<Vec3b>(i, j) = new Vec3b(GetCorrectPixel(bf), GetCorrectPixel(gf), GetCorrectPixel(rf));
                }
            }
            return dstImg;
            byte GetCorrectPixel(float sf)
            {
                if (sf < 0) return 0;
                if (sf > 255) return 255;
                return (byte)sf;
            }
        }

        #endregion 1参数

        #region 图像算术运算

        /// <summary>
        /// 两张图像相加 返回值 = srcImg1 + srcImg2
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <returns></returns>
        public static Mat Add(Mat srcImg1, Mat srcImg2)
        {
            Mat dstImg = new Mat();
            Cv2.Add(srcImg1, srcImg2, dstImg);
            return dstImg;
        }

        /// <summary>
        /// 权重相加  返回值等于 alpha * srcImg1 + beta * srcImg2 + gamma
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="gamma"></param>
        /// <returns></returns>
        public static Mat AddWeighted(Mat srcImg1, Mat srcImg2, double alpha, double beta, double gamma)
        {
            Mat dstImg = new Mat();
            Cv2.AddWeighted(srcImg1, alpha, srcImg2, beta, gamma, dstImg);
            return dstImg;
        }

        /// <summary>
        /// 相减绝对值  返回值: | srcImg1 - srcImg2 |
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <returns></returns>
        public static Mat AbsDiff(Mat srcImg1, Mat srcImg2)
        {
            Mat dstImg = new Mat();
            Cv2.Absdiff(srcImg1, srcImg2, dstImg);
            return dstImg;
        }

        /// <summary>
        /// 相减  返回值: srcImg1 - srcImg2  注意减与被减
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <returns></returns>
        public static Mat Subtract(Mat srcImg1, Mat srcImg2)
        {
            Mat dstImg = new Mat();
            Cv2.Subtract(srcImg1, srcImg2, dstImg);
            return dstImg;
        }

        /// <summary>
        /// 相与  返回值: srcImg1 ∩ srcImg2
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <returns></returns>
        public static Mat BitwiseAnd(Mat srcImg1, Mat srcImg2)
        {
            Mat dstImg = new Mat();
            Cv2.BitwiseAnd(srcImg1, srcImg2, dstImg);
            return dstImg;
        }

        /// <summary>
        /// 相或  返回值: srcImg1 ∪ srcImg2
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <returns></returns>
        public static Mat BitwiseOr(Mat srcImg1, Mat srcImg2)
        {
            Mat dstImg = new Mat();
            Cv2.BitwiseOr(srcImg1, srcImg2, dstImg);
            return dstImg;
        }

        /// <summary>
        /// 异或  返回值: srcImg1 ^ srcImg2
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <returns></returns>
        public static Mat BitwiseXor(Mat srcImg1, Mat srcImg2)
        {
            Mat dstImg = new Mat();
            Cv2.BitwiseXor(srcImg1, srcImg2, dstImg);
            return dstImg;
        }

        /// <summary>
        /// 取反  返回值: ~srcImg
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        public static Mat BitwiseNot(Mat srcImg)
        {
            Mat dstImg = new Mat();
            Cv2.BitwiseNot(srcImg, dstImg);
            return dstImg;
        }

        /// <summary>
        /// 次幂运算   返回值:  srcImg^power
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static Mat Power(Mat srcImg, double power)
        {
            Mat dstImg = new Mat();
            srcImg.ConvertTo(srcImg, MatType.CV_64F);
            Cv2.Pow(srcImg, power, dstImg);
            dstImg.ConvertTo(dstImg, MatType.CV_8U);
            return dstImg;
        }

        /// <summary>
        ///  指数运算  返回值:  e^srcImg  e=2.71828
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        public static Mat Exp(Mat srcImg)
        {
            Mat dstImg = new Mat();
            srcImg.ConvertTo(srcImg, MatType.CV_64F);
            Cv2.Exp(srcImg, dstImg);
            dstImg.ConvertTo(dstImg, MatType.CV_8U);
            return dstImg;
        }

        /// <summary>
        ///  对数运算  返回值:  ln(srcImg) 自然对数
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        public static Mat Log(Mat srcImg)
        {
            Mat dstImg = new Mat();
            srcImg.ConvertTo(srcImg, MatType.CV_64F);
            Cv2.Log(srcImg, dstImg);
            Cv2.Normalize(dstImg, dstImg, 255, 0, OpenCvSharp.NormTypes.MinMax);
            dstImg.ConvertTo(dstImg, MatType.CV_8U);
            return dstImg;
        }

        /// <summary>
        /// 图像绝对值   | srcImg |
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        public static Mat Abs(Mat srcImg)
        {
            Mat dstImg = new Mat();
            dstImg = Cv2.Abs(srcImg);
            // 计算图像绝对值 并将图像转化成 8位数据位
            // Cv2.ConvertScaleAbs(mm, dstImg);
            return dstImg;
        }

        #endregion 图像算术运算

        #region 图像相似性比较

        /// <summary>
        /// 均值哈希
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <returns></returns>
        public static double AHash(Mat srcImg1, Mat srcImg2)
        {
            // 图像缩放---转灰度求像素均值---比较均值求哈希列---比较哈希列的结果
            OpenCvSharp.Size size = new OpenCvSharp.Size(8, 8);
            Cv2.Resize(srcImg1, srcImg1, size);
            Cv2.Resize(srcImg2, srcImg2, size);

            Cv2.CvtColor(srcImg1, srcImg1, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(srcImg2, srcImg2, ColorConversionCodes.BGR2GRAY);

            int sum1 = 0, sum2 = 0; // 两张图的像素和
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    sum1 += srcImg1.Get<byte>(i, j);
                    sum2 += srcImg2.Get<byte>(i, j);
                }
            }

            int avg1 = 0, avg2 = 0;// 像素和均值
            avg1 = sum1 / 64;
            avg2 = sum2 / 64;
            //m1.Sum();
            //m2.Sum();

            string hash1 = "", hash2 = ""; // 哈希列
                                           // 原图与像素和均值比较 大于则往哈希列中添1 小于就添0
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (srcImg1.Get<byte>(i, j) > avg1)
                    {
                        hash1 += "1";
                    }
                    else
                    {
                        hash1 += "0";
                    }
                    if (srcImg2.Get<byte>(i, j) > avg2)
                    {
                        hash2 += "1";
                    }
                    else
                    {
                        hash2 += "0";
                    }
                }
            }

            int num = 0; // 相同位置相同字符的数目
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1.Substring(i, 1) == hash2.Substring(i, 1))
                {
                    num++;
                }
            }

            return num * 1.0 / hash1.Length;
        }

        /// <summary>
        /// 差值哈希
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <returns></returns>
        public static double DHash(Mat srcImg1, Mat srcImg2)
        {
            // 图像缩放--转灰度--比较每一行前后像素值得哈希列--得到相似度
            OpenCvSharp.Size size = new OpenCvSharp.Size(8, 9);
            Cv2.Resize(srcImg1, srcImg1, size);
            Cv2.Resize(srcImg2, srcImg2, size);

            Cv2.CvtColor(srcImg1, srcImg1, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(srcImg2, srcImg2, ColorConversionCodes.BGR2GRAY);

            string hash1 = "", hash2 = ""; // 哈希列

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (srcImg1.Get<byte>(i, j) > srcImg1.Get<byte>(i, j + 1))
                    {
                        hash1 += "1";
                    }
                    else
                    {
                        hash1 += "0";
                    }
                    if (srcImg2.Get<byte>(i, j) > srcImg2.Get<byte>(i, j + 1))
                    {
                        hash2 += "1";
                    }
                    else
                    {
                        hash2 += "0";
                    }
                }
            }

            int num = 0; // 相同位置相同字符的数目
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1.Substring(i, 1) == hash2.Substring(i, 1))
                {
                    num++;
                }
            }
            return num * 1.0 / hash1.Length;
        }

        /// <summary>
        /// 感知哈希
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <returns></returns>
        public static double PHash(Mat srcImg1, Mat srcImg2)
        {
            OpenCvSharp.Size size = new OpenCvSharp.Size(32, 32);
            Cv2.Resize(srcImg1, srcImg1, size);
            Cv2.Resize(srcImg2, srcImg2, size);

            Cv2.CvtColor(srcImg1, srcImg1, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(srcImg2, srcImg2, ColorConversionCodes.BGR2GRAY);

            srcImg1.ConvertTo(srcImg1, MatType.CV_32F);
            srcImg2.ConvertTo(srcImg2, MatType.CV_32F);

            Cv2.Dct(srcImg1, srcImg1);
            Cv2.Dct(srcImg2, srcImg2);
            // 取左上角 8*8 即可 计算平均值 求哈希列
            int sum1 = 0, sum2 = 0; // 两张图的像素和
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    sum1 += srcImg1.Get<byte>(i, j);
                    sum2 += srcImg2.Get<byte>(i, j);
                }
            }

            int avg1 = 0, avg2 = 0;// 像素和均值
            avg1 = sum1 / 64;
            avg2 = sum2 / 64;

            string hash1 = "", hash2 = ""; // 哈希列
                                           // 原图与像素和均值比较 大于则往哈希列中添1 小于就添0
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (srcImg1.Get<byte>(i, j) > avg1)
                    {
                        hash1 += "1";
                    }
                    else
                    {
                        hash1 += "0";
                    }
                    if (srcImg2.Get<byte>(i, j) > avg2)
                    {
                        hash2 += "1";
                    }
                    else
                    {
                        hash2 += "0";
                    }
                }
            }

            int num = 0; // 相同位置相同字符的数目
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1.Substring(i, 1) == hash2.Substring(i, 1))
                {
                    num++;
                }
            }

            return num * 1.0 / hash1.Length;
        }

        /// <summary>
        /// SSIM
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <returns></returns>
        public static double SSIM(Mat srcImg1, Mat srcImg2)
        {
            const double C1 = 6.5025, C2 = 58.5225;

            Mat I1 = new Mat(), I2 = new Mat();
            srcImg1.ConvertTo(I1, MatType.CV_32F);
            srcImg2.ConvertTo(I2, MatType.CV_32F);

            Mat I2_2 = I2.Mul(I2);        // I2^2
            Mat I1_2 = I1.Mul(I1);        // I1^2
            Mat I1_I2 = I1.Mul(I2);        // I1 * I2

            Mat mu1 = new Mat(), mu2 = new Mat();   //
            Cv2.GaussianBlur(I1, mu1, new OpenCvSharp.Size(11, 11), 1.5);
            Cv2.GaussianBlur(I2, mu2, new OpenCvSharp.Size(11, 11), 1.5);

            Mat mu1_2 = mu1.Mul(mu1).ToMat();
            Mat mu2_2 = mu2.Mul(mu2).ToMat();
            Mat mu1_mu2 = mu1.Mul(mu2).ToMat();

            Mat sigma1_2 = new Mat(), sigma2_2 = new Mat(), sigma12 = new Mat();

            Cv2.GaussianBlur(I1_2, sigma1_2, new OpenCvSharp.Size(11, 11), 1.5);
            sigma1_2 -= mu1_2;

            Cv2.GaussianBlur(I2_2, sigma2_2, new OpenCvSharp.Size(11, 11), 1.5);
            sigma2_2 -= mu2_2;

            Cv2.GaussianBlur(I1_I2, sigma12, new OpenCvSharp.Size(11, 11), 1.5);
            sigma12 -= mu1_mu2;

            // FORMULA
            Mat t1, t2, t3;
            t1 = 2 * mu1_mu2 + new Scalar(C1);
            t2 = 2 * sigma12 + new Scalar(C2);
            t3 = t1.Mul(t2);
            //t1 = 2 * mu1_mu2 + C1;
            //t2 = 2 * sigma12 + C2;
            //t3 = t1.Mul(t2);              // t3 = ((2*mu1_mu2 + C1).*(2*sigma12 + C2))

            t1 = mu1_2 + mu2_2 + new Scalar(C1);
            t2 = sigma1_2 + sigma2_2 + new Scalar(C2);
            t1 = t1.Mul(t2);
            //t1 = mu1_2 + mu2_2 + C1;
            //t2 = sigma1_2 + sigma2_2 + C2;
            //t1 = t1.Mul(t2);               // t1 =((mu1_2 + mu2_2 + C1).*(sigma1_2 + sigma2_2 + C2))

            Mat ssim_map = new Mat();
            Cv2.Divide(t3, t1, ssim_map);      // ssim_map =  t3./t1;

            Scalar mssim = Cv2.Mean(ssim_map);// mssim = average of ssim map

            return (mssim.Val0 + mssim.Val1 + mssim.Val2) / 3.0;
        }

        /// <summary>
        /// PSNR        //  等于 ===>>> Cv2.PSNR
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <returns></returns>
        public static double PSNR(Mat srcImg1, Mat srcImg2)
        {
            srcImg1.ConvertTo(srcImg1, MatType.CV_32F);
            srcImg2.ConvertTo(srcImg2, MatType.CV_32F);

            Mat diff = new Mat(srcImg1.Size(), MatType.CV_32F);
            Cv2.Absdiff(srcImg1, srcImg2, diff);  //  | mat1-mat2 |
            diff = diff.Mul(diff);  //    | mat1-mat2 |^2
            Scalar s = Cv2.Sum(diff);
            double n = srcImg1.Channels() * srcImg1.Total();
            double sse;
            if (srcImg1.Channels() == 3)
            {
                sse = s.Val0 + s.Val1 + s.Val2;
            }
            else
            {
                sse = s.Val0;
            }
            double mse = sse * 1.0 / n;
            double psnr = 10.0 * Math.Log10((255 * 255 * 1.0) / (mse + 0.000001));
            return psnr;
        }

        #endregion 图像相似性比较

        /// <summary>
        /// 图像拼接
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <param name="concatEnum"></param>
        /// <returns></returns>
        public static Mat Concat(Mat srcImg1, Mat srcImg2, ConcatEnum concatEnum)
        {
            Mat dstImg = new Mat();
            switch (concatEnum)
            {
                case ConcatEnum.Stitch:
                    {
                        Mat[] srcImgArr = new Mat[] { srcImg1, srcImg2 };

                        Stitcher st = Stitcher.Create(Stitcher.Mode.Scans);
                        var status = st.Stitch(srcImgArr, dstImg);
                        //if (status != Stitcher.Status.OK)
                        //{
                        //   string errorMsg = status.ToString();
                        //}
                    }
                    break;

                case ConcatEnum.HConcat:
                    {
                        Cv2.HConcat(srcImg1, srcImg2, dstImg);
                    }
                    break;

                case ConcatEnum.VConcat:
                    {
                        Cv2.VConcat(srcImg1, srcImg2, dstImg);
                    }
                    break;
            }
            return dstImg;
        }

        #region 特征点匹配

        /// <summary>
        /// SIFT 特征点匹配
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <param name="matcher"></param>
        /// <param name="matchDisParam"></param>
        /// <returns></returns>
        public static FeatureMatchResModel SIFTMatch(Mat srcImg1, Mat srcImg2, MatcherMode matcher, double matchDisParam)
        {
            FeatureMatchResModel matchResModel = new FeatureMatchResModel();
            //pic2.Resize(pic1.Size());
            SIFT sift = SIFT.Create(1000);
            KeyPoint[] kp1 = sift.Detect(srcImg1);
            KeyPoint[] kp2 = sift.Detect(srcImg2);

            Mat sift_feature = new Mat();
            Mat sift_feature2 = new Mat();

            Cv2.DrawKeypoints(srcImg1, kp1, sift_feature,
                                    new Scalar(0, 255, 0), DrawMatchesFlags.Default);
            Cv2.DrawKeypoints(srcImg2, kp2, sift_feature2,
                                    new Scalar(0, 255, 0), DrawMatchesFlags.Default);
            matchResModel.FeatureDstMat1 = sift_feature;
            matchResModel.FeatureDstMat2 = sift_feature2;

            Mat descriptors = new Mat();
            Mat descriptors2 = new Mat();
            sift.Compute(srcImg1, ref kp1, descriptors);
            sift.Compute(srcImg2, ref kp2, descriptors2);

            DMatch[] matches = new DMatch[0];

            switch (matcher)
            {
                case MatcherMode.BFMatcher:
                    {
                        //暴力匹配器
                        BFMatcher bFMatcher = new BFMatcher(NormTypes.L2);
                        matches = bFMatcher.Match(descriptors, descriptors2);
                    }
                    break;

                case MatcherMode.FlannMatcher:
                    {
                        //Flann匹配器
                        LinearIndexParams id = new LinearIndexParams();
                        SearchParams sp = new SearchParams();
                        FlannBasedMatcher flannBasedMatcher = new FlannBasedMatcher(id, sp);
                        matches = flannBasedMatcher.Match(descriptors, descriptors2);
                    }
                    break;
            }

            //计算匹配距离
            double max_dist = 0, min_dist = 100;
            for (int i = 0; i < descriptors.Rows; i++)
            {
                if (matches[i].Distance > max_dist)
                    max_dist = matches[i].Distance;
                if (matches[i].Distance < min_dist && matches[i].Distance > 0)
                    min_dist = matches[i].Distance;
            }

            //if (min_dist == 0)
            //    min_dist = 1;
            //匹配结果删选
            List<DMatch> good_matches = new List<DMatch>();
            for (int i = 0; i < matches.Length; i++)
            {
                if (matches[i].Distance < (min_dist + max_dist) / matchDisParam)
                    good_matches.Add(matches[i]);
                //if (matches[i].Distance > 5 * min_dist && matches[i].Distance < 0.2 * max_dist)
                //    good_matches.Add(matches[i]);
            }

            Mat result = new Mat();
            //匹配删选前
            //Cv2.DrawMatches(srcImg, vKeyPoints, srcImg2, vKeyPoints2, matches,
            //                result, new Scalar(0, 255, 0), new Scalar(0, 0, 255));
            //匹配删选后
            Cv2.DrawMatches(srcImg1, kp1, srcImg2, kp2, good_matches,
                            result, new Scalar(0, 255, 0), new Scalar(0, 0, 255));

            matchResModel.FeatureMatchDstMat = result;
            return matchResModel;
        }

        /// <summary>
        ///  SURF 特征点匹配
        /// </summary>
        /// <param name="srcImg1"></param>
        /// <param name="srcImg2"></param>
        /// <param name="matcher"></param>
        /// <param name="matchDisParam"></param>
        /// <returns></returns>
        public static FeatureMatchResModel SURFMatch(Mat srcImg1, Mat srcImg2, MatcherMode matcher, double matchDisParam)
        {
            FeatureMatchResModel matchResModel = new FeatureMatchResModel();
            // pic2.Resize(pic1.Size());
            SURF sift = SURF.Create(1000);
            KeyPoint[] kp1 = sift.Detect(srcImg1);
            KeyPoint[] kp2 = sift.Detect(srcImg2);

            Mat sift_feature = new Mat();
            Mat sift_feature2 = new Mat();

            Cv2.DrawKeypoints(srcImg1, kp1, sift_feature,
                                    new Scalar(0, 255, 0), DrawMatchesFlags.Default);
            Cv2.DrawKeypoints(srcImg2, kp2, sift_feature2,
                                    new Scalar(0, 255, 0), DrawMatchesFlags.Default);

            matchResModel.FeatureDstMat1 = sift_feature;
            matchResModel.FeatureDstMat2 = sift_feature2;

            Mat descriptors = new Mat();
            Mat descriptors2 = new Mat();
            sift.Compute(srcImg1, ref kp1, descriptors);
            sift.Compute(srcImg2, ref kp2, descriptors2);

            DMatch[] matches = new DMatch[0];
            switch (matcher)
            {
                case MatcherMode.BFMatcher:
                    {
                        //暴力匹配器
                        BFMatcher bFMatcher = new BFMatcher(NormTypes.L2);
                        matches = bFMatcher.Match(descriptors, descriptors2);
                    }
                    break;

                case MatcherMode.FlannMatcher:
                    {
                        //Flann匹配器
                        LinearIndexParams id = new LinearIndexParams();
                        SearchParams sp = new SearchParams();
                        FlannBasedMatcher flannBasedMatcher = new FlannBasedMatcher(id, sp);
                        matches = flannBasedMatcher.Match(descriptors, descriptors2);
                    }
                    break;
            }

            //计算匹配距离
            double max_dist = 0, min_dist = 100;
            for (int i = 0; i < descriptors.Rows; i++)
            {
                if (matches[i].Distance > max_dist)
                    max_dist = matches[i].Distance;
                if (matches[i].Distance < min_dist && matches[i].Distance > 0)
                    min_dist = matches[i].Distance;
            }
            //if (min_dist == 0)
            //    min_dist = 1;

            //匹配结果删选
            List<DMatch> good_matches = new List<DMatch>();
            for (int i = 0; i < matches.Length; i++)
            {
                if (matches[i].Distance < (min_dist + max_dist) / matchDisParam)
                    good_matches.Add(matches[i]);
            }

            Mat result = new Mat();
            //匹配删选前
            //Cv2.DrawMatches(srcImg, vKeyPoints, srcImg2, vKeyPoints2, matches,
            //                result, new Scalar(0, 255, 0), new Scalar(0, 0, 255));
            //匹配删选后
            Cv2.DrawMatches(srcImg1, kp1, srcImg2, kp2, good_matches,
                            result, new Scalar(0, 255, 0), new Scalar(0, 0, 255));

            matchResModel.FeatureMatchDstMat = result;
            return matchResModel;
        }

        #endregion 特征点匹配

        #region 特征检测

        /// <summary>
        /// 霍夫圆检测
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="dp"></param>
        /// <param name="minDist"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <param name="minRadius"></param>
        /// <param name="maxRadius"></param>
        /// <returns></returns>
        public static Mat HoughCircle(Mat srcImg, double dp, double minDist, double param1, double param2, int minRadius, int maxRadius)
        {
            Mat dstImg = new Mat();
            Mat gray = new Mat();

            Cv2.MedianBlur(srcImg, gray, 3);

            Cv2.CvtColor(gray, gray, ColorConversionCodes.BGR2GRAY);

            //3：霍夫圆检测：使用霍夫变换查找灰度图像中的圆。
            /*
             * 参数：
             *      1：输入参数： 8位、单通道、灰度输入图像
             *      2：实现方法：目前，唯一的实现方法是 ********HoughCirclesMethod.Gradient
             *      3: dp      :累加器分辨率与图像分辨率的反比。默认=1
             *      4：minDist: 检测到的圆的中心之间的最小距离。(最短距离-可以分辨是两个圆的，否则认为是同心圆- rc_gray.rows/8)
             *      5:param1:   第一个方法特定的参数。[默认值是100] canny边缘检测阈值低
             *      6:param2:   第二个方法特定于参数。[默认值是100] 中心点累加器阈值 – 候选圆心
             *      7:minRadius: 最小半径
             *      8:maxRadius: 最大半径
             *
             */
            HoughModes type = HoughModes.Gradient;
            CircleSegment[] cs = Cv2.HoughCircles(gray, type,
                                         dp, minDist,
                                         param1, param2,
                                         minRadius, maxRadius);
            if (cs.Count() <= 0)
            {
                // string ErrorMsg = "未检测到圆,请适当调整参数或更换图片";
                return srcImg;
            }

            srcImg.CopyTo(dstImg);
            for (int i = 0; i < cs.Count(); i++)
            {
                //画圆
                Cv2.Circle(dstImg, (int)cs[i].Center.X, (int)cs[i].Center.Y, (int)cs[i].Radius, new Scalar(0, 0, 255), 2, LineTypes.AntiAlias);
                //加强圆心显示
                Cv2.Circle(dstImg, (int)cs[i].Center.X, (int)cs[i].Center.Y, 1, new Scalar(0, 0, 255), 2, LineTypes.AntiAlias);
            }
            return dstImg;
        }

        /// <summary>
        /// 霍夫直线检测
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="rho"></param>
        /// <param name="theta"></param>
        /// <param name="threshold"></param>
        /// <param name="minLineLength"></param>
        /// <param name="maxLineGap"></param>
        /// <returns></returns>
        public static Mat HoughLines(Mat srcImg, double rho, double theta, int threshold, double minLineLength, double maxLineGap)
        {
            Mat dstImg = new Mat();
            Mat gray = new Mat();

            Cv2.CvtColor(srcImg, gray, ColorConversionCodes.BGR2GRAY);

            Cv2.MedianBlur(gray, gray, 3);

            Cv2.Canny(gray, gray, 100, 200);
            Cv2.Threshold(gray, gray, 128, 255, ThresholdTypes.Binary);

            LineSegmentPoint[] lineSegmentPoint = Cv2.HoughLinesP(gray,
                                          rho, Cv2.PI / 180 * theta,
                                          threshold, minLineLength,
                                         maxLineGap);

            if (lineSegmentPoint.Count() <= 0)
            {
                string ErrorMsg = "没有检测到线,请适当调整参数或更换图片";
                return srcImg;
            }
            srcImg.CopyTo(dstImg);
            for (int i = 0; i < lineSegmentPoint.Count(); i++)
            {
                Cv2.Line(dstImg, lineSegmentPoint[i].P1, lineSegmentPoint[i].P2, Scalar.Red, 2, LineTypes.AntiAlias);
            }
            return dstImg;
        }

        /// <summary>
        /// 角点检测
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="dstImg"></param>
        /// <param name="maxCorners"></param>
        /// <param name="qualityLevel"></param>
        /// <param name="minDistance"></param>
        /// <param name="blockSize"></param>
        /// <param name="useHarrisDetector"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static Mat ConnerGoodFeatures(Mat srcImg, int maxCorners, double qualityLevel, double minDistance, int blockSize, bool useHarrisDetector, double k)
        {
            Mat dstImg = new Mat();
            Mat gray = new Mat();

            Cv2.CvtColor(srcImg, gray, ColorConversionCodes.BGR2GRAY);

            Point2f[] pt = Cv2.GoodFeaturesToTrack(gray,
                                   maxCorners, qualityLevel,
                                   minDistance, new Mat(), blockSize, useHarrisDetector, k);

            if (pt.Count() <= 0)
            {
                string ErrorMsg = "未检测到角点,请适当调整参数或者更换图片";
                return srcImg;
            }

            srcImg.CopyTo(dstImg);
            foreach (Point2f pp in pt)
            {
                Cv2.Circle(dstImg, Convert.ToInt16(pp.X), Convert.ToInt16(pp.Y), 2, Scalar.Red, 1, LineTypes.AntiAlias);
            }
            return dstImg;
        }

        /// <summary>
        /// 亚像素矩阵提取
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="rectWidth"></param>
        /// <param name="rectHeight"></param>
        /// <param name="rectCenterX"></param>
        /// <param name="rectCenterY"></param>
        /// <returns></returns>
        public static Mat RectSubPix(Mat srcImg, int rectWidth, int rectHeight, double rectCenterX, double rectCenterY)
        {
            OpenCvSharp.Size size = new OpenCvSharp.Size(rectWidth, rectHeight);
            OpenCvSharp.Point2f pt = new OpenCvSharp.Point2f((float)rectCenterX, (float)rectCenterY);
            Mat dstImg = new Mat();
            Cv2.GetRectSubPix(srcImg, size, pt, dstImg);
            return dstImg;
        }

        public static Mat HSVRecogn(Mat srcImg, Scalar scalarMin, Scalar scalarMax)        //  HSV颜色识别
        {
            Mat dstImg = srcImg.Clone();
            Mat hsv = new Mat();
            Cv2.CvtColor(srcImg, hsv, ColorConversionCodes.BGR2HSV);//转化为HSV

            Mat tempMat = new Mat();

            Cv2.InRange(hsv, scalarMin, scalarMax, tempMat);
            //核 结构元
            var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(20, 20),
               new OpenCvSharp.Point(-1, -1));
            Cv2.Threshold(tempMat, tempMat, 0, 255, ThresholdTypes.Binary);         //二值化
            Cv2.Dilate(tempMat, tempMat, kernel); //膨胀
            Cv2.Erode(tempMat, tempMat, kernel); // 腐蚀

            //获得轮廓
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchly;
            Cv2.FindContours(tempMat, out contours, out hierarchly, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple, null);

            if (contours.Length > 0)
            {
                var boxes = contours.Select(Cv2.BoundingRect).Where(w => w.Height >= 1 && w.Width > 1);
                foreach (var rect in boxes)
                {
                    Cv2.Rectangle(dstImg, new OpenCvSharp.Point(rect.X, rect.Y), new OpenCvSharp.Point(rect.X + rect.Width, rect.Y + rect.Height), new OpenCvSharp.Scalar(0, 0, 255), 1);
                }
            }
            return dstImg;
        }

        #endregion 特征检测

        #region 形态学操作

        /// <summary>
        /// 图像形态学操作
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="shape"></param>
        /// <param name="size"></param>
        /// <param name="type"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        [MethodCNName("形态学操作")]
        [NoScript("暂时不支持Size 参数输入")]
        [ScriptParam("shape", ParamType.Enum, "", "结构元")]
        public static Mat Morphology(Mat srcImg, MorphShapes shape, OpenCvSharp.Size size, MorphTypes type, int times = 1)
        {
            Mat dstImg = new Mat();
            if (type == MorphTypes.HitMiss)
            {
                Cv2.CvtColor(srcImg, srcImg, ColorConversionCodes.BGR2GRAY);
            }
            Mat structs = Cv2.GetStructuringElement(shape, size);
            OpenCvSharp.Point anchors = new OpenCvSharp.Point(-1, -1);
            Cv2.MorphologyEx(srcImg, dstImg, type, structs, anchors, times);
            return dstImg;
        }

        [MethodCNName("金字塔分割")]
        [ScriptParam("sp", ParamType.Double, "1", "sp")]
        [ScriptParam("sr", ParamType.Double, "1", "sr")]
        [ScriptParam("maxlevel", ParamType.Interger, "1", "最大层级")]
        public static Mat MeanShiftFilter(Mat srcImg, double sp, double sr, int maxlevel)
        {
            Mat dstImg = new Mat();
            Cv2.PyrMeanShiftFiltering(srcImg, dstImg, sp, sr, maxlevel);
            return dstImg;
        }

        /// <summary>
        /// 图像横纵分别重复指定次数
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="ny">竖直</param>
        /// <param name="nx">水平</param>
        /// <returns></returns>
        [MethodCNName("图像堆叠")]
        [ScriptParam("ny", ParamType.Interger, "1", "竖直数量")]
        [ScriptParam("nx", ParamType.Interger, "1", "水平数量")]
        public static Mat Repeat(Mat srcImg, int ny, int nx)
        {
            Mat dstImg = new Mat();
            dstImg = Cv2.Repeat(srcImg, ny, nx);
            return dstImg;
        }

        /// <summary>
        /// 距离变换
        /// </summary>
        /// <param name="srcImg"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [MethodCNName("距离变换")]
        [ScriptParam("type", ParamType.Enum, "", "变换方式")]
        [ScriptParam("size", ParamType.Enum, "", "大小")]
        public static Mat DistanceTranForm(Mat srcImg, OpenCvSharp.DistanceTypes type, OpenCvSharp.DistanceTransformMasks size)
        {
            Mat dstImg = new Mat();
            Mat gray = new Mat();
            Cv2.CvtColor(srcImg, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(gray, gray, 100, 255, ThresholdTypes.Binary);

            Cv2.DistanceTransform(gray, dstImg, type, size);

            dstImg.ConvertTo(dstImg, MatType.CV_8U);
            Cv2.Normalize(dstImg, dstImg, 255, 0, NormTypes.MinMax);
            return dstImg;
        }

        [MethodCNName("极空间变换")]
        [NoScript("暂时不支持center 参数输入")]
        public static Mat Polar(Mat srcImg, OpenCvSharp.Point2f center, double maxRadius, OpenCvSharp.InterpolationFlags flag, PolarMode mode)
        {
            Mat dstImg = new Mat();
            switch (mode)
            {
                case PolarMode.LinerPolar:
                    {
                        Cv2.LinearPolar(srcImg, dstImg, center, maxRadius, flag);
                    }
                    break;

                case PolarMode.LogPolar:
                    {
                        Cv2.LogPolar(srcImg, dstImg, center, maxRadius, flag);
                    }
                    break;
            }
            return dstImg;
        }

        #endregion 形态学操作

        /// <summary>
        /// 文字识别
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        [MethodCNName("文字识别")]
        public static Mat OCRText(Mat srcImg)
        {
            Mat dstImg = srcImg.Clone();
            string path = AppDomain.CurrentDomain.BaseDirectory + "Resource\\tessdata\\";
            OpenCvSharp.Text.OCRTesseract test = OpenCvSharp.Text.OCRTesseract.Create(path, "eng");

            string readStr = "";
            OpenCvSharp.Rect[] rects;
            string[] strs;
            float[] rrs;

            test.Run(srcImg, out readStr, out rects, out strs, out rrs);

            if (rects.Length <= 0)
            {
                return srcImg;
            }

            for (int i = 0; i < rects.Length; i++)
            {
                Cv2.Rectangle(srcImg, rects[i], Scalar.Green, 2, LineTypes.AntiAlias);
                Cv2.PutText(srcImg, strs[i], rects[i].TopLeft, HersheyFonts.HersheyComplex, 1.0, Scalar.Green, 2, LineTypes.AntiAlias);
            }
            return srcImg;
        }

        //public static Mat OCRText(Mat srcImg)
        //{
        //    Mat dstImg = srcImg.Clone();
        //    string path = AppDomain.CurrentDomain.BaseDirectory + "Resource\\tessdata\\";
        //    OpenCvSharp.Text.OCRTesseract test = OpenCvSharp.Text.OCRTesseract.Create(path, "eng");

        //    string readStr = "";
        //    OpenCvSharp.Rect[] rects;
        //    string[] strs;
        //    float[] rrs;

        //    Mat gray = new Mat();
        //    Cv2.CvtColor(srcImg, gray, ColorConversionCodes.BGR2GRAY);
        //    Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(3, 3), 0, 0);
        //    // Cv2.Threshold(gray, gray, 128, 255, ThresholdTypes.Binary);
        //    Cv2.Canny(gray, gray, 50, 120);

        //    OpenCvSharp.Point[][] countours;
        //    OpenCvSharp.HierarchyIndex[] hierarchy;

        //    Cv2.FindContours(gray, out countours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);

        //    for (int i = 0; i < countours.Length; i++)
        //    {
        //        OpenCvSharp.Rect rect = Cv2.BoundingRect(countours[i]);
        //        int ww = rect.Width;
        //        int hh = rect.Height;

        //        if (ww < 20 && hh < 20)
        //            continue;

        //        Mat roiImg;

        //        if (rect.X > 10 && rect.Y > 10)
        //        {
        //            roiImg = new Mat(gray, new OpenCvSharp.Rect(rect.X - 10, rect.Y - 10, Math.Min(rect.Width + 20, gray.Width - rect.X + 10), Math.Min(rect.Height + 20, gray.Height - rect.Y + 10)));
        //        }
        //        else
        //        {
        //            roiImg = new Mat(gray, new OpenCvSharp.Rect(rect.X, rect.Y, rect.Width, rect.Height));
        //        }
        //        test.Run(roiImg, out readStr, out rects, out strs, out rrs);
        //        if (readStr == "")
        //            continue;

        //        for (int k = 0; k < rects.Count(); k++)
        //        {
        //            //  rects[k].Location += new OpenCvSharp.Point(rect.X -10, rect.Y-10);

        //            if (rect.X > 10 && rect.Y > 10)
        //            {
        //                rects[k].Location += new OpenCvSharp.Point(rect.X - 10, rect.Y - 10);
        //            }
        //            else
        //            {
        //                rects[k].Location += new OpenCvSharp.Point(rect.X, rect.Y);
        //            }
        //        }

        //        for (int j = 0; j < rects.Length; j++)
        //        {
        //            Cv2.Rectangle(dstImg, rects[j], Scalar.Green, 2, LineTypes.AntiAlias);
        //            Cv2.PutText(dstImg, strs[j], rects[j].TopLeft, HersheyFonts.HersheyComplex, 1.0, Scalar.Green, 2, LineTypes.AntiAlias);
        //        }
        //    }
        //    return dstImg;
        //}

        #region 普通二维码

        public static BitmapImage GetNormalQRCode(string url, int pixel, int width = 256, int height = 256, bool blankSide = true, System.Windows.Media.Color? color = null)
        {
            // 将 WPF Color 转换为 System.Drawing.Color
            System.Drawing.Color drawingColor = color.HasValue
                ? System.Drawing.Color.FromArgb(color.Value.A, color.Value.R, color.Value.G, color.Value.B)
                : System.Drawing.Color.Black;

            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData codeData = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M, true);
            QRCoder.QRCode qrcode = new QRCoder.QRCode(codeData);

            using (System.Drawing.Bitmap qrImage = qrcode.GetGraphic(pixel, drawingColor, System.Drawing.Color.White, blankSide))
            using (System.Drawing.Bitmap resizedImage = new System.Drawing.Bitmap(qrImage, width, height))
            {
                return ConvertToBitmapImage(resizedImage);
            }
        }

        private static BitmapImage ConvertToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        #endregion 普通二维码

        #region 带logo的二维码

        public static BitmapImage GetLogoQRCode(string url, BitmapImage icon, int pixel, int width = 256, int height = 256, bool blankSide = true, System.Windows.Media.Color? color = null)
        {
            // 将 WPF Color 转换为 System.Drawing.Color
            System.Drawing.Color drawingColor = color.HasValue
                ? System.Drawing.Color.FromArgb(color.Value.A, color.Value.R, color.Value.G, color.Value.B)
                : System.Drawing.Color.Black;

            // 将 BitmapImage 转换为 System.Drawing.Bitmap
            System.Drawing.Bitmap drawingIcon = ConvertToDrawingBitmap(icon);

            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData codeData = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M, true);
            QRCoder.QRCode qrcode = new QRCoder.QRCode(codeData);

            using (drawingIcon)
            using (System.Drawing.Bitmap qrImage = qrcode.GetGraphic(pixel, drawingColor, System.Drawing.Color.White, drawingIcon, 10, 2, blankSide))
            using (System.Drawing.Bitmap resizedImage = new System.Drawing.Bitmap(qrImage, width, height))
            {
                return ConvertToBitmapImage(resizedImage);
            }
        }

        // 将 BitmapImage 转换为 System.Drawing.Bitmap
        private static System.Drawing.Bitmap ConvertToDrawingBitmap(BitmapImage bitmapImage)
        {
            if (bitmapImage == null)
                return null;

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);

                return new System.Drawing.Bitmap(outStream);
            }
        }

        #endregion 带logo的二维码
    }

    internal struct zxy
    {
        public double x, y, z;
    }

    public class MethodCNNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public MethodCNNameAttribute(string name)
        {
            Name = name;
        }
    }
}