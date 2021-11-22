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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ImageRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SelectedPath { get; set; }
        public CancellationTokenSource CTS { get; set; }
        DbRepository Db = new DbRepository(dbPath);
        private static readonly FileInfo _dataRoot = new FileInfo(typeof(RecognitionModel).Assembly.Location);
        private static readonly string dbPath = Path.Combine(_dataRoot.Directory.FullName, @"..\..\..\..\ImageRecognition\recognitions.db");
        public MainWindow()
        {
            InitializeComponent();
            StartButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            DeleteItemsButton.IsEnabled = false;
            UpdateListBox();
        }
        public void UpdateListBox()
        {
            foreach (var r in Db.RecognizedImages)
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
            imagesBox.Items.Clear();
            DeleteItemsButton.IsEnabled = false;
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
                        ConcurrentQueue<RecognizedObject> dbObjects = new ConcurrentQueue<RecognizedObject>();
                        foreach (var obj in res.Corners)
                            dbObjects.Enqueue(new RecognizedObject(obj.Key[0], obj.Key[1], obj.Key[2], obj.Key[3], obj.Value));

                        RecognizedImage dbImage = new RecognizedImage();
                        ImageConverter converter = new ImageConverter();
                        byte[] byteImg = (byte[])converter.ConvertTo(res.Image, typeof(byte[]));
                        dbImage.Image = byteImg;
                        dbImage.Hash = new BigInteger(byteImg).GetHashCode();
                        dbImage.Objects = new List<RecognizedObject>(dbObjects.ToArray());
                        if (Db.RecognizedImages.All(r => r.Hash != dbImage.Hash) || 
                            Db.RecognizedImages.Where(r => r.Hash == dbImage.Hash).All(r => r.Image != dbImage.Image))
                        {
                            Db.RecognizedImages.Add(dbImage);
                            Db.SaveChanges();
                        }
                    }
                }
            });
            await Task.WhenAll(t1, t2);
            CancelButton.IsEnabled = false;
            UpdateListBox();
        }
        private void CancelRecognition(object sender, RoutedEventArgs e)
        {
            CTS.Cancel();
        }
        private void DeleteItems(object sender, RoutedEventArgs e)
        {
            Db.RecognizedImages.RemoveRange(Db.RecognizedImages);
            Db.RecognizedObjects.RemoveRange(Db.RecognizedObjects);
            Db.SaveChanges();
            imagesBox.Items.Clear();
            DeleteItemsButton.IsEnabled = false;
        }
    }
}
