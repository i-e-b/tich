namespace libtich;

/// <summary>
/// Math against Variants.
/// Note that parameters are in order to make Pop() easier to use
/// </summary>
internal class VMath
{
    /// <summary>
    /// The threshold for equality comparisons. Also the zero/non-zero threshold
    /// </summary>
    public const double EqualityDifference = 1E-6;

    public static Variant Cos(Variant v)
    {
        return Variant.Scalar(Math.Cos(v.Values[0]));
    }

    public static Variant Acos(Variant v)
    {
        return Variant.Scalar(Math.Acos(v.Values[0]));
    }
    
    public static Variant Atan(Variant b, Variant a)
    {
        return Variant.Scalar(Math.Atan2(a.Values[0],b.Values[0]));
    }

    public static Variant Sin(Variant v)
    {
        return Variant.Scalar(Math.Sin(v.Values[0]));
    }

    public static Variant Abs(Variant v)
    {
        for (int i = 0; i < v.Width; i++)
        {
            v.Values[i] = Math.Abs(v.Values[i]);
        }
        return v;
    }

    public static Variant Sqrt(Variant v)
    {
        for (int i = 0; i < v.Width; i++)
        {
            v.Values[i] = Math.Sqrt(v.Values[i]);
        }
        return v;
    }

    public static Variant Neg(Variant v)
    {
        for (int i = 0; i < v.Width; i++)
        {
            v.Values[i] = -v.Values[i];
        }
        return v;
    }

    public static Variant Length(Variant v)
    {
        var sum = 0.0;
        
        for (int i = 0; i < v.Width; i++)
        {
            sum += v.Values[i] * v.Values[i];
        }
        return Variant.Scalar(Math.Sqrt(sum));
    }

    public static Variant Sign(Variant v)
    {
        for (int i = 0; i < v.Width; i++)
        {
            v.Values[i] = Math.Sign(v.Values[i]);
        }
        return v;
    }

    public static Variant Reciprocal(Variant v)
    {
        for (int i = 0; i < v.Width; i++)
        {
            if (v.Values[i] == 0.0) continue;
            v.Values[i] = 1.0 / v.Values[i];
        }
        return v;
    }

    public static Variant VectorNormal(Variant v)
    {
        var sum = 0.0;
        
        for (int i = 0; i < v.Width; i++)
        {
            sum += v.Values[i] * v.Values[i];
        }

        if (sum == 0) { return v; }

        var length = Math.Sqrt(sum);
        
        for (int i = 0; i < v.Width; i++)
        {
            v.Values[i] /= length;
        }
        return v;
        
    }

