using Microsoft.ML;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;

namespace Models
{
    public class RecognitionModel
    {
        public string DirectoryPath { get; set; }
        public CancellationTokenSource CTS { get; set; }

        static readonly ConcurrentQueue<Bitmap> bitmapQueue = new ConcurrentQueue<Bitmap>();

        private static readonly FileInfo _dataRoot = new FileInfo(typeof(RecognitionModel).Assembly.Location);
        private static readonly string modelPath = Path.Combine(_dataRoot.Directory.FullName, @"..\..\..\..\RecognitionModel\yolov4.onnx");
        private static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat",
            "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear",
            "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat",
            "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple",
            "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet",
            "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase",
            "scissors", "teddy bear", "hair drier", "toothbrush" };

        public RecognitionModel(string directoryPath, CancellationTokenSource cancellationTokenSource)
        {
            DirectoryPath = directoryPath;
            CTS = cancellationTokenSource;
        }

        public void Recognize(ConcurrentQueue<RecognitionResponse> responseQueue)
        {
            MLContext mlContext = new MLContext();
            var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: modelPath, recursionLimit: 100));
            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));
            var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);

            Dictionary<string, int> foundObjects = new Dictionary<string, int>();
            int imgCount = Directory.GetFiles(DirectoryPath).Length;
            var locker = new Object();
            int currCount = 0;
            var tasks = new List<Task>();

            var sw = new Stopwatch();
            sw.Start();

            foreach (string imagePath in Directory.GetFiles(DirectoryPath))
            {
                tasks.Add(Task.Factory.StartNew( () =>
                {
                    if (!CTS.Token.IsCancellationRequested)
                    {
                        var bitmap = new Bitmap(Image.FromFile(imagePath));
                        bitmapQueue.Enqueue(bitmap);
                    }
                }, CTS.Token));
            }
            for (int j = 0; j < imgCount; j++)
            {
                while (bitmapQueue.Count == 0) { }
                bitmapQueue.TryDequeue(out Bitmap bitmap);

                var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });

                tasks.Add(Task.Factory.StartNew( () =>
                {
                    RecognitionResponse response = new RecognitionResponse();
                    if (!CTS.Token.IsCancellationRequested)
                    {
                        var results = predict.GetResults(classesNames, 0.3f, 0.7f);
                        foreach (var res in results)
                        {
                            int count = foundObjects.ContainsKey(res.Label) ? foundObjects[res.Label] : 0;
                            if (count == 0)
                                foundObjects.Add(res.Label, ++count);
                            else
                                foundObjects[res.Label]++;
                            response.Corners.Add(new List<float> { res.BBox[0], res.BBox[1], res.BBox[2], res.BBox[3]}, res.Label);
                        }
                        response.FoundObjects = foundObjects;
                        response.Image = bitmap;
                        lock (locker)
                        {
                            currCount++;
                            response.Percent = Convert.ToInt32(100 * currCount / imgCount);
                        }
                        responseQueue.Enqueue(response);
                    }
                    return response;
                }, CTS.Token));
            }
            Task.WaitAll(tasks.ToArray());
            sw.Stop();
            Console.WriteLine($"\nDone in {sw.ElapsedMilliseconds}ms.");
        }
    }
}
