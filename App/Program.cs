using Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace App
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Please write a full path of a directory:");
            string path = Console.ReadLine();

            RecognitionModel model = new RecognitionModel(path);
            model.Recognize();
        }
    }
}
