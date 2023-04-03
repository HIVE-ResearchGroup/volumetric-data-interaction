using System.IO;
using UnityEngine;

public static class FileLoader
{
    public static Texture2D LoadImage(string path)
    {
        Texture2D texture = new(1, 1);
        texture.LoadImage(File.ReadAllBytes(path));
        return texture;
    }
}
