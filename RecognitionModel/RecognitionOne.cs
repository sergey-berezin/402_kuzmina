using Microsoft.ML;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;

namespace Models
{
    public class RecognitionOne
    {
        private static readonly FileInfo _dataRoot = new FileInfo(typeof(RecognitionOne).Assembly.Location);
        private static readonly string modelPath = Path.Combine(_dataRoot.Directory.FullName, @"..\..\..\..\RecognitionModel\yolov4.onnx");
        private static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat",
            "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear",
            "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat",
            "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple",
            "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet",
            "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase",
            "scissors", "teddy bear", "hair drier", "toothbrush" };

        public static RecognitionResponse Recognize(Bitmap bitmapOrig)
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

            var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmapOrig });

            RecognitionResponse response = new RecognitionResponse();
            var results = predict.GetResults(classesNames, 0.3f, 0.7f);
            foreach (var res in results)
            {
                int count = foundObjects.ContainsKey(res.Label) ? foundObjects[res.Label] : 0;
                if (count == 0)
                    foundObjects.Add(res.Label, ++count);
                else
                    foundObjects[res.Label]++;

                response.Objects.Add(new RecognitionObject(res.Label, new float[] { res.BBox[0], res.BBox[1], res.BBox[2], res.BBox[3]}));
            }
            response.FoundObjects = foundObjects;
            ImageConverter converter = new ImageConverter();
            response.Image = (byte[])converter.ConvertTo(bitmapOrig, typeof(byte[]));

            return response;
        }
    }
}
