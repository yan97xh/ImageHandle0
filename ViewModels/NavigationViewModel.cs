using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageHandle.Attributes;
using ImageHandle.Commands;
using ImageHandle.Models;
using ImageHandle.Services;
using System.Reflection;

namespace ImageHandle.ViewModels
{
    public partial class NavigationViewModel : ObservableObject
    {
        public NavigationViewModel()
        {
            // 注册页面
            SimpleNavigationService? simpleNavigationService = SimpleNavigationService.Instance;
            var controls = Assembly.GetExecutingAssembly()
                                             .GetTypes()
                                             .Where(t => t.IsClass && !t.IsAbstract &&
                                                             t.IsSubclassOf(typeof(System.Windows.Controls.UserControl)) && // 继承自 UserControl
                                                             t.GetCustomAttribute<NavigationPageAttribute>() != null) // 有指定特性
                                                             .ToList();
            var methodInfo = typeof(SimpleNavigationService).GetMethod("Register");

            foreach (var control in controls)// 一定会继承 UserControl 并且有 NavigationPageAttribute 特性
            {
                var attr = control.GetCustomAttribute<NavigationPageAttribute>();
                var genericMethod = methodInfo.MakeGenericMethod(control);
                genericMethod.Invoke(simpleNavigationService, new object[] { attr.PageEnum });
            }

            //设置默认界面
            CurrentPage = SimpleNavigationService.Instance.Navigate(PageEnum.MainPage);
        }

        [ObservableProperty]
        private object _currentPage;

        /// <summary>
        /// 切换界面
        /// </summary>
        /// <param name="dstPage"></param>
        [RelayCommand]
        private void Navigate(PageEnum dstPage)
        {
            // 用属性赋值
            CurrentPage = SimpleNavigationService.Instance.Navigate(dstPage);
        }
    }
}