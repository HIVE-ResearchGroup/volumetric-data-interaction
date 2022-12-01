using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Assets.Scripts.Helper
{
    public static class FileSetter
    {
        public static void EnsurePathExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static string GetDatedFilePath(string name = "plane", string path = ConfigurationConstants.IMAGES_FOLDER_PATH)
        {
            var fileName = DateTime.Now.ToString("yy-MM-dd hh.mm.ss " + name);
            EnsurePathExists(path);
            return Path.Combine(ConfigurationConstants.IMAGES_FOLDER_PATH, fileName);
        }

        public static string SaveBitmapPng(Bitmap image)
        {
            var fileLocation = GetDatedFilePath();
            image.Save(fileLocation + ".bmp", ImageFormat.Bmp);
            image.Save(fileLocation + ".png", format: ImageFormat.Png);
            return fileLocation;
        }
    }
}
