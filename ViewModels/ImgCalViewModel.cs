using ImageHandle.Helpers;
using ImageHandle.Models;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ImageHandle.ViewModels
{
    /// <summary>
    /// 图像计算类的 vm
    /// </summary>
    public class ImgCalViewModel : ViewModelBase
    {
        public ImgCalViewModel()
        {
            ImageArr = Enum.GetNames(typeof(MatOpName));

            // 两张图
            TwoMatOpCommand = new Commands.Command<ImgCalOpTowMatEnum>(ProcessImageTwoMat);
            InitImgOpTwoMat();

            // 第一张图
            OneMatOpCommand1 = new Commands.Command<ImgCalOpOneMatEnum>(ProcessImageOneMat1);
            InitImgOpOneMat1();

            // 第二张图
            OneMatOpCommand2 = new Commands.Command<ImgCalOpOneMatEnum>(ProcessImageOneMat2);
            InitImgOpOneMat2();

            //图像相似度比较
            SimilarityCommand = new Commands.Command<MatSimilarityEnum>(ProcessSimilarityOperation);
            InitSimilarityOperations();

            //图像拼接
            TwoMatOneEnumCommand = new Commands.Command<TwoMatOpEnum>(ProcessConcatOperation);
            InitTwoMatOneEnumOps();
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

        #region 两张图像算数运算

        private bool _setSameSizeType;

        //是否强制转换大小类型 ， 两张图在做计算的时候 需要大小和类型一样
        public bool SetSameSizeType
        {
            get { return _setSameSizeType; }
            set
            {
                _setSameSizeType = value;
                OnPropertyChanged();
            }
        }

        public string[] ImageArr
        {
            get;
        }

        private string _dstImageSizeType;

        // 转换的目标图像  1 还是 2  ,最后的大小
        public string DstImageSizeType
        {
            get { return _dstImageSizeType; }
            set { _dstImageSizeType = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 图像操作集合 一个参数 数值类
        /// </summary>
        public ObservableCollection<ImgCalOperationTwoMat> OperationsTwoMat { get; } = new();

        private void InitImgOpTwoMat()
        {
            OperationsTwoMat.Add(new ImgCalOperationTwoMat()
            {
                DisplayName = "Add",
                operationEnum = ImgCalOpTowMatEnum.Add,
                Remark = "srcImg1 + srcImg2",
            });
            OperationsTwoMat.Add(new ImgCalOperationTwoMat()
            {
                DisplayName = "AddWeight",
                operationEnum = ImgCalOpTowMatEnum.AddWeight,
                ParamList = new List<ParamValueModel>()
                {
                  new ParamValueModel("alpha","1.0","a"),
                  new ParamValueModel("beta","1.0","b"),
                  new ParamValueModel("gamma","0","c"),
                },
                //Remark = "alpha * srcImg1 + beta * srcImg2 + gamma",
                Remark = "a * srcImg1 + b * srcImg2 + c",
            });
            OperationsTwoMat.Add(new ImgCalOperationTwoMat()
            {
                DisplayName = "SubA",
                operationEnum = ImgCalOpTowMatEnum.SubA,
                Remark = "srcImg1 - srcImg2",
            });
            OperationsTwoMat.Add(new ImgCalOperationTwoMat()
            {
                DisplayName = "SubB",
                operationEnum = ImgCalOpTowMatEnum.SubB,
                Remark = "srcImg2 - srcImg1",
            });
            OperationsTwoMat.Add(new ImgCalOperationTwoMat()
            {
                DisplayName = "Or",
                operationEnum = ImgCalOpTowMatEnum.Or,
                Remark = "srcImg1 ∪ srcImg2",
            });
            OperationsTwoMat.Add(new ImgCalOperationTwoMat()
            {
                DisplayName = "And",
                operationEnum = ImgCalOpTowMatEnum.And,
                Remark = "srcImg1 ∩ srcImg2",
            });
            OperationsTwoMat.Add(new ImgCalOperationTwoMat()
            {
                DisplayName = "AbsDiff",
                operationEnum = ImgCalOpTowMatEnum.AbsDiff,
                Remark = "| srcImg1 - srcImg2 |",
            });
            OperationsTwoMat.Add(new ImgCalOperationTwoMat()
            {
                DisplayName = "Xor",
                operationEnum = ImgCalOpTowMatEnum.Xor,
                Remark = "srcImg1 ^ srcImg2",
            });
        }

        public ICommand TwoMatOpCommand { get; }

        /// <summary>
        /// 图像处理 一个参数方法 数值类
        /// </summary>
        /// <param name="operation"></param>
        private void ProcessImageTwoMat(ImgCalOpTowMatEnum operation)
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

            if (SetSameSizeType == true)
            {
                if (Enum.TryParse(typeof(MatOpName), _dstImageSizeType, true, out object result) == false)
                {
                    MessageBox.Show($"转换后大小选择有误");
                    return;
                }
                MatOpName matOpName = (MatOpName)result;
                switch (matOpName)
                {
                    case MatOpName.SrcImg1:
                        {
                            srcImg2 = srcImg2.Resize(srcImg1.Size());
                            srcImg2.ConvertTo(srcImg2, srcImg1.Type());
                        }
                        break;

                    case MatOpName.SrcImg2:
                        {
                            srcImg1 = srcImg1.Resize(srcImg2.Size());
                            srcImg1.ConvertTo(srcImg1, srcImg2.Type());
                        }
                        break;
                }
            }

            if (srcImg1.Size() != srcImg2.Size() || srcImg1.Type() != srcImg2.Type())
            {
                MessageBox.Show($"两张图像大小类型不同");
                return;
            }
            switch (operation)
            {
                case ImgCalOpTowMatEnum.Add:
                    {
                        DstMat = ImageOperateMethods.Add(srcImg1, srcImg2);
                    }
                    break;

                case ImgCalOpTowMatEnum.AddWeight:
                    {
                        ImgCalOperationTwoMat op = OperationsTwoMat.First(x => x.operationEnum == operation);
                        if (op == null)
                        {
                            MessageBox.Show($"方法:{operation.ToString()}未注册");
                            return;
                        }
                        string alpha = op.ParamList.First(x => x.ParamName == "alpha")?.ParamValue;
                        if (double.TryParse(alpha, out double alphaDouble) == false)
                        {
                            MessageBox.Show($"参数alpha输入有误");
                            return;
                        }
                        string beta = op.ParamList.First(x => x.ParamName == "beta")?.ParamValue;
                        if (double.TryParse(beta, out double betaDouble) == false)
                        {
                            MessageBox.Show($"参数beta输入有误");
                            return;
                        }
                        string gamma = op.ParamList.First(x => x.ParamName == "alpha")?.ParamValue;
                        if (double.TryParse(gamma, out double gammaDouble) == false)
                        {
                            MessageBox.Show($"参数gamma输入有误");
                            return;
                        }
                        DstMat = ImageOperateMethods.AddWeighted(srcImg1, srcImg2, alphaDouble, betaDouble, gammaDouble);
                    }
                    break;

                case ImgCalOpTowMatEnum.AbsDiff:
                    {
                        DstMat = ImageOperateMethods.AbsDiff(srcImg1, srcImg2);
                    }
                    break;

                case ImgCalOpTowMatEnum.SubA:
                    {
                        DstMat = ImageOperateMethods.Subtract(srcImg1, srcImg2);
                    }
                    break;

                case ImgCalOpTowMatEnum.SubB:
                    {
                        DstMat = ImageOperateMethods.AbsDiff(srcImg2, srcImg1);
                    }
                    break;

                case ImgCalOpTowMatEnum.And:
                    {
                        DstMat = ImageOperateMethods.BitwiseAnd(srcImg1, srcImg2);
                    }
                    break;

                case ImgCalOpTowMatEnum.Or:
                    {
                        DstMat = ImageOperateMethods.BitwiseOr(srcImg1, srcImg2);
                    }
                    break;

                case ImgCalOpTowMatEnum.Xor:
                    {
                        DstMat = ImageOperateMethods.BitwiseXor(srcImg1, srcImg2);
                    }
                    break;
            }
        }

        #endregion 两张图像算数运算

        #region 第一张图的计算

        public ObservableCollection<ImgCalOperationOneMat> OperationsOneMat1 { get; } = new();

        private void InitImgOpOneMat1()
        {
            OperationsOneMat1.Add(new ImgCalOperationOneMat()
            {
                DisplayName = "BitwiseNot1",
                MatName = MatOpName.SrcImg1,
                operationEnum = ImgCalOpOneMatEnum.Not,
                Remark = "~srcImg1"
            });
            OperationsOneMat1.Add(new ImgCalOperationOneMat()
            {
                DisplayName = "Abs1",
                MatName = MatOpName.SrcImg1,
                operationEnum = ImgCalOpOneMatEnum.Abs,
                Remark = "|srcImg1|"
            });
            OperationsOneMat1.Add(new ImgCalOperationOneMat()
            {
                DisplayName = "Exp1",
                MatName = MatOpName.SrcImg1,
                operationEnum = ImgCalOpOneMatEnum.Exp,
                Remark = "e^srcImg1  e=2.71828"
            });
            OperationsOneMat1.Add(new ImgCalOperationOneMat()
            {
                DisplayName = "Log1",
                MatName = MatOpName.SrcImg1,
                operationEnum = ImgCalOpOneMatEnum.Log,
                Remark = "ln(srcImg1)"
            });
            OperationsOneMat1.Add(new ImgCalOperationOneMat()
            {
                DisplayName = "Pow1",
                MatName = MatOpName.SrcImg1,
                operationEnum = ImgCalOpOneMatEnum.Pow,
                Remark = "srcImg1^p ; p**srcImg1",
                ParamList = new List<ParamValueModel>()
                {
                    new ParamValueModel("power","1","p")
                }
            });
        }

        public ICommand OneMatOpCommand1 { get; }

        private void ProcessImageOneMat1(ImgCalOpOneMatEnum operation)
        {
            if (string.IsNullOrEmpty(_srcImagePath1))
            {
                MessageBox.Show("未选择输入图像1");
                return;
            }
            if (File.Exists(_srcImagePath1) == false)
            {
                MessageBox.Show($"图像:{_srcImagePath1}不存在");
                return;
            }
            Mat srcImg1 = new Mat(_srcImagePath1);
            switch (operation)
            {
                case ImgCalOpOneMatEnum.Not:
                    {
                        DstMat = ImageOperateMethods.BitwiseNot(srcImg1);
                    }
                    break;

                case ImgCalOpOneMatEnum.Exp:
                    {
                        DstMat = ImageOperateMethods.Exp(srcImg1);
                    }
                    break;

                case ImgCalOpOneMatEnum.Log:
                    {
                        DstMat = ImageOperateMethods.Log(srcImg1);
                    }
                    break;

                case ImgCalOpOneMatEnum.Pow:
                    {
                        ImgCalOperationOneMat op = OperationsOneMat1.First(x => x.operationEnum == operation);
                        if (op == null)
                        {
                            MessageBox.Show($"方法:{operation.ToString()}未注册");
                            return;
                        }
                        string power = op.ParamList.First(x => x.ParamName == "power")?.ParamValue;
                        if (double.TryParse(power, out double powerDouble) == false)
                        {
                            MessageBox.Show($"参数power输入有误");
                            return;
                        }
                        DstMat = ImageOperateMethods.Power(srcImg1, powerDouble);
                    }
                    break;

                case ImgCalOpOneMatEnum.Abs:
                    {
                        DstMat = ImageOperateMethods.Abs(srcImg1);
                    }
                    break;
            }
        }

        #endregion 第一张图的计算

        #region 第二张图的计算

        public ObservableCollection<ImgCalOperationOneMat> OperationsOneMat2 { get; } = new();

        private void InitImgOpOneMat2()
        {
            OperationsOneMat2.Add(new ImgCalOperationOneMat()
            {
                DisplayName = "BitwiseNot2",
                MatName = MatOpName.SrcImg2,
                operationEnum = ImgCalOpOneMatEnum.Not,
                Remark = "~srcImg2"
            });
            OperationsOneMat2.Add(new ImgCalOperationOneMat()
            {
                DisplayName = "Abs2",
                MatName = MatOpName.SrcImg2,
                operationEnum = ImgCalOpOneMatEnum.Abs,
                Remark = "|srcImg2|"
            });
            OperationsOneMat2.Add(new ImgCalOperationOneMat()
            {
                DisplayName = "Exp2",
                MatName = MatOpName.SrcImg2,
                operationEnum = ImgCalOpOneMatEnum.Exp,
                Remark = "e^srcImg2  e=2.71828"
            });
            OperationsOneMat2.Add(new ImgCalOperationOneMat()
            {
                DisplayName = "Log2",
                MatName = MatOpName.SrcImg2,
                operationEnum = ImgCalOpOneMatEnum.Log,
                Remark = "ln(srcImg2)"
            });
            OperationsOneMat2.Add(new ImgCalOperationOneMat()
            {
                DisplayName = "Pow2",
                MatName = MatOpName.SrcImg2,
                operationEnum = ImgCalOpOneMatEnum.Pow,
                Remark = "srcImg2^p ; p**srcImg2",
                ParamList = new List<ParamValueModel>()
                {
                    new ParamValueModel("power" ,"1","p")
                }
            });
        }

        public ICommand OneMatOpCommand2 { get; }

        private void ProcessImageOneMat2(ImgCalOpOneMatEnum operation)
        {
            if (string.IsNullOrEmpty(_srcImagePath2))
            {
                MessageBox.Show("未选择输入图像2");
                return;
            }
            if (File.Exists(_srcImagePath2) == false)
            {
                MessageBox.Show($"图像:{_srcImagePath2}不存在");
                return;
            }
            Mat srcImg2 = new Mat(_srcImagePath2);
            switch (operation)
            {
                case ImgCalOpOneMatEnum.Not:
                    {
                        DstMat = ImageOperateMethods.BitwiseNot(srcImg2);
                    }
                    break;

                case ImgCalOpOneMatEnum.Exp:
                    {
                        DstMat = ImageOperateMethods.Exp(srcImg2);
                    }
                    break;

                case ImgCalOpOneMatEnum.Log:
                    {
                        DstMat = ImageOperateMethods.Log(srcImg2);
                    }
                    break;

                case ImgCalOpOneMatEnum.Pow:
                    {
                        ImgCalOperationOneMat op = OperationsOneMat2.First(x => x.operationEnum == operation);
                        if (op == null)
                        {
                            MessageBox.Show($"方法:{operation.ToString()}未注册");
                            return;
                        }
                        string power = op.ParamList.First(x => x.ParamName == "power")?.ParamValue;
                        if (double.TryParse(power, out double powerDouble) == false)
                        {
                            MessageBox.Show($"参数power输入有误");
                            return;
                        }
                        DstMat = ImageOperateMethods.Power(srcImg2, powerDouble);
                    }
                    break;

                case ImgCalOpOneMatEnum.Abs:
                    {
                        DstMat = ImageOperateMethods.Abs(srcImg2);
                    }
                    break;
            }
        }

        #endregion 第二张图的计算

        #region 图像相似度比较

        private double _similarity;

        public double Similarity
        {
            get { return _similarity; }
            set
            {
                _similarity = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ImgSimilarityOperation> SimilarityOperations { get; } = new();

        private void InitSimilarityOperations()
        {
            SimilarityOperations.Add(new ImgSimilarityOperation("均值哈希", MatSimilarityEnum.AHash, ImageOperateMethods.AHash));
            SimilarityOperations.Add(new ImgSimilarityOperation("差值哈希", MatSimilarityEnum.DHash, ImageOperateMethods.DHash));
            SimilarityOperations.Add(new ImgSimilarityOperation("感知哈希", MatSimilarityEnum.PHash, ImageOperateMethods.PHash));
            SimilarityOperations.Add(new ImgSimilarityOperation("SSIM", MatSimilarityEnum.SSIM, ImageOperateMethods.SSIM));
            SimilarityOperations.Add(new ImgSimilarityOperation("PSNR", MatSimilarityEnum.PSNR, ImageOperateMethods.PSNR, "峰值信噪比"));
        }

        public ICommand SimilarityCommand { get; }

        private void ProcessSimilarityOperation(MatSimilarityEnum similarityEnum)
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

            if (SetSameSizeType == true)
            {
                if (Enum.TryParse(typeof(MatOpName), _dstImageSizeType, true, out object result) == false)
                {
                    MessageBox.Show($"转换后大小选择有误");
                    return;
                }
                MatOpName matOpName = (MatOpName)result;
                switch (matOpName)
                {
                    case MatOpName.SrcImg1:
                        {
                            srcImg2 = srcImg2.Resize(srcImg1.Size());
                            srcImg2.ConvertTo(srcImg2, srcImg1.Type());
                        }
                        break;

                    case MatOpName.SrcImg2:
                        {
                            srcImg1 = srcImg1.Resize(srcImg2.Size());
                            srcImg1.ConvertTo(srcImg1, srcImg2.Type());
                        }
                        break;
                }
            }

            if (srcImg1.Size() != srcImg2.Size() || srcImg1.Type() != srcImg2.Type())
            {
                MessageBox.Show($"两张图像大小类型不同");
                return;
            }

            ImgSimilarityOperation operation = SimilarityOperations.First(x => x.similarityOperation == similarityEnum);
            if (operation == null)
            {
                MessageBox.Show($"方法:{similarityEnum}未注册");
                return;
            }

            Similarity = operation.func(srcImg1, srcImg2);
        }

        #endregion 图像相似度比较

        #region 两张图像 一个枚举参数

        public ObservableCollection<TwoMatOpEnumBase> TwoMatOneEnumOps { get; } = new();

        private void InitTwoMatOneEnumOps()
        {
            TwoMatOneEnumOps.Add(new TwoMatOpEnumClass<ConcatEnum>("图像拼接", "拼接方式", TwoMatOpEnum.Concat, ImageOperateMethods.Concat));
        }

        public ICommand TwoMatOneEnumCommand { get; }

        private void ProcessConcatOperation(TwoMatOpEnum operation)
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

            if (SetSameSizeType == true)
            {
                if (Enum.TryParse(typeof(MatOpName), _dstImageSizeType, true, out object result) == false)
                {
                    MessageBox.Show($"转换后大小选择有误");
                    return;
                }
                MatOpName matOpName = (MatOpName)result;
                switch (matOpName)
                {
                    case MatOpName.SrcImg1:
                        {
                            srcImg2 = srcImg2.Resize(srcImg1.Size());
                            srcImg2.ConvertTo(srcImg2, srcImg1.Type());
                        }
                        break;

                    case MatOpName.SrcImg2:
                        {
                            srcImg1 = srcImg1.Resize(srcImg2.Size());
                            srcImg1.ConvertTo(srcImg1, srcImg2.Type());
                        }
                        break;
                }
            }

            if (srcImg1.Size() != srcImg2.Size() || srcImg1.Type() != srcImg2.Type())
            {
                MessageBox.Show($"两张图像大小类型不同");
                return;
            }
            TwoMatOpEnumBase operate = TwoMatOneEnumOps.First(x => x.OperationType == operation);
            if (operate == null)
            {
                MessageBox.Show($"方法:{operation}未注册");
                return;
            }
            DstMat = operate.Execute(srcImg1, srcImg2);
        }

        #endregion 两张图像 一个枚举参数
    }

    /// <summary>
    /// 两张图像的操作类型
    /// </summary>
    public class ImgCalOperationTwoMat
    {
        public string DisplayName { get; set; }
        public List<ParamValueModel> ParamList { get; set; } = new List<ParamValueModel>();

        public ImgCalOpTowMatEnum operationEnum { get; set; }

        public string Remark { get; set; }
    }

    /// <summary>
    /// 一张图像的计算类型  A 或者 B
    /// </summary>
    public class ImgCalOperationOneMat
    {
        public ImgCalOperationOneMat()
        {
        }

        public ImgCalOperationOneMat(string displayName, List<ParamValueModel> paramList, ImgCalOpOneMatEnum operationEnum, MatOpName matType)
        {
            DisplayName = displayName;
            ParamList = paramList;
            this.operationEnum = operationEnum;
            MatName = matType;
        }

        public string DisplayName { get; set; }
        public List<ParamValueModel> ParamList { get; set; } = new List<ParamValueModel>();

        public ImgCalOpOneMatEnum operationEnum { get; set; }

        public MatOpName MatName { get; set; }

        public string Remark { get; set; }
    }

    public class ImgSimilarityOperation
    {
        public ImgSimilarityOperation()
        {
        }

        public ImgSimilarityOperation(string displayName, MatSimilarityEnum similarityOperation, Func<Mat, Mat, double> func, string reamrk = "")
        {
            DisplayName = displayName;
            this.similarityOperation = similarityOperation;
            this.func = func;
            Reamrk = reamrk;
        }

        public string DisplayName { get; set; }
        public MatSimilarityEnum similarityOperation { get; set; }
        public Func<Mat, Mat, double> func { get; set; }

        public string Reamrk { get; set; }
    }

    #region 两张图像 一个参数 枚举类型

    // 泛型类继承基类
    public class TwoMatOpEnumClass<T> : TwoMatOpEnumBase where T : Enum
    {
        public TwoMatOpEnumClass()
        {
        }

        public TwoMatOpEnumClass(string buttonText, string labelText, TwoMatOpEnum operationType, Func<Mat, Mat, T, Mat> func, string toolTips = "", string defeatValue = "0")
            : base()
        {
            ButtonText = buttonText;
            LabelText = labelText;
            Func = func;
            OperationType = operationType;
            InputParam = defeatValue;
            ParamToolText = toolTips;
            ItemsArray = Enum.GetNames(typeof(T));
        }

        public Func<Mat, Mat, T, Mat> Func { get; }

        public override bool CheckParamFormat()
        {
            int index = Array.IndexOf(ItemsArray, InputParam);
            return index >= 0;
        }

        public override Mat Execute(Mat srcImg1, Mat srcImg2)
        {
            T para = ConvertInputValue(InputParam);
            return Func(srcImg1, srcImg2, para);
        }

        private T ConvertInputValue(string input)
        {
            if (Enum.TryParse(typeof(T), input, true, out object result))
            {
                return (T)result;
            }
            //throw new ArgumentException($"无法将 '{input}' 转换为 {typeof(T).Name}");
            return default;
        }
    }

    #endregion 两张图像 一个参数 枚举类型
}