using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;
using Path = System.IO.Path;

namespace ReactNative_Generator
{
    /// <summary>
    /// Interaction logic for ImagePicker.xaml
    /// </summary>
    public partial class ImagePicker : Window, INotifyPropertyChanged
    {
        public ObservableCollection<ImageSource> imageSource { get; set; } = new ObservableCollection<ImageSource>();
        public event PropertyChangedEventHandler PropertyChanged;


        public Dictionary<ImageSource, String> paths = new Dictionary<ImageSource, String>();

        public ImagePicker()
        {
            InitializeComponent();


            DataContext = this;
        }


        private void StackPanel_DragEnter(object sender, DragEventArgs e)
        {
            (sender as Control).Background = new SolidColorBrush(Colors.Red);
           
        }

        private void imagesListBox_DragLeave(object sender, DragEventArgs e)
        {
            (sender as Control).Background = new SolidColorBrush();
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            var data = (string[])e.Data.GetData(DataFormats.FileDrop);
            if(data == null || data.Length == 0 )
                return;

            imageSource.Clear();

            foreach ( var item in data )
            {
                if (Directory.Exists(item))
                    LoadImages(Directory.GetFiles(item));

                if (File.Exists(item))
                    LoadImage(item);
    
            }

            //imageSource = new ObservableCollection<ImageSource>();
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(imageSource)));
            (sender as Control).Background = new SolidColorBrush();
        }

        private void LoadImages(string[] paths)
        {
            foreach (var path in paths)
                if (File.Exists(path))
                    LoadImage(path);

        }

        private void LoadImage(string path)
        {
            try
            {
                var src = new BitmapImage(new Uri(path, UriKind.Absolute));
                imageSource.Add(src);

                paths.Add(src, path);

            }
            catch
            {

            }
        }


        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var img = (sender as Image);

            var src = img.Source;

            if (!paths.ContainsKey(src))
                return;

            String path = paths[src];
            String fileName= Path.GetFileName(path);

            path = path.Remove(path.LastIndexOf(Path.DirectorySeparatorChar));
            path = path.Remove(0, path.LastIndexOf(Path.DirectorySeparatorChar) + 1);

            String rnPath = $"./{fileName}";

            String aspectRatio = (src.Width / src.Height).ToString().Replace(",", ".");
            String imgComp = $"<Image source={{require('{rnPath}')}} style={{{{width: '100%', height: undefined, aspectRatio: {aspectRatio} }}}}/>";
                ;
            DragDrop.DoDragDrop(img, imgComp, DragDropEffects.Copy);
        }

        private void GroupBox_Drop(object sender, DragEventArgs e)
        {
            var data = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var item in data)
                RenameFilesInDirectory(item, new Random().Next(5, 12));
        }

        static void RenameFilesInDirectory(string directoryPath, int nameLength)
        {
            Random random = new Random();

            // Check if the directory exists
            if (Directory.Exists(directoryPath))
            {
                string[] files = Directory.GetFiles(directoryPath);

                foreach (string filePath in files)
                {
                    string extension = Path.GetExtension(filePath);
                    string randomName = GenerateRandomName(random, nameLength);

                    string newFilePath = Path.Combine(directoryPath, randomName + extension);

                    // Ensure the new filename doesn't already exist
                    while (File.Exists(newFilePath))
                    {
                        randomName = GenerateRandomName(random, nameLength);
                        newFilePath = Path.Combine(directoryPath, randomName + extension);
                    }

                    File.Move(filePath, newFilePath);
                    Console.WriteLine($"Renamed '{Path.GetFileName(filePath)}' to '{Path.GetFileName(newFilePath)}'");
                }

                Console.WriteLine("All files renamed successfully.");
            }
            else
            {
                Console.WriteLine($"Directory '{directoryPath}' does not exist.");
            }
        }

        static string GenerateRandomName(Random random, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] name = new char[length];

            name[0] = chars[random.Next(chars.Length - 10)]; //First character is no digital
            for (int i = 1; i < length; i++)
            {
                name[i] = chars[random.Next(chars.Length)];
            }

            return new string(name);
        }
    }
}
