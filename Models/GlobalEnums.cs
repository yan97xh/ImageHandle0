using System.ComponentModel;

namespace ImageHandle.Models
{
    public enum PageEnum
    {
        [Description("主界面")]
        MainPage = 0,

        [Description("算数运算")]
        ImgCalPage,

        [Description("特征匹配")]
        FeatureMatchPage,

        [Description("特征检测")]
        FeatureDetectPage,

        [Description("形态学操作")]
        MorphologicalPage,

        [Description("文本操作")]
        TextPage,

        [Description("水漫操作")]
        FloodFillPage,

        [Description("马赛克")]
        MosaicPage,

        [Description("人脸识别")]
        FaceRecognitionPage,

        [Description("矩形抠图")]
        ROIDetectPage,

        [Description("多边形抠图")]
        PolyROIDetectPage,

        [Description("矩形插图")]
        ROIInsertPage,

        [Description("模版匹配")]
        TemplateMatchPage,

        [Description("脚本操作")]
        ScriptPage,
    }

    /// <summary>
    /// 图像单操作 无参数
    /// </summary>
    public enum ImgOpNoneParamEnum
    {
        /// <summary>
        /// 转为灰度图像
        /// </summary>
        CvtColorToGray,

        CvtColorToHSV,
        CvtColorToLab,
        CvtColorToRGB,
        Otsu,

        /// <summary>
        /// 图像取反
        /// </summary>
        Negation,

        /// <summary>
        /// 直方图均衡化
        /// </summary>
        EqualizeHist,

        RetroEffect,
        FusedCastEffect,
        FrozenEffect,
        ComicEffect,
        FleetingEffect,
        USM,

        /// <summary>
        /// 自动白平衡
        /// </summary>
        AutoWhithBalance,

        /// <summary>
        /// 磨皮
        /// </summary>
        Buffing,

        /// <summary>
        /// 亮度增强
        /// </summary>
        Whitening,

        /// <summary>
        /// 低照度增强
        /// </summary>
        LowIlluminationEnhance,

        /// <summary>
        /// 转置
        /// </summary>
        Transpose,

        Smooth,

        /// <summary>
        /// 小波变化
        /// </summary>
        WaveletTransform,

        /// <summary>
        /// 可分离线性滤波
        /// </summary>
        SepFilter2D,

        /// <summary>
        /// 仿射变换 重映射
        /// </summary>
        Remaping,

        /// <summary>
        /// 放射变换 透视变换
        /// </summary>
        Perspect,

        /// <summary>
        ///  放射变换 剪切
        /// </summary>
        Shear,

        /// <summary>
        /// 直线矫正
        /// </summary>
        LineCorrect,

        DCT,

        /// <summary>
        /// 轮廓检测
        /// </summary>
        Contours,

        /// <summary>
        /// 凸包
        /// </summary>
        Hull,

        /// <summary>
        /// 凸包缺陷检测
        /// </summary>
        ConvexityDefects,

        DNN_Caffe,
        DNN_TensorFlow,
    }

    /// <summary>
    /// 图像单操作 一个参数  数字
    /// </summary>
    public enum ImgOpOneParamNumEnum
    {
        /// <summary>
        /// 素描
        /// </summary>
        Sketch,

        /// <summary>
        /// 浮雕
        /// </summary>
        Emboss,

        /// <summary>
        /// 快速去雾
        /// </summary>
        FastDehazing,

        /// <summary>
        /// 毛玻璃
        /// </summary>
        GroundGlass,

        /// <summary>
        /// 透明度
        /// </summary>
        Lucency,

        /// <summary>
        /// 加盐噪声
        /// </summary>
        AddSaltNoise,

        /// <summary>
        /// 加椒噪声
        /// </summary>
        AddPepperNoise,

        /// <summary>
        /// 加椒盐噪声
        /// </summary>
        AddSaltPepperNoise,

        /// <summary>
        /// 加高斯噪声
        /// </summary>
        AddGaussianNoise,

        /// <summary>
        /// 均值滤波
        /// </summary>
        Blur,

        /// <summary>
        /// 中值滤波
        /// </summary>
        MedianBlur,

        /// <summary>
        /// 双边滤波
        /// </summary>
        BilateralFilter,

        /// <summary>
        /// 高斯滤波
        /// </summary>
        GaussianBlur,

        /// <summary>
        /// 方盒滤波
        /// </summary>
        BoxFilter,

        PyrDown,
        PyrUp,
        BoundingRect,
        BoundingCircle,
    }

