using Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace App
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Please write a full path of a directory:");
            string path = Console.ReadLine();

            CancellationTokenSource cts = new CancellationTokenSource();
            RecognitionModel model = new RecognitionModel(path, cts);
            ConcurrentQueue<RecognitionResponse> responseQueue = new ConcurrentQueue<RecognitionResponse>();

            var t = Task.Run(() =>
            {
                if (!cts.Token.IsCancellationRequested)
                {
                    for (int i = 0; i < Directory.GetFiles(path).Length; i++)
                    {
                        while (responseQueue.Count == 0) { }
                        responseQueue.TryDequeue(out var res);
                        Console.Write($"\n{res.Percent}% images are processed. ");
                        foreach (var obj in res.FoundObjects)
                            Console.Write($"{obj.Key}: {obj.Value}, ");
                        Console.WriteLine();
                    }
                }
            }, cts.Token);

            model.Recognize(responseQueue);

            t.Wait();
        }
    }
}