using ImageHandle.Commands;
using ImageHandle.Models;
using ImageHandle.Scripts;
using ImageHandle.Services;
using ImageHandle.UserControls;
using System.Windows.Input;

namespace ImageHandle.ViewModels
{
    public class NavigationViewModel : ViewModelBase
    {
        public NavigationViewModel()
        {
            NavigateCommand = new Command<PageEnum>(Navigate);

            // 注册页面
            SimpleNavigationService? simpleNavigationService = SimpleNavigationService.Instance;
            simpleNavigationService.Register<MainUserControl>(PageEnum.MainPage);
            simpleNavigationService.Register<ImgCalUserControl>(PageEnum.ImgCalPage);
            simpleNavigationService.Register<FeatureMatchUserControl>(PageEnum.FeatureMatchPage);
            simpleNavigationService.Register<FeatureDetectUserControl>(PageEnum.FeatureDetectPage);
            simpleNavigationService.Register<MorphologicalOperationsControl>(PageEnum.MorphologicalPage);
            simpleNavigationService.Register<TextUserControl>(PageEnum.TextPage);
            simpleNavigationService.Register<FloodFillUserControl>(PageEnum.FloodFillPage);
            simpleNavigationService.Register<MosaicUserControl>(PageEnum.MosaicPage);
            simpleNavigationService.Register<FaceRecognitionUserControl>(PageEnum.FaceRecognitionPage);
            simpleNavigationService.Register<ROIDetectUserControl>(PageEnum.ROIDetectPage);
            simpleNavigationService.Register<PolygonROIDetectUserControl>(PageEnum.PolyROIDetectPage);
            simpleNavigationService.Register<ROIInsertUserControl>(PageEnum.ROIInsertPage);
            simpleNavigationService.Register<TemplateMatchUserControl>(PageEnum.TemplateMatchPage);
            simpleNavigationService.Register<ScriptMainUserControl>(PageEnum.ScriptPage);

            //设置默认界面
            CurrentPage = SimpleNavigationService.Instance.Navigate(PageEnum.MainPage);
        }

        private object _currentPage;

        /// <summary>
        /// 当前页面
        /// </summary>
        public object CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 切换界面 命令
        /// </summary>
        public ICommand NavigateCommand
        {
            get;
        }

        /// <summary>
        /// 切换界面
        /// </summary>
        /// <param name="dstPage"></param>
        private void Navigate(PageEnum dstPage)
        {
            // 用属性赋值
            CurrentPage = SimpleNavigationService.Instance.Navigate(dstPage);
        }
    }
}