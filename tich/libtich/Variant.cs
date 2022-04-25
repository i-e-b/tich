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
    /// Output a human readable representation of this variant
    /// </summary>
    public override string ToString()
    {
        switch (Width)
        {
            case 4: return $"({X},{Y},{Z},{W})";
            case 3: return $"({X},{Y},{Z})";
            case 2: return $"({X},{Y})";
            case 1: return $"{X}";
            default: return "?";
        }
    }

    /// <summary>
    /// How many values are valid?
    /// 1= Scalar, 2=Vec2, 3=Vec3, 4=Vec4
    /// </summary>
    public int Width = 0;

    /// <summary> X value or zero </summary>
    public double X { get => Values[0]; set => Values[0] = value;}
    /// <summary> Y value or zero </summary>
    public double Y { get => Values[1]; set => Values[1] = value;}
    /// <summary> Z value or zero </summary>
    public double Z { get => Values[2]; set => Values[2] = value;}
    /// <summary> W value or zero </summary>
    public double W { get => Values[3]; set => Values[3] = value;}

    /// <summary>
    /// Wrap a single value in a variant
    /// </summary>
    public static Variant Scalar(double d)
    {
        var v = new Variant
        {
            Values = { [0] = d },
            Width = 1
        };
        return v;
    }

    /// <summary>
    /// Two part variant
    /// </summary>
    public static Variant Vec2(double x, double y)
    {
        return new Variant
        {
            Values = { [0] = x, [1] = y },
            Width = 2
        };
    }
    
    /// <summary>
    /// three part variant
    /// </summary>
    public static Variant Vec3(double x, double y, double z)
    {
        return new Variant
        {
            Values = { [0] = x, [1] = y, [2] = z },
            Width = 3
        };
    }
    
    /// <summary>
    /// four part variant
    /// </summary>
    public static Variant Vec4(double x, double y, double z, double w)
    {
        return new Variant
        {
            Values = { [0] = x, [1] = y, [2] = z, [3] = w },
            Width = 4
        };
    }
}