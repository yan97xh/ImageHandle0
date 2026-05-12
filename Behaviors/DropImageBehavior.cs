using Microsoft.Xaml.Behaviors;
using System.Windows;

namespace ImageHandle.Behaviors
{
    internal class DropImageBehavior : Behavior<FrameworkElement>
    {
        public string ImageName
        {
            get { return (string)GetValue(ImageNameProperty); }
            set { SetValue(ImageNameProperty, value); }
        }

        public static readonly DependencyProperty ImageNameProperty =
            DependencyProperty.Register("ImageName", typeof(string), typeof(DropImageBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            AssociatedObject.AllowDrop = true;
            AssociatedObject.Drop += OnDrop;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Drop -= OnDrop;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    // 处理拖放的文件
                    foreach (var file in files)
                    {
                        if (file.EndsWith(".jpg") || file.EndsWith(".png"))
                        {
                            ImageName = file;
                            return;
                        }
                    }
                    MessageBox.Show("仅支持拖入 jpg 或 png");
                }
            }
        }
    }
}