using System;
using System.IO;
using UnityEngine;

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

        public static string SaveBitmapPng(Texture2D image)
        {
            var fileLocation = GetDatedFilePath();
            File.WriteAllBytes($"{fileLocation}.png", image.EncodeToPNG());
            //File.WriteAllBytes($"{fileLocation}.bmp", image.EncodeToBMP());
            return fileLocation;
        }
    }
}
