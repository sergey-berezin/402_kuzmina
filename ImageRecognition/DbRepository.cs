using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class DbRepository : DbContext
    {
        public DbSet<RecognizedImage> RecognizedImages { get; set; }
        public DbSet<RecognizedObject> RecognizedObjects { get; set; }
        public string DbPath { get; set; }
        public DbRepository(string path = "recognitions.db") => DbPath = path;
        protected override void OnConfiguring(DbContextOptionsBuilder options) 
            => options.UseLazyLoadingProxies().UseSqlite($"Data Source={DbPath}");
    }
    public class RecognizedImage
    {
        public int Id { get; set; }
        [ConcurrencyCheck]
        public byte[] Image { get; set; }
        [ConcurrencyCheck]
        virtual public ICollection<RecognizedObject> Objects { get; set; }
        [ConcurrencyCheck]
        public int Hash { get; set; }
    }
    public class RecognizedObject
    {
        public int Id { get; set; }
        [ConcurrencyCheck]
        public float X1 { get; set; }
        [ConcurrencyCheck]
        public float Y1 { get; set; }
        [ConcurrencyCheck]
        public float X2 { get; set; }
        [ConcurrencyCheck]
        public float Y2 { get; set; }
        [ConcurrencyCheck]
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
