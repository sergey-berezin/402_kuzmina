using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using System;
using System.Threading.Tasks;
using System.Threading;
using Models;
using System.Collections.Concurrent;
using System.Drawing.Imaging;

namespace ImageRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SelectedPath { get; set; }
        public CancellationTokenSource CTS { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            StartButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
        }
        private void ChoosingFolder(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                SelectedPath = fbd.SelectedPath;
            }
            StartButton.IsEnabled = true;
        }
        private async void BeginRecognizing(object sender, RoutedEventArgs e)
        {
            imagesBox.Items.Clear();

            CTS = new CancellationTokenSource();
            RecognitionModel model = new RecognitionModel(SelectedPath, CTS);
            ConcurrentQueue<RecognitionResponse> responseQueue = new ConcurrentQueue<RecognitionResponse>();

            Task t1 = Task.Factory.StartNew(() =>
            {
                try
                {
                    model.Recognize(responseQueue);
                }
                catch (TaskCanceledException)
                {
                    System.Windows.MessageBox.Show("Recognition was cancelled.");
                }
            });

            CancelButton.IsEnabled = true;
            StartButton.IsEnabled = false;

            Task t2 = Task.Factory.StartNew(() =>
            {
                while (t1.Status == TaskStatus.Running)
                {
                    while (responseQueue.TryDequeue(out var res) && res != null)
                    {
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
                        
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            System.Windows.Controls.Image myImage = new System.Windows.Controls.Image
                            {
                                Source = ToBitmapImage(bitmap),
                                Width = 400
                            };
                            imagesBox.Items.Add(myImage);
                        }));
                    }
                }
            });
            await Task.WhenAll(t1, t2);
            CancelButton.IsEnabled = false;
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
        private void CancelRecognition(object sender, RoutedEventArgs e)
        {
            CTS.Cancel();
        }
    }
}
