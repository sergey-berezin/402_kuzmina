using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;

namespace Models
{
    public class DbRepository : DbContext
    {
        public DbSet<RecognizedImage> RecognizedImages { get; set; }
        public DbSet<RecognizedObject> RecognizedObjects { get; set; }
        private static readonly FileInfo _dataRoot = new FileInfo(typeof(RecognitionOne).Assembly.Location);
        private static readonly string dbPath = Path.Combine(_dataRoot.Directory.FullName, @"..\..\..\..\ImageRecognition\recognitions.db");
        protected override void OnConfiguring(DbContextOptionsBuilder options) 
            => options.UseLazyLoadingProxies().UseSqlite($"Data Source={dbPath}");
    }
    public class RecognizedImage
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
        virtual public ICollection<RecognizedObject> Objects { get; set; }
        public int Hash { get; set; }
    }
    public class RecognizedObject
    {
        public int Id { get; set; }
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
        public string Class { get; set; }
        public RecognizedObject(float x1, float y1, float x2, float y2, string label)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            Class = label;
        }
        public RecognizedObject() { }
    }
}
