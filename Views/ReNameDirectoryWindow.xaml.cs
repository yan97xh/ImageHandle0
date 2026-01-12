using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace ImageHandle.Views
{
    /// <summary>
    /// ReNameDirectoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ReNameDirectoryWindow : Window
    {
        private string _oriDir = "";
        private string _folderName = "";
        private string _parentPath = "";
        private string _regexStr = "";

        public ReNameDirectoryWindow(string oriDir, string regexStr = "")
        {
            InitializeComponent();
            _oriDir = oriDir;

            if (Directory.Exists(oriDir))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(oriDir);
                _folderName = directoryInfo.Name;
                _parentPath = directoryInfo.Parent?.FullName;
                label_rootpath.Text = _parentPath + "\\";
                tbx_path.Text = _folderName;
                _regexStr = regexStr;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbx_path.Text))
            {
                MessageBox.Show("输入字符串为空");
                return;
            }
            if (string.IsNullOrWhiteSpace(_regexStr) == false)
            {
                if (Regex.IsMatch(tbx_path.Text, _regexStr) == false)
                {
                    MessageBox.Show($"输入字符串格式不正确: {_regexStr}");
                    return;
                }
            }
            string newPath = Path.Combine(_parentPath, tbx_path.Text) + "\\";
            if (_oriDir == newPath)
            {
                this.Close();
                return;
            }
            try
            {
                Directory.Move(_oriDir, newPath);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"重命名失败:{ex}");
                return;
            }
        }
    }
}