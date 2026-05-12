using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using OpenCvSharp.Face;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Input;

namespace ImageHandle.ViewModels
{
    public partial class FaceRecognitionViewModel : ObservableObject
    {
        public FaceRecognitionViewModel()
        {
            LoadDirectory(_trainInfoPath);
            TrainGroupName = GetNamesByGroupId(_trainGroupId);
        }

        private readonly OpenCvSharp.Size _trainImageSize = new OpenCvSharp.Size(100, 100);
        private bool _trainAgain = true;

        private List<FaceImage> _faceImageList = new List<FaceImage>();
        private Dictionary<int, string> _nameDic = new Dictionary<int, string>();
        private FaceRecognizer faceRecongnizer = FisherFaceRecognizer.Create();

        [ObservableProperty]
        private string _srcImgPath;

        [ObservableProperty]
        private string _trainImgPath;

        [ObservableProperty]
        private string _saveImagePath;

        [ObservableProperty]
        private Mat _lastMat;

        [ObservableProperty]
        private Mat _trainMat;

        [ObservableProperty]
        private Mat _preMat;

        [ObservableProperty]
        private int _trainGroupId = 1;

        partial void OnTrainGroupIdChanged(int value)
        {
            TrainGroupName = GetNamesByGroupId(value);
        }

        [ObservableProperty]
        private string _trainGroupName;

        [ObservableProperty]
        private string _trainInfoPath = @"E:\FaceTrain\";

        partial void OnTrainInfoPathChanged(string value)
        {
            LoadDirectory(value);
        }

        // 训练集路径 用于前台显示训练集文件夹结构
        public ObservableCollection<FileSystemItem> TrainPathItems { get; set; } = [];

        [RelayCommand]
        private void SelectSrcImg()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "请选择图片";
            openFileDialog.Filter = "所有图片文件(*.jpg;*.bmp;*.jpeg;*.png;*.tif)|*.jpg;*.bmp;*.jpeg;*.png;*.tif";
            // dialog.InitialDirectory = @"E:\myPictures\Lena";

            if (openFileDialog.ShowDialog() == true)
            {
                SrcImgPath = openFileDialog.FileName;
                LastMat = new Mat(SrcImgPath);
            }
        }

        [RelayCommand]
        private void SelectTrainImg()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "请选择图片";
            openFileDialog.Filter = "所有图片文件(*.jpg;*.bmp;*.jpeg;*.png;*.tif)|*.jpg;*.bmp;*.jpeg;*.png;*.tif";

