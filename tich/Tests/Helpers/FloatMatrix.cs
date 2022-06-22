using System.Drawing;
using System.Drawing.Imaging;

namespace Tests.Helpers;

public static class FloatMatrix
{
    public static void ToImage(this double[,] src, string path)
    {
        var w = src.GetLength(0);
        var h = src.GetLength(1);
        using var bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var v = (int)(src[x,y] * 255.0);
                bmp.SetPixel(x,y, Color.FromArgb(v,v,v));
            }
        }
        
        bmp.SaveBmp(path);
    }
    
    public static void SaveBmp(this Bitmap src, string filePath)
    {
        var p = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(p))
        {
            Directory.CreateDirectory(p);
        }
        if (File.Exists(filePath)) File.Delete(filePath);
        src.Save(filePath, ImageFormat.Bmp);
    }

}
