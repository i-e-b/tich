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
    public static double[,] RenderLayer(TichProgram prog, int width, int height)
    {
        // simple scan-wise stepping algorithm
        var outp = new double[width,height];
    }
}