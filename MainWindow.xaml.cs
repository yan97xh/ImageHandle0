using ImageHandle.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ImageHandle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _mainViewModel = new MainViewModel();
            DataContext = _mainViewModel;

            //设置按钮初始图标
            maxBtn.Content = CreateIconTextBlock("&#xe651;");
        }

        #region 标题栏按钮点击事件

        private void minBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void maxBtn_Click(object sender, RoutedEventArgs e)
        {
            //if (this.WindowState != WindowState.Maximized)
            //{
            //    this.WindowState = WindowState.Maximized;
            //    maxBtn.Content = CreateIconTextBlock("&#xe601;");
            //}
            //else
            //{
            //    this.WindowState = WindowState.Normal;
            //    maxBtn.Content = CreateIconTextBlock("&#xe651;");
            //}
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        // 转换 HTML 实体为 Unicode 字符的方法
        private string ConvertHtmlEntityToUnicode(string htmlEntity)
        {
            // 去除 &#x 和 ; 获取十六进制值
            string hexValue = htmlEntity.Replace("&#x", "").Replace(";", "");
            // 将十六进制转换为 Unicode 字符
            int unicodeValue = Convert.ToInt32(hexValue, 16);
            return char.ConvertFromUtf32(unicodeValue);
        }

        private TextBlock CreateIconTextBlock(string htmlEntity)
        {
            string unicodeChar = ConvertHtmlEntityToUnicode(htmlEntity);

            return new TextBlock
            {
                Text = unicodeChar,
                // 使用按钮当前的 FontFamily，这样会继承 Style 中的设置
                FontFamily = maxBtn.FontFamily,
                FontSize = maxBtn.FontSize,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }

        #endregion 标题栏按钮点击事件
    }
}