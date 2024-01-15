using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.Drawing;
using System.IO;

namespace ReactNative_Generator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Process process = null;
        public MainWindow()
        {
            InitializeComponent();
            StartCmd();

            frame1.Content = new ImagePicker().Content;
        }

        private void StartCmd()
        {
            if (process != null) 
                process.StandardInput.WriteLine("exit");

            process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.OutputDataReceived += Process_OutputDataReceived;
            
            process.Start();
            process.BeginOutputReadLine();

        }

        private void OnRunCommandClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string command = cmdInput.Text;
                process.StandardInput.WriteLine(command);
            }
            catch (Exception ex)
            {
                outputTextBlock.Text = "Error running command: " + ex.Message;
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                outputTextBlock.Text += e.Data + "\n";
                sv.ScrollToEnd();
                Activate();
            });
        }

        private void OnCloseCmdClick(object sender, RoutedEventArgs e)
        {
            process.StandardInput.WriteLine("exit");
        }

        private void OnStartCmdClick(object sender, RoutedEventArgs e)
        {
            StartCmd();
        }

        private void OnImagesFunctionClick(object sender, RoutedEventArgs e)
        {
            string initialDirectory = @"C:\";

            // Show the image open dialog and get the selected image file(s)
            string[] selectedImageFiles = ShowImageOpenDialog(initialDirectory);

            // Generate the JavaScript function with the given template and selected images
            string jsFunction = GenerateJsFunction(selectedImageFiles);

            // Output the JavaScript function
            BigTextBox.Text = jsFunction;
            Console.WriteLine(jsFunction);
        }

        static string[] ShowImageOpenDialog(string initialDirectory)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
                // Set the initial directory for the dialog
                openFileDialog.InitialDirectory = initialDirectory;

                // Allow selecting multiple files
                openFileDialog.Multiselect = true;

            // Filter only PNG files (you can customize this filter if needed)
            openFileDialog.Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.ico|PNG files (*.png)|*.png|JPEG files (*.jpg;*.jpeg)|*.jpg;*.jpeg|BMP files (*.bmp)|*.bmp|GIF files (*.gif)|*.gif|TIFF files (*.tiff)|*.tiff|Icon files (*.ico)|*.ico|All files (*.*)|*.*";


            if (openFileDialog.ShowDialog().GetValueOrDefault())
                {
                    // Return the selected image file(s)

                    return openFileDialog.FileNames;
                }


            // Return an empty array if the dialog was canceled or closed
            return new string[0];
        }

        static string GenerateJsFunction(string[] imageFiles)
        {
            string jsFunction = "const Img = ({num} : any) => {\n" +
                               "    const images = [\n";

            foreach (string imageFile in imageFiles)
            {

                BitmapImage image = new BitmapImage(new Uri(imageFile));
                string fileName = Path.GetFileName(imageFile); // Get the file name

                jsFunction += $"        <Image source={{require('{"../images/" +fileName}')}}   style={{{{width: '100%', height: undefined, aspectRatio: {image.PixelWidth}/{image.PixelHeight}}}}} />,\n";
            }

            jsFunction += "    ];\n" +
                          "    return images[num-1];\n" +
                          "}\n";

            Clipboard.SetText(jsFunction);

            return jsFunction;
        }

        static double GetImageAspectRatio(string imagePath)
        {
            BitmapImage image = new BitmapImage(new Uri(imagePath));
            double aspectRatio = (double)image.PixelWidth / image.PixelHeight;
            return aspectRatio;
        }

        private void OnGetTextComponentsClick(object sender, RoutedEventArgs e)
        {
            string s = Clipboard.GetText();

            string[] strs = s.Split('\n');

            StringBuilder stringBuilder = new StringBuilder();

            foreach (string str in strs)
            {
                if(str=="")
                    continue; 
                stringBuilder.Append($"<Text style={{{{...styles.text}}}}>{str.Remove(str.Length-1,1)}</Text>\n");
            }

            //for remove last \n
           // stringBuilder.Remove(stringBuilder.Length - 1, 1); 
            BigTextBox.Text = stringBuilder.ToString();
            Clipboard.Clear();

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetText(stringBuilder.ToString());
                    return;
                }
                catch { }
                System.Threading.Thread.Sleep(10);
            }

        }
    }
}
