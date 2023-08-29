using System;
using System.IO;
using Constants;
using UnityEngine;

namespace Helper
{
    public static class FileTools
    {
        public static string SaveBitmapPng(Texture2D image)
        {
            var fileLocation = GetDatedFilePath();
            File.WriteAllBytes($"{fileLocation}.png", image.EncodeToPNG());
            //File.WriteAllBytes($"{fileLocation}.bmp", image.EncodeToBMP());
            return fileLocation;
        }

        private static string GetDatedFilePath(string name = "plane", string path = ConfigurationConstants.IMAGES_FOLDER_PATH)
        {
            var fileName = DateTime.Now.ToString("yy-MM-dd hh.mm.ss " + name);
            EnsurePathExists(path);
            return Path.Combine(ConfigurationConstants.IMAGES_FOLDER_PATH, fileName);
        }
        
        private static void EnsurePathExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        
        public static Texture2D LoadImage(string path)
        {
            Texture2D texture = new(1, 1);
            texture.LoadImage(File.ReadAllBytes(path));
            return texture;
        }
    }
}
