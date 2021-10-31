using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System;
using System.Windows.Documents;
using System.Threading.Tasks;
using System.Threading;
using Models;
using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.Linq;

namespace ImageRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SelectedPath { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }
        private void ChoosingFolder(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            //List<string> files = new List<string>();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                SelectedPath = fbd.SelectedPath;
                //files = new List<string>(Directory.GetFiles(SelectedPath));
            }

            //foreach (string file in files)
            //{ 
            //    System.Windows.Controls.Image myImage = new System.Windows.Controls.Image();

            //    BitmapImage myBitmapImage = new BitmapImage();
            //    myBitmapImage.BeginInit();
            //    myBitmapImage.UriSource = new Uri(file);
            //    myBitmapImage.DecodePixelWidth = 200;
            //    myBitmapImage.EndInit();

            //    myImage.Source = myBitmapImage;
            //    myImage.Width = 200;

            //    myPanel.Children.Add(myImage);
            //}
        }
        private async void BeginRecognizing(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            RecognitionModel model = new RecognitionModel(SelectedPath, cts);
            ConcurrentQueue<RecognitionResponse> responseQueue = new ConcurrentQueue<RecognitionResponse>();

            await Task.Run(() =>
            {
                model.Recognize(responseQueue);
            });

            for (int i = 0; i < Directory.GetFiles(SelectedPath).Length; i++)
            {
                while (responseQueue.Count == 0) { }
                responseQueue.TryDequeue(out var res);

                Bitmap bitmap = res.Image;
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    foreach (var obj in res.Corners)
                    {
                        double width = obj.Key[2] - obj.Key[0];
                        double height = obj.Key[3] - obj.Key[1];
                        g.DrawRectangle(Pens.Red, Convert.ToInt32(obj.Key[0]), Convert.ToInt32(obj.Key[1]), Convert.ToInt32(width), Convert.ToInt32(height));
                        g.DrawString(obj.Value, new Font("Arial", 16), Brushes.Blue, new PointF(obj.Key[0], obj.Key[1]));
                    }
                }
                System.Windows.Controls.Image myImage = new System.Windows.Controls.Image
                {
                    Source = ToBitmapImage(bitmap),
                    Width = 400
                };
                imagesBox.Items.Add(myImage);
            }
        }
        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
