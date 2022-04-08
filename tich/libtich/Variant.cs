namespace libtich;

/// <summary>
/// Represents a value in one of the possible types in the program
/// </summary>
public class Variant
{
    /// <summary>
    /// Values held
    /// </summary>
    public readonly double[] Values = new double[4];
    
    /// <summary>
    /// How many values are valid?
    /// 1= Scalar, 2=Vec2, 3=Vec3, 4=Vec4
    /// </summary>
    public int Width = 0;

    public double X { get => Values[0]; set => Values[0] = value;}
    public double Y { get => Values[1]; set => Values[1] = value;}
    public double Z { get => Values[2]; set => Values[2] = value;}
    public double W { get => Values[3]; set => Values[3] = value;}

    public static Variant Scalar(double d)
    {
        var v = new Variant
        {
            Values = { [0] = d },
            Width = 1
        };
        return v;
    }

    public static Variant Vec2(double x, double y)
    {
        return new Variant
        {
            Values = { [0] = x, [1] = y },
            Width = 2
        };
    }
    
    public static Variant Vec3(double x, double y, double z)
    {
        return new Variant
        {
            Values = { [0] = x, [1] = y, [2] = z },
            Width = 3
        };
    }
    
    public static Variant Vec4(double x, double y, double z, double w)
    {
        return new Variant
        {
            Values = { [0] = x, [1] = y, [2] = z, [3] = w },
            Width = 4
        };
    }
}