using System;
using System.IO;
using Constants;
using UnityEngine;

namespace Helper
{
    public static class FileSaver
    {
        public static string SaveBitmapPng(Texture2D image)
        {
            var fileLocation = GetDatedFilePath();
            File.WriteAllBytes($"{fileLocation}.png", image.EncodeToPNG());
            //File.WriteAllBytes($"{fileLocation}.bmp", image.EncodeToBMP());
            return fileLocation;
        }
        
        private static void EnsurePathExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static string GetDatedFilePath(string name = "plane", string path = ConfigurationConstants.IMAGES_FOLDER_PATH)
        {
            var fileName = DateTime.Now.ToString("yy-MM-dd hh.mm.ss " + name);
            EnsurePathExists(path);
            return Path.Combine(ConfigurationConstants.IMAGES_FOLDER_PATH, fileName);
        }
    }
}
