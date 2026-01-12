using ImageHandle.Models;
using System.Windows.Controls;

namespace ImageHandle.Services
{
    /// <summary>
    /// 页面导航注册服务类
    /// </summary>
    public class SimpleNavigationService
    {
        #region 懒加载实现单例

        private static readonly Lazy<SimpleNavigationService> _instance = new Lazy<SimpleNavigationService>(() => new SimpleNavigationService());

        public static SimpleNavigationService Instance => _instance.Value;

        private SimpleNavigationService()
        {
        }

        #endregion 懒加载实现单例

        private readonly Dictionary<PageEnum, UserControl> _instances = new Dictionary<PageEnum, UserControl>();

        /// <summary>
        /// 注册界面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        public void Register<T>(PageEnum key) where T : UserControl, new()
        {
            _instances[key] = new T();
        }

        public UserControl Navigate(PageEnum key)
        {
            if (_instances.TryGetValue(key, out UserControl instance))
            {
                return instance;
            }

            throw new ArgumentException($"页面未注册: {key}");
        }
    }
}