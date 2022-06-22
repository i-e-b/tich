namespace libtich;

/// <summary>
/// Render a tich program to an image
/// </summary>
public static class TichRenderer
{
    /// <summary>
    /// Render a single layer's sub-program to a rectangular array.
    /// Values in the array are between 0.0 (entirely out of the object)
    /// and 1.0 (entirely inside the object);
    ///
    /// P value of (0,0) is in the centre of the array.
    ///
    /// 
    /// </summary>
    public static double[,] RenderLayer(TichProgram prog, int width, int height, double dx, double dy)
    {
        // simple scan-wise stepping algorithm
        var outp = new double[width,height];
        
        // for each horizontal line, we start at the left, and step by the absolute distance of the field
        // or 1 pixel, whichever is more.
        // the colour of the field is the distance from the edge pinned to a range of 0.0..1.0
        // this gives us a spherical window anti-aliasing at the edges (assuming the program is L-0 norm).

        for (int y = 0; y < height; y++)
        {
            int x = 0;
            while (x < width)
            {
                var dist = prog.CalculateForPoint(x-dx,y-dy);
                var step = Math.Max(1, (int)Math.Abs(dist));
                var end = Math.Min(width, x+step);
                
                var value = Math.Min(1.0, Math.Max(0.0, dist));
                    
                // fill gap to next point
                while (x < end)
                {
                    outp[x++, y] = value;
                }
            }
        }
        
        return outp;
    }
}