    public static Variant PairwiseMax(Variant b, Variant a)
    {
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] = Math.Max(a.Values[i], b.Values[i]);
        }
        return a;
    }

    private static int Max(int a, int b) => a > b ? a : b;

    public static Variant PairwiseMin(Variant b, Variant a)
    {
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] = Math.Min(a.Values[i], b.Values[i]);
        }
        return a;
    }

    public static Variant PairwiseAdd(Variant b, Variant a)
    {
        if (b.Width == 1) // vector + scalar; we smear the scalar first
            b.W = b.Z = b.Y = b.X;
        
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] += b.Values[i];
        }
        return a;
    }

    public static Variant PairwiseSubtract(Variant b, Variant a)
    {
        if (b.Width == 1) // vector - scalar; we smear the scalar first
            b.W = b.Z = b.Y = b.X;
        
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] -= b.Values[i];
        }
        
        return a;
    }

    public static Variant PairwiseDivideFloor(Variant b, Variant a)
    {
        if (b.Width == 1) // vector / scalar; we smear the scalar first
            b.W = b.Z = b.Y = b.X;
        
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            var vb = b.Values[i];
            a.Values[i] = vb == 0 ? 0 : a.Values[i] / vb;
        }
        return a;
    }

    public static Variant PairwiseModulo(Variant b, Variant a)
    {
        if (b.Width == 1) // vector % scalar; we smear the scalar first
            b.W = b.Z = b.Y = b.X;
        
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            var vb = b.Values[i];
            a.Values[i] = vb == 0 ? a.Values[i] : a.Values[i] % vb;
        }
        return a;
    }

    public static Variant PairwiseMultiply(Variant b, Variant a)
    {
        if (b.Width == 1) // vector * scalar; we smear the scalar first
            b.W = b.Z = b.Y = b.X;
        
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] *= b.Values[i];
        }
        
        return a;
    }

    public static Variant ComponentwisePower(Variant b, Variant a)
    {
        if (b.Width == 1) // vector ^ scalar; we smear the scalar first
            b.W = b.Z = b.Y = b.X;
        
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] = Math.Pow(a.Values[i], b.Values[i]);
        }
        return a;
    }
    
    // ReSharper disable UnusedMember.Local
    private const int X = 0;
    private const int Y = 1;
    private const int Z = 2;
    private const int W = 3;
    // ReSharper restore UnusedMember.Local

    public static Variant DotProduct(Variant b, Variant a)
    {
        if (a.Width != 2 || b.Width != 2) return Variant.Scalar(0);
        
        return Variant.Scalar(a.Values[X] * b.Values[X] + a.Values[Y] * b.Values[Y]);
    }

    public static Variant CrossProduct(Variant b, Variant a)
    {
        if (a.Width != 2 || b.Width != 2) return Variant.Scalar(0);
        
        return Variant.Scalar(a.Values[X] * b.Values[Y] + a.Values[Y] * b.Values[X]);
    }

    public static Variant Clamp(Variant v, double lower, double upper)
    {
        for (int i = 0; i < v.Width; i++)
        {
            if (v.Values[i] > upper) v.Values[i] = upper;
            else if (v.Values[i] < lower) v.Values[i] = lower;
        }
        return v;
    }

    public static Variant MatrixMul4(Variant v, double[] m)
    {
        v.X = m[0] * v.X + m[2] * v.Y;
        v.Y = m[1] * v.X + m[3] * v.Y;
        return v;
    }

    public static Variant CompareLess(Variant b, Variant a)
    {
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] = a.Values[i] < b.Values[i] ? 1 : 0;
        }
        return a;
    }
    public static Variant CompareLessEqual(Variant b, Variant a)
    {
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] = a.Values[i] <= b.Values[i] ? 1 : 0;
        }
        return a;
    }

    public static Variant CompareGreater(Variant b, Variant a)
    {
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] = a.Values[i] > b.Values[i] ? 1 : 0;
        }
        return a;
    }
    public static Variant CompareGreaterEqual(Variant b, Variant a)
    {
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] = a.Values[i] >= b.Values[i] ? 1 : 0;
        }
        return a;
    }
    public static Variant CompareEqual(Variant b, Variant a)
    {
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] = Math.Abs(a.Values[i] - b.Values[i]) < EqualityDifference ? 1 : 0;
        }
        return a;
    }
    public static Variant CompareNotEqual(Variant b, Variant a)
    {
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] = Math.Abs(a.Values[i] - b.Values[i]) > EqualityDifference ? 1 : 0;
        }
        return a;
    }
    
    public static Variant ComponentAllNonZero(Variant v)
    {
        var all = true;
        for (int i = 0; i < v.Width; i++)
        {
            all = all && (Math.Abs(v.Values[i]) >= EqualityDifference);
        }
        return Variant.Scalar(all ? 1 : 0);
    }
    public static Variant ComponentAllZero(Variant v)
    {
        var all = true;
        for (int i = 0; i < v.Width; i++)
        {
            all = all && (Math.Abs(v.Values[i]) < EqualityDifference);
        }
        return Variant.Scalar(all ? 1 : 0);
    }

    public static Variant ComponentwiseLogicalAnd(Variant b, Variant a)
    {
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] = ((Math.Abs(a.Values[i]) >= EqualityDifference) && (Math.Abs(b.Values[i]) >= EqualityDifference)) ? 1 : 0;
        }
        return a;
    }
    public static Variant ComponentwiseLogicalOr(Variant b, Variant a)
    {
        a.Width = Max(a.Width, b.Width);
        for (int i = 0; i < a.Width; i++)
        {
            a.Values[i] = ((Math.Abs(a.Values[i]) >= EqualityDifference) || (Math.Abs(b.Values[i]) >= EqualityDifference)) ? 1 : 0;
        }
        return a;
    }

    public static Variant ComponentwiseLogicalNot(Variant v)
    {
        for (int i = 0; i < v.Width; i++)
        {
            v.Values[i] = (Math.Abs(v.Values[i]) < EqualityDifference) ? 1 : 0;
        }
        return v;
    }

    public static Variant Swizzle(Variant v, double[] indexes)
    {
        var r = new Variant { Width = indexes.Length };
        
        for (int i = 0; i < indexes.Length; i++)
        {
            var j = (int)indexes[i];
            if (j < 0 || j > 3) continue;
            r.Values[i] = v.Values[j];
        }
        return r;
    }

    public static Variant SwizzleSplit(Variant src, double[] indexes, out Variant split)
    {
        var a = (int)indexes[0];
        var b = (int)indexes[1];
        var c = (int)indexes[2];

        var vA = (a > 3 || a < 0) ? 0 : src.Values[a];
        var vB = (b > 3 || b < 0) ? 0 : src.Values[b];
        var vC = (c > 3 || c < 0) ? 0 : src.Values[c];
        
        split = Variant.Scalar(vC);
        return Variant.Vec2(vA, vB);
    }

    public static Variant Lerp(Variant b, Variant a, double prop)
    {
        if (prop == 0.0) return a;
        if (Math.Abs(prop - 1.0) < EqualityDifference) return b;

        /* var c = (b-a) * prop;
         c += a;
         */

        var c = PairwiseSubtract(a.Copy(), b); // remember parameters are backwards!
        c = PairwiseMultiply(Variant.Scalar(prop), c);
        c = PairwiseAdd(a, c);

        return c;
    }

    public static Variant ComponentMax(Variant v)
    {
        var m = v.X;
        for (int i = 1; i < v.Width; i++)
        {
            m = Math.Max(m, v.Values[i]);
        }
        return Variant.Scalar(m);
    }

    public static Variant RectangleVector(Variant vb, Variant va)
    {
        var l = Math.Min(va.X, vb.X);
        var r = Math.Max(va.X, vb.X);
        var t = Math.Min(va.Y, vb.Y);
        var b = Math.Max(va.Y, vb.Y);
            
        var hw = (r-l)/2.0;
        var hh = (b-t)/2.0;
            
        return Variant.Vec2(hw, hh);
    }

    public static Variant Angle(Variant v)
    {
        var radians = v.X;
        return Variant.Vec2(Math.Cos(radians), Math.Sin(radians));
    }

    public static double Gain(double x, double k) 
    {
        var a = 0.5 * Math.Pow(2.0 * ((x < 0.5) ? x : 1.0 - x), k);
        return (x < 0.5) ? a : 1.0 - a;
    }
}