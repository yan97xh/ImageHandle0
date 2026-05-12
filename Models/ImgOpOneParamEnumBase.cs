using ImageHandle.ViewModels;
using OpenCvSharp;

namespace ImageHandle.Models
{
    public abstract class ImgOpOneParamEnumBase : ViewModelBase
    {
        public string ButtonText { get; protected set; }
        public string LabelText { get; protected set; }
        public ImgOpOneParamEnumEnum OperationType { get; protected set; }

        public string ParamToolText { get; protected set; }

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

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        public abstract Mat Execute(Mat srcImg);

        /// <summary>
        /// 检查格式是否正确
        /// </summary>
        /// <returns></returns>
        public abstract bool CheckParamFormat();

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

        public Type EnumType
        {
            get; set;
        }
    }
}