    /// <summary>
    /// 图像单操作 一个参数  枚举
    /// </summary>
    public enum ImgOpOneParamEnumEnum
    {
        Flip,
        ApplyColorMap,
        Edge,

        /// <summary>
        /// 特征检测
        /// </summary>
        FeatureRecogn,

        /// <summary>
        /// 皮肤检测
        /// </summary>
        SkinDefect,

        /// <summary>
        /// 图像斜折叠
        /// </summary>
        CompleteSymm,

        BGRSingle,
    }

    /// <summary>
    /// 边缘检测方式
    /// </summary>
    public enum EdegTypeEnum
    {
        Canny = 0,
        Sobel = 1,
        Sobel_X = 2,
        Sobel_Y = 3,
        Laplacian = 4,
        Scharr = 5,
        Scharr_X = 6,
        Scharr_Y = 7,
    }

    /// <summary>
    /// 人脸识别 分类器类型
    /// </summary>
    public enum ClassifierEnum
    {
        Haarcascades_ALL,
        Haarcascades_EyeGlass,
        Haarcascades_Nose,
        Haarcascades_Mouth,
        Lbpcascade_Face
    }

    /// <summary>
    /// 皮肤检测类型
    /// </summary>
    public enum SkinDefectEnum
    {
        BGR,
        HSV_Range,
        YCrCb_Range,
        YCrCb_Thresh,
        Ellipse,
    }

    /// <summary>
    /// 折叠方向
    /// </summary>
    public enum SymmDirection
    {
        UpperToLower,
        LowerToUpper,
    }

    public enum BGREnum
    {
        B,
        G,
        R
    }

    /// <summary>
    ///  图像计算   两张图像
    /// </summary>
    public enum ImgCalOpTowMatEnum
    {
        Add,

        AddWeight,
        AbsDiff,
        SubA,
        SubB,
        And,
        Or,
        Xor,
    }

    /// <summary>
    ///  图像计算   一张图像
    /// </summary>
    public enum ImgCalOpOneMatEnum
    {
        Not,
        Exp,
        Log,
        Pow,
        Abs
    }

    /// <summary>
    ///
    /// </summary>
    public enum MatOpName
    {
        SrcImg1,
        SrcImg2
    }

    /// <summary>
    /// 图像相似度比较
    /// </summary>
    public enum MatSimilarityEnum
    {
        AHash,
        DHash,
        PHash,
        SSIM,
        PSNR
    }

    /// <summary>
    /// 图像拼接方式
    /// </summary>
    public enum ConcatEnum
    {
        Stitch,
        HConcat,
        VConcat
    }

    /// <summary>
    /// 两张图像操作   参数是枚举
    /// </summary>
    public enum TwoMatOpEnum
    {
        /// <summary>
        /// 图像拼接
        /// </summary>
        Concat,
    }

    /// <summary>
    /// 特征匹配模式
    /// </summary>
    public enum FeatureMatchMode
    {
        SIFT,
        SURF
    }

    /// <summary>
    /// 匹配器类型
    /// </summary>
    public enum MatcherMode
    {
        /// <summary>
        /// 暴力匹配器
        /// </summary>
        BFMatcher,

        /// <summary>
        /// Flann匹配器
        /// </summary>
        FlannMatcher
    }

    /// <summary>
    /// 特征检测模式
    /// </summary>
    public enum FeatureDetectMode
    {
        HoughCircle,
        HoughLine,
        Corner,

        /// <summary>
        /// 亚像素矩阵
        /// </summary>
        RectSubPix,
    }

    /// <summary>
    /// 特征点提取方式
    /// </summary>
    public enum FeatureDetectPointMode
    {
        SIFT = 0,
        SURF = 1,
        Star = 2,
        ORB_FERAK = 3,
        BRISK = 4,
        MSER = 5,
        GFTT = 6,
    }

    public enum MorphologicalMode
    {
        /// <summary>
        /// 形态学变化
        /// </summary>
        MorphologyEx,

        /// <summary>
        /// MeanShift分割
        /// </summary>
        MeanShiftFilter,

        /// <summary>
        /// 堆叠
        /// </summary>
        Repeat,

        /// <summary>
        /// 距离变换
        /// </summary>
        DistanceTranForm,

        /// <summary>
        /// 极空间
        /// </summary>
        Polar,
    }

    public enum PolarMode
    {
        LinerPolar,
        LogPolar,
    }

    public enum NumberParamTypeEnum
    {
        Integer,
        Double,
    }
}