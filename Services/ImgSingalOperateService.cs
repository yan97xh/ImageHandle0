using ImageHandle.Helpers;
using ImageHandle.Models;
using OpenCvSharp;

namespace ImageHandle.Services
{
    /// <summary>
    /// 图像简单操作处理 无参数方法
    /// </summary>
    public class ImgSingalOperateService
    {
        #region 懒加载实现单例
        private static readonly Lazy<ImgSingalOperateService> _instance = new Lazy<ImgSingalOperateService>(() => new ImgSingalOperateService());

        public static ImgSingalOperateService Instance => _instance.Value;
        private ImgSingalOperateService()
        {

        }

        #endregion

        #region 图像简单操作

        private readonly Dictionary<ImgSingalOperations, Func<Mat, Mat>> _operations = new()
         {
              { ImgSingalOperations.CvtColorToRGB, ImageOperateMethods.CvtColorToRGB },
              { ImgSingalOperations.CvtColorToHSV, ImageOperateMethods.CvtColorToHSV },
              { ImgSingalOperations.CvtColorToLab, ImageOperateMethods.CvtColorToLab },
              { ImgSingalOperations.EqualizeHist, ImageOperateMethods.EqualizeHist },
              { ImgSingalOperations.CvtColorToGray, ImageOperateMethods.CvtColorToGray },
              { ImgSingalOperations.Negation, ImageOperateMethods.Negation },
              { ImgSingalOperations.Otsu, ImageOperateMethods.Otsu },
              { ImgSingalOperations.RetroEffect, ImageOperateMethods.RetroEffect },
              { ImgSingalOperations.FusedCastEffect, ImageOperateMethods.FusedCastEffect },
              { ImgSingalOperations.FrozenEffect, ImageOperateMethods.FrozenEffect },
              { ImgSingalOperations.ComicEffect, ImageOperateMethods.ComicEffect },
              { ImgSingalOperations.FleetingEffect, ImageOperateMethods.FleetingEffect },
              { ImgSingalOperations.USM, ImageOperateMethods.USM },
              { ImgSingalOperations.AutoWhithBalance, ImageOperateMethods.AutoWhithBalance },
              { ImgSingalOperations.Buffing, ImageOperateMethods.Buffing },
              { ImgSingalOperations.Whitening, ImageOperateMethods.Whitening },
              { ImgSingalOperations.LowIlluminationEnhance, ImageOperateMethods.LowIlluminationEnhance },
              { ImgSingalOperations.Transpose, ImageOperateMethods.Transpose },
        };

        public Mat Execute(ImgSingalOperations operation, Mat src)
        {
            if (_operations.TryGetValue(operation, out var func))
            {
                return func(src);
            }
            throw new ArgumentException($"未注册改方法类型:{operation}");
        }


        #endregion
    }
}