            if (openFileDialog.ShowDialog() == true)
            {
                TrainImgPath = openFileDialog.FileName;
                TrainMat = new Mat(TrainImgPath);
            }
        }

        [RelayCommand]
        private void SaveImage()
        {
            if (LastMat == null)
            {
                System.Windows.MessageBox.Show("没有需要保存的图像");
                return;
            }
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Title = "请选择图片";
            dialog.Filter = "图片文件 (*.jpg)|*.jpg |图片文件 (*.bmp)|*.bmp |图片文件 (*.jpeg)|*.jpeg |图片文件 (*.png)|*.png";

            if (dialog.ShowDialog() == true)
            {
                string savePath = dialog.FileName;
                if (savePath == "")
                {
                    System.Windows.MessageBox.Show("未选择保存路径");
                    return;
                }

                bool success = Cv2.ImWrite(savePath, LastMat);
                if (!success)
                {
                    System.Windows.MessageBox.Show("图像保存失败");
                    return;
                }
                System.Windows.MessageBox.Show("保存成功");
                SaveImagePath = savePath;
            }
        }

        [RelayCommand]
        private void AddTrainImage()
        {
            if (File.Exists(TrainImgPath) == false)
            {
                MessageBox.Show("未选择训练图像");
                return;
            }
            if (string.IsNullOrWhiteSpace(TrainGroupName))
            {
                MessageBox.Show("训练集人名为空");
                return;
            }
            string name = GetNamesByGroupId(TrainGroupId);
            if (name != "")
            {
                if (name != TrainGroupName)
                {
                    MessageBox.Show($"本地已存在训练分组:{TrainGroupId},名称为:{name}");
                    return;
                }
            }
            string pattern = @"^[a-zA-Z]+$";
            if (Regex.IsMatch(TrainGroupName, pattern) == false)
            {
                MessageBox.Show("训练集人名只能包含字母");
                return;
            }
            AddTrainImg(TrainMat, _trainImageSize, TrainGroupId, TrainGroupName);
            LoadDirectory(_trainInfoPath);
        }

        public void LoadDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                var root = new FileSystemItem(path);
                TrainPathItems.Clear();
                TrainPathItems.Add(root);
                _trainAgain = true;
            }
        }

        [RelayCommand]
        private void ChangeTrainPath()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "请选择文件夹";
                dialog.RootFolder = Environment.SpecialFolder.Desktop;
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    TrainInfoPath = dialog.SelectedPath;
                }
            }
        }

        [RelayCommand]
        private void RefreshTrainInfo()
        {
            LoadDirectory(_trainInfoPath);
        }

        [RelayCommand]
        private void FaceRecongnize()
        {
            if (string.IsNullOrWhiteSpace(_srcImgPath))
            {
                MessageBox.Show("未选择输入图像");
                return;
            }
            if (_trainAgain == true)
            {
                MessageBox.Show("请先训练");
                return;
            }
            Mat mm = new Mat(_srcImgPath);
            OpenCvSharp.Rect[] rects = GetRects(mm);

            if (rects.Length < 1)
            {
                MessageBox.Show("图片未识别到人脸");
                return;
            }

            List<Mat> faces = GetFaces(mm, rects);

            List<string> names = GetNames(faces);

            LastMat = ShowFaceRects(mm, rects, names);
        }

        private string GetNamesByGroupId(int groupId)
        {
            DirectoryInfo _path = new DirectoryInfo(_trainInfoPath);

            foreach (DirectoryInfo var in _path.GetDirectories())
            {
                string[] tempstr = var.Name.ToString().Split('_');
                if (tempstr[0] == groupId.ToString())
                {
                    return tempstr[1];
                }
            }
            return "";
        }

        private void AddTrainImg(Mat src, OpenCvSharp.Size size, int groupId, string name)
        {
            string path0 = groupId.ToString() + "_" + name;
            string path = _trainInfoPath + "\\" + path0 + "\\";
            // 判断图像是否可以作为训练图像添加  即 图像包含人脸 且只有一张人脸
            if (CheckTrainImg(src) == false)
            {
                return;
            }
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("路径创建失败:" + ex.Message);
                return;
            }
            DirectoryInfo _path = new DirectoryInfo(path);
            int i = 0;
            do
            {
                i++;
            } while (File.Exists(path + i.ToString() + ".jpg"));
            string picname = path + i.ToString() + ".jpg";
            try
            {
                Cv2.Resize(src, src, size);
                src.SaveImage(picname);
                _trainAgain = true;
                MessageBox.Show("训练图像添加成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("训练图像添加失败:" + ex.Message);
            }
        }

        // 训练图像添加单人脸的 判断训练图像是不是只有一张人脸  测试图像可以是多人脸的
        private bool CheckTrainImg(Mat srcImg)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Resource\\xml\\haarcascades\\" + "haarcascade_frontalface_alt.xml";
            if (!File.Exists(path))
            {
                MessageBox.Show("本地训练器文件不存在");
                return false;
            }
            CascadeClassifier face = new CascadeClassifier(path);

            Rect[] rect = face.DetectMultiScale(srcImg);

            if (rect.Count() == 1)
            {
                return true;
            }
            else if (rect.Count() > 1)
            {
                MessageBox.Show("该图像包含多张人脸");
                return false;
            }
            else
            {
                MessageBox.Show("该图像无人脸");
                return false;
            }
        }

        private bool GetImageInfos()
        {
            _faceImageList.Clear();
            _nameDic.Clear();
            DirectoryInfo _path = new DirectoryInfo(_trainInfoPath);

            if (_path.GetDirectories().Length < 2)
            {
                MessageBox.Show("本地训练集小于两组,请添加训练集");
                return false;
            }
            if (HasAtLeastTwoDirectoriesWithFiles(_trainInfoPath) == false)
            {
                MessageBox.Show("本地训练集小于两组,请添加训练集");
                return false;
            }

            foreach (DirectoryInfo var in _path.GetDirectories())
            {
                string[] tempstr = var.Name.ToString().Split('_');
                int groupID = 0;
                int.TryParse(tempstr[0], out groupID);
                foreach (FileInfo vv in var.GetFiles())
                {
                    if (!vv.FullName.Contains(".jpg"))
                        continue;
                    _faceImageList.Add(
                        new FaceImage
                        {
                            Image = new Mat(vv.FullName, ImreadModes.Grayscale),
                            GroupId = groupID,
                        });
                }
                _nameDic.Add(groupID, tempstr[1]);
            }
            return true;
        }

        [RelayCommand]
        private void TrainInfo()
        {
            if (!Directory.Exists(_trainInfoPath))
            {
                MessageBox.Show("训练集路径不存在,请先添加");
                return;
            }
            if (GetImageInfos() == false)
                return;
            try
            {
                faceRecongnizer.Train(_faceImageList.Select(x => x.Image), _faceImageList.Select(x => x.GroupId));
                _trainAgain = false;
                MessageBox.Show("本地训练集训练完成!");
            }
            catch (Exception ex)
            {
                _trainAgain = true;
                MessageBox.Show("训练失败: " + ex.Message);
            }
        }

        private bool HasAtLeastTwoDirectoriesWithFiles(string directoryPath, string fileExtension = ".jpg")
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return false;

            try
            {
                var directoryInfo = new DirectoryInfo(directoryPath);

                return directoryInfo.EnumerateDirectories()
                                    .Count(subDir => subDir.EnumerateFiles()
                                                           .Any(file => file.Extension.Equals(fileExtension, StringComparison.OrdinalIgnoreCase))) >= 2;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is PathTooLongException || ex is DirectoryNotFoundException)
            {
                return false;
            }
        }

        // 从图片中获取所有的人脸图片
        private List<Mat> GetFaces(Mat mm, OpenCvSharp.Rect[] rects)
        {
            List<Mat> faces = new List<Mat>();
            foreach (Rect rect in rects)
            {
                Mat m1 = new Mat(mm, rect);
                Cv2.CvtColor(m1, m1, ColorConversionCodes.BGR2GRAY);
                Cv2.Resize(m1, m1, _trainImageSize);
                // Cv2.EqualizeHist(m1, m1);
                faces.Add(m1);
            }
            return faces;
        }

        // 获取所有名字
        private List<string> GetNames(List<Mat> mm)
        {
            List<string> names = new List<string>();
            for (int i = 0; i < mm.Count; i++)
            {
                int groupId = -2;
                //groupId = yVars.FaceDetect.faceRecongnizer.Predict(mm[i]);
                double confidence = 0.0;
                faceRecongnizer.Predict(mm[i], out groupId, out confidence);
                string desName;
                _nameDic.TryGetValue(groupId, out desName);
                // names.Add(desName + " " + confidence.ToString("0.00"));
                names.Add(desName);
            }
            return names;
        }

        // 获取图像所有的人脸框
        private OpenCvSharp.Rect[] GetRects(Mat mm)
        {
            Mat grayImage = new Mat();
            Cv2.CvtColor(mm, grayImage, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(grayImage, grayImage);
            string path = AppDomain.CurrentDomain.BaseDirectory + "Resource\\xml\\haarcascades\\" + "haarcascade_frontalface_alt.xml";
            CascadeClassifier face = new CascadeClassifier(path);

            //Rect[] faces = face.DetectMultiScale(
            //    image: grayImage,
            //    scaleFactor: 1.1,
            //    minNeighbors: 2,
            //    flags: HaarDetectionType.DoRoughSearch | HaarDetectionType.ScaleImage,
            //    minSize: new OpenCvSharp.Size(30, 30)
            //);
            //Rect[] faces = face.DetectMultiScale(grayImage);
            Rect[] faces = face.DetectMultiScale(mm);
            return faces;
        }

        // 画出所有的框和名字
        private Mat ShowFaceRects(Mat mm, Rect[] faces, List<string> names)
        {
            Random rnd = new Random();
            int i = -1;
            foreach (Rect face in faces)
            {
                i++;
                Scalar color = new Scalar(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
                Cv2.Rectangle(mm, face, color);
                // 无法显示中文 可以考虑用  System.Drawing.Graphics
                Cv2.PutText(mm, names[i], face.TopLeft, HersheyFonts.HersheySimplex, 1, Scalar.Red);
            }
            return mm;
        }
    }

    public class FileSystemItem
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public ObservableCollection<FileSystemItem> Children { get; set; }

        public FileSystemItem(string path)
        {
            FullPath = path;
            Name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(Name))
                Name = path;

            IsDirectory = Directory.Exists(path);
            Children = new ObservableCollection<FileSystemItem>();

            if (IsDirectory)
            {
                LoadChildren();
            }
        }

        private void LoadChildren()
        {
            try
            {
                // 先加载子文件夹
                var directories = Directory.GetDirectories(FullPath)
                    .Select(dir => new FileSystemItem(dir))
                    .OrderBy(item => item.Name);

                foreach (var dir in directories)
                {
                    Children.Add(dir);
                }

                // 再加载文件
                var files = Directory.GetFiles(FullPath)
                    .Select(file => new FileSystemItem(file))
                    .OrderBy(item => item.Name);

                foreach (var file in files)
                {
                    Children.Add(file);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 处理无权限访问的情况
                Children.Add(new FileSystemItem("【无访问权限】"));
            }
            catch (Exception ex)
            {
                // 处理其他异常
                Children.Add(new FileSystemItem($"【错误: {ex.Message}】"));
            }
        }
    }

    public class FaceImage
    {
        /// <summary>
        /// 图像
        /// </summary>
        public Mat Image { set; get; }

        /// <summary>
        /// 编号
        /// </summary>
        public int GroupId { set; get; }
    }
}