using ImageHandle.ViewModels;
using ImageHandle.Views;
using OpenCvSharp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageHandle.UserControls
{
    /// <summary>
    /// FaceRecognitionUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class FaceRecognitionUserControl : UserControl
    {
        private FaceRecognitionViewModel _viewModel;

        public FaceRecognitionUserControl()
        {
            InitializeComponent();
            _viewModel = new FaceRecognitionViewModel();
            this.DataContext = _viewModel;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string tt = "E:\\111\\123\\";
            string regexStr = @"^\d+_[a-zA-Z]+$";
            ReNameDirectoryWindow re = new ReNameDirectoryWindow(tt, regexStr);
            re.ShowDialog();
        }

        private void TreeTrainMsg_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem clickedItem = FindVisualAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

            FileSystemItem fileSystemItem = (FileSystemItem)TreeTrainMsg.SelectedItem;

            if (clickedItem != null)
            {
                // 获取点击项的数据上下文
                object clickedDataItem = clickedItem.DataContext;
                // 获取TreeView当前选中的数据项
                object selectedDataItem = TreeTrainMsg.SelectedItem;

                // 判断点击的数据项是否是当前选中的数据项
                bool isClickingSelectedItem = clickedDataItem == selectedDataItem;

                if (isClickingSelectedItem)
                {
                    if (fileSystemItem.IsDirectory)
                    {
                        TreeTrainMsg.ContextMenu = TreeTrainMsg.FindResource("FolderContextMenu") as ContextMenu;
                    }
                    else
                    {
                        TreeTrainMsg.ContextMenu = TreeTrainMsg.FindResource("FileContextMenu") as ContextMenu;
                    }
                }
                e.Handled = true;
            }
        }

        private void Folder_ReName(object sender, System.Windows.RoutedEventArgs e)
        {
            FileSystemItem fileSystemItem = (FileSystemItem)TreeTrainMsg.SelectedItem;
            if (fileSystemItem == null) return;
            if (!fileSystemItem.IsDirectory) return;
            ReNameDirectoryWindow reName = new ReNameDirectoryWindow(fileSystemItem.FullPath, @"^\d+_[a-zA-Z]+$");
            reName.ShowDialog();

            _viewModel.LoadDirectory(_viewModel.TrainInfoPath);
        }

        private void Folder_Delete(object sender, System.Windows.RoutedEventArgs e)
        {
            FileSystemItem fileSystemItem = (FileSystemItem)TreeTrainMsg.SelectedItem;
            if (fileSystemItem == null) return;
            if (!fileSystemItem.IsDirectory) return;

            if (MessageBox.Show($"是否删除：{fileSystemItem.FullPath}\\", "提示", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }
            try
            {
                System.IO.Directory.Delete(fileSystemItem.FullPath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败:{ex}");
            }
            _viewModel.LoadDirectory(_viewModel.TrainInfoPath);
        }

        private void File_Delete(object sender, System.Windows.RoutedEventArgs e)
        {
            FileSystemItem fileSystemItem = (FileSystemItem)TreeTrainMsg.SelectedItem;
            if (fileSystemItem == null) return;
            if (fileSystemItem.IsDirectory) return;

            if (MessageBox.Show($"是否删除：{fileSystemItem.FullPath}", "提示", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }
            try
            {
                System.IO.File.Delete(fileSystemItem.FullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败:{ex}");
            }
            _viewModel.LoadDirectory(_viewModel.TrainInfoPath);
        }

        // 视觉树查找辅助方法
        private T FindVisualAncestor<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T ancestor)
                    return ancestor;
                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }

        private void TreeTrainMsg_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FileSystemItem fileSystemItem = (FileSystemItem)TreeTrainMsg.SelectedItem;
            if (fileSystemItem == null)
            {
                return;
            }
            if (fileSystemItem.IsDirectory)
            {
                _viewModel.PreMat = null;
            }
            else
            {
                Mat mat = new Mat(fileSystemItem.FullPath);
                _viewModel.PreMat = mat;
            }
        }

        private DateTime _lastClickTime;
        private const int DoubleClickThreshold = 300; // 双击时间间隔阈值，单位为毫秒
        private void pbxTrainShow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < DoubleClickThreshold)
            {
                PicShowWindow picShowWindow = new PicShowWindow(_viewModel.PreMat);
                picShowWindow.Show();
                e.Handled = true;
            }
            _lastClickTime = DateTime.Now;
        }
    }
}