using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController
    {
        private DbRepository db;
        public ImageController(DbRepository dbRepository) =>
            db = dbRepository;
        [HttpPost("recognize")]
        public ActionResult<RecognitionResponse> PostImage(byte[] imgByte)
        {
            var ms = new MemoryStream(imgByte);
            var bitmap = new Bitmap(ms);
            var res = RecognitionOne.Recognize(bitmap);
            List<RecognizedObject> dbObjects = new List<RecognizedObject>();
            foreach (var obj in res.Objects)
                dbObjects.Add(new RecognizedObject(obj.Corners[0], obj.Corners[1], obj.Corners[2], obj.Corners[3], obj.Label));
            RecognizedImage dbImage = new RecognizedImage();
            dbImage.Image = res.Image;
            dbImage.Hash = new BigInteger(res.Image).GetHashCode();
            dbImage.Objects = new List<RecognizedObject>(dbObjects.ToArray());
            if (db.RecognizedImages.All(r => r.Hash != dbImage.Hash) ||
                db.RecognizedImages.Where(r => r.Hash == dbImage.Hash).All(r => r.Image != dbImage.Image))
            {
                db.RecognizedImages.Add(dbImage);
                db.SaveChanges();
            }
            return res;
        }
        [HttpGet("info")]
        public ActionResult<ICollection<RecognizedImage>> GetInfo()
        {
            return new List<RecognizedImage>(db.RecognizedImages);
        }
        [HttpDelete("delete")]
        public ActionResult DeleteImages()
        {
            db.RecognizedImages.RemoveRange(db.RecognizedImages);
            db.RecognizedObjects.RemoveRange(db.RecognizedObjects);
            db.SaveChanges();
            return new EmptyResult();
        }

    }
}
