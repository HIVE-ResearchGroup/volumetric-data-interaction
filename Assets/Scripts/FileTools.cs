using System;
using System.IO;
using UnityEngine;

public static class FileTools
{
    private const string DefaultImageExportPath = @"C:\temp\TempImages";
    
    public static string SaveBitmapPng(Texture2D image)
    {
        var fileLocation = GetDatedFilePath();
        File.WriteAllBytes($"{fileLocation}.png", image.EncodeToPNG());
        //File.WriteAllBytes($"{fileLocation}.bmp", image.EncodeToBMP());
        return fileLocation;
    }

    private static string GetDatedFilePath(string name = "plane", string path = DefaultImageExportPath)
    {
        var fileName = DateTime.Now.ToString("yy-MM-dd hh.mm.ss " + name);
        EnsurePathExists(path);
        return Path.Combine(path, fileName);
    }
        
    private static void EnsurePathExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
        
    /// <summary>
    /// The Unity ImageConversion API is used, which supports loading of .png and .jpg files.
    /// </summary>
    /// <param name="path">The path the image is located at.</param>
    /// <returns>A Texture2D object with the loaded image. Invalid images still return valid Texture2D objects, so it needs to be checked.</returns>
    public static Texture2D LoadImage(string path)
    {
        var texture = new Texture2D(2, 2)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.LoadImage(File.ReadAllBytes(path));
        return texture;
    }
}
