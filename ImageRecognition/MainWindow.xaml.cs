using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using System;
using System.Threading.Tasks;
using System.Threading;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

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
            DeleteItemsButton.IsEnabled = false;
        }
        private async void UpdateListBox(object sender, RoutedEventArgs e)
        {
            imagesBox.Items.Clear();

            HttpClient client = new HttpClient();
            var response = await client.GetAsync($"https://localhost:44374/image/info");
            string json = await response.Content.ReadAsStringAsync();
            List<RecognizedImage> recognizedImages = JsonConvert.DeserializeObject<List<RecognizedImage>>(json);
            foreach (var r in recognizedImages)
            {
                AddImage(imagesBox, r);
            }
            if (imagesBox.Items.Count > 0)
                DeleteItemsButton.IsEnabled = true;
        }
        private void AddImage(System.Windows.Controls.ListBox listBox, RecognizedImage recognized)
        {
            using (var ms = new MemoryStream(recognized.Image))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                using (Graphics g = Graphics.FromImage(new Bitmap(ms)))
                {
                    foreach (var obj in recognized.Objects)
                    {
                        double width = obj.X2 - obj.X1;
                        double height = obj.Y2 - obj.Y1;
                        g.DrawRectangle(Pens.Red, Convert.ToInt32(obj.X1), Convert.ToInt32(obj.Y1), Convert.ToInt32(width), Convert.ToInt32(height));
                        g.DrawString(obj.Class, new Font("Arial", 16), Brushes.Blue, new PointF(obj.X1, obj.Y1));
                    }
                }
                System.Windows.Controls.Image myImage = new System.Windows.Controls.Image
                {
                    Source = bitmapImage,
                    Width = 400
                };
                imagesBox.Items.Add(myImage);
            }
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
            DeleteItemsButton.IsEnabled = false;
            CTS = new CancellationTokenSource();
            RecognitionOne model = new RecognitionOne();

            CancelButton.IsEnabled = true;

            await Task.Factory.StartNew(() =>
            {
                var tasks = new List<Task>();
                foreach (string imagePath in Directory.GetFiles(SelectedPath))
                    tasks.Add(Task.Factory.StartNew(async () =>
                    {
                        if (CTS.Token.IsCancellationRequested) return;
                        try
                        {
                            HttpClient client = new HttpClient();
                            var bitmap = new Bitmap(Image.FromFile(imagePath));
                            ImageConverter converter = new ImageConverter();
                            var bytes = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
                            var data = new StringContent(JsonConvert.SerializeObject(bytes), Encoding.Default, "application/json");

                            var response = await client.PostAsync($"https://localhost:44374/image/recognize", data);
                            if (response.IsSuccessStatusCode == false)
                                System.Windows.MessageBox.Show($"ERROR from server:\n{response.ReasonPhrase}");
                            string json = await response.Content.ReadAsStringAsync();
                            RecognitionResponse res = JsonConvert.DeserializeObject<RecognitionResponse>(json);
                        }
                        catch (HttpRequestException)
                        {
                            System.Windows.MessageBox.Show("Service is unavailable");
                        }

                    }, CTS.Token));
                try
                {
                    Task.WaitAll(tasks.ToArray());
                }
                catch (AggregateException e)
                {
                    if (e.InnerExceptions.All(ex => ex.GetType() == typeof(TaskCanceledException)))
                        if (e.InnerExceptions.All(ex => (ex as TaskCanceledException).CancellationToken == CTS.Token))
                            System.Windows.MessageBox.Show("Recognition was cancelled.");
                }
            });
            
            StartButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
        }
        private void CancelRecognition(object sender, RoutedEventArgs e)
        {
            CTS.Cancel();
        }
        private async void DeleteItems(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            await client.DeleteAsync($"https://localhost:44374/image/delete");
            imagesBox.Items.Clear();
            DeleteItemsButton.IsEnabled = false;
        }
    }
}
