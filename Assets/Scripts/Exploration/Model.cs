using System;
using System.Drawing;
using System.IO;

namespace Assets.Scripts.Exploration
{
    /// <summary>
    /// Add scs.rsp to be able to use Bitmaps in Unity
    /// https://forum.unity.com/threads/using-bitmaps-in-unity.899168/
    /// </summary>
    public class Model
    {
        private Bitmap[] originalBitmap;

        private int xMax; // img width
        private int yMax; // img height
        private int zMax; // number of images

        public Model(string path = @"C:FH XPRO\Data\Nähmaschine\Stack_z_1mm")
        // todo - move to configuration constants after merge!, z, y, x!
        // create an instance of each model (x, y, z)
        // problem! WHEN trying from other axis--
        // make calculation/imgs from all axis - depending on orientation to model - choose calculation?
        {
            originalBitmap = InitModel(path);
        }

        private Bitmap[] InitModel(string path)
        {
            var files = Directory.GetFiles(path);
            Bitmap[] model3D = new Bitmap[files.Length];

            for (var i = 0; i < files.Length; i++)
            {
                var imagePath = Path.Combine(path, files[i]);
                Console.WriteLine(imagePath);
                model3D[i] = new Bitmap(imagePath);
            }

            xMax = model3D[0].Width;
            yMax = model3D[0].Height;
            zMax = files.Length;

            return model3D;
        }
    
        /// <summary>
        /// Attention - depends on where bit map starts with pixel!
        /// using 8 bit grey would be best
        /// </summary>
        /// <returns></returns>
        public Bitmap CalculateCuttingplane(int zTopLeft, int zTopRight, int zBottomRight, int zBottomLeft, InterpolationType interpolation = InterpolationType.NearestNeighbour)
        {
            var diffZLeft = zBottomLeft - zTopLeft;
            var diffZTop = zTopRight - zTopLeft;

            // could be done with pythagoras but does not look right - check with prototype!
            var newWidth = xMax + Math.Abs(diffZTop); // (int)Math.Round(Math.Sqrt(Math.Pow(xMax, 2) + Math.Pow(Math.Abs(diffZTop), 2)), 0);
            var newHeight = yMax + Math.Abs(diffZLeft); // (int)Math.Round(Math.Sqrt(Math.Pow(yMax, 2) + Math.Pow(Math.Abs(diffZLeft), 2)), 0); 

            var zStepX = (float)diffZTop / newWidth;
            var zStepY = (float)diffZLeft / newHeight;

            return GetInterpolation(newWidth, newHeight, zTopLeft, zStepX, zStepY, interpolation);
        }

        private Bitmap GetInterpolation(int newWidth, int newHeight, int zStartTopLeft, float zStepX, float zStepY, InterpolationType interpolationType)
        {
            float xStep = (float)xMax / newWidth;
            float yStep = (float)yMax / newHeight;

            var resultImage = new Bitmap(newWidth, newHeight);
            int currZx, currZy, currX, currY = 0;
            for (int x = 0; x < newWidth; x++)
            {
                currZx = (int)Math.Round(zStartTopLeft + x * zStepX, 0);
                currX = (int)Math.Round(x * xStep, 0);

                for (int y = 0; y < newHeight; y++)
                {
                    currZy = (int)Math.Round(currZx + y * zStepY, 0);
                    if (currZy >= zMax)
                    {
                        currZy = zMax - 1;
                    }
                    currY = (int)Math.Round(y * yStep, 0);

                    var currBitmap = originalBitmap[currZy];

                    System.Drawing.Color result;
                    if (interpolationType == InterpolationType.NearestNeighbour)
                    {
                        result = Interpolation.GetNearestNeighbourInterpolation(currBitmap, xMax, yMax, currX, currY, false);
                    }
                    else
                    {
                        result = Interpolation.GetBiLinearInterpolatedValue(currBitmap, xMax, yMax, currX, currY, false);
                    }

                    resultImage.SetPixel(x, y, result);
                }
            }

            return resultImage;
        }
    }
}
