namespace ImageHandle.ViewModels
{
    /// <summary>
    /// 主要的VM
    /// </summary>
    internal class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
             NavigationViewModel = new NavigationViewModel(); 
        }

        // 多界面切换的VM
        public NavigationViewModel NavigationViewModel { get; set; }
    }
}