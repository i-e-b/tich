namespace libtich;

/// <summary>
/// Command byte values in storage
/// </summary>
public enum Command : byte
{
    /// <summary>
    /// Not a valid command
    /// </summary>
    [ArgCount(0)]
    Invalid = 0,

    // Math. Component-wise unless otherwise noted
    /// <summary> Cosine, stack:1->1, params:0 </summary>
    [ArgCount(0)]
    Cos,
    /// <summary> Arc cosine, stack:1->1, params:0 </summary>
    [ArgCount(0)]
    Acos,
    /// <summary> Sine, stack:1->1, params:0 </summary>
    [ArgCount(0)]
    Sin,
    /// <summary> Absolute value, stack:1->1, params:0 </summary>
    [ArgCount(0)]
    Abs,
    /// <summary> Square root, stack:1->1, params:0 </summary>
    [ArgCount(0)]
    Sqrt,
    /// <summary> Negate value, stack:1->1, params:0 </summary>
    [ArgCount(0)]
    Neg,
    /// <summary> Length of vector, no-op for scalars, stack:1->1, params:0 </summary>
    [ArgCount(0)]
    Length,
    /// <summary> Sign of components (-1,0,1), stack:1->1, params:0 </summary>
    [ArgCount(0)]
    Sign,
    /// <summary> 1/value, stack:1->1, params:0 </summary>
    [ArgCount(0)]
    Reciprocal,
    /// <summary> Adjust all components to make length of vector = 1, no-op for scalars and zero-length vectors, stack:1->1, params:0 </summary>
    [ArgCount(0)]
    Normalise,
    
    /// <summary> Maximum values, stack:2->1, params:0; vec/scalar does component-wise max. scalar/scalar does pick max. vec/vec does component-wise max in pairs </summary>
    [ArgCount(0)]
    Max,
    /// <summary> Minimum values, stack:2->1, params:0; vec/scalar does component-wise min. scalar/scalar does pick min. vec/vec does component-wise min in pairs </summary>
    [ArgCount(0)]
    Min,
    /// <summary> a + b, stack:2->1, params:0 </summary>
    [ArgCount(0)]
    Add,
    /// <summary> a - b, stack:2->1, params:0 </summary>
    [ArgCount(0)]
    Sub,
    /// <summary> a / b, stack:2->1, params:0 </summary>
    [ArgCount(0)]
    Div,
    /// <summary> a % b, stack:2->1, params:0 </summary>
    [ArgCount(0)]
    Mod,
    /// <summary> a * b, stack:2->1, params:0 </summary>
    [ArgCount(0)]
    Mul,
    /// <summary> a ^ b, stack:2->1, params:0 </summary>
    [ArgCount(0)]
    Pow,
    /// <summary> dot product a &#183; b, stack:2->1, params:0 </summary>
    [ArgCount(0)]
    Dot,
    /// <summary> cross product a &#215; b, stack:2->1, params:0 </summary>
    [ArgCount(0)]
    Cross,
    
    /// <summary> a clamped to p1..p2, stack:1->1, params:2 </summary>
    [ArgCount(2)]
    Clamp,

    /// <summary> multiply vec2 by matrix4, stack:1->1, params:4 </summary>
    [ArgCount(4)]
    MatrixMul,

    // Comparisons. Outcomes are false=0, true=1

    /// <summary> (a &lt; b) ? 1 : 0, stack:2->1, params:0 </summary>
    [ArgCount(0)]
    Less,
    /// <summary> (a &gt; b) ? 1 : 0, stack:2->1, params:0 </summary>
    [ArgCount(0)]
    More,
    /// <summary> (a &lt;= b) ? 1 : 0, stack:2->1, params:0 </summary>
    [ArgCount(0)]
    LessEq,
    /// <summary> (a &gt;= b) ? 1 : 0, stack:2->1, params:0 </summary>
    [ArgCount(0)]
    MoreEq,
    /// <summary> (a == b) ? 1 : 0, stack:2->1, params:0; Equality is considered a difference less than 1×10⁻⁶ </summary>
    [ArgCount(0)]
    Equal,
    /// <summary> (a != b) ? 1 : 0, stack:2->1, params:0; Equality is considered a difference less than 1×10⁻⁶ </summary>
    [ArgCount(0)]
    NotEq,

    /// <summary> 1 if all components are non-zero else 0, stack:1->1, params:0 </summary>
    [ArgCount(0)]
    All,
    /// <summary> 1 if all components are zero else 0, stack:1->1, params:0 </summary>
    [ArgCount(0)]
    None,
    /// <summary> (a != 0 &amp;&amp; b != 0) ? 1 : 0, stack:2->1, params:0  </summary>
    [ArgCount(0)]
    And,
    /// <summary> (a != 0 || b != 0) ? 1 : 0, stack:2->1, params:0  </summary>
    [ArgCount(0)]
    Or,
    /// <summary> (a == 0) ? 1 : 0, stack:1->1, params:0  </summary>
    [ArgCount(0)]
    Not,


    // Values (from params to stack)
    
    /// <summary> the point being calculated, stack:0->1, params:0  </summary>
    [ArgCount(0)]
    P,
    /// <summary> param onto stack as scalar, stack:0->1, params:1  </summary>
    [ArgCount(1)]
    Scalar,
    /// <summary> param onto stack as vec2, stack:0->1, params:2  </summary>
    [ArgCount(2)]
    Vec2,
    /// <summary> param onto stack as vec3, stack:0->1, params:3  </summary>
    [ArgCount(3)]
    Vec3,
    /// <summary> param onto stack as vec4, stack:0->1, params:4  </summary>
    [ArgCount(4)]
    Vec4,
    /// <summary> scalar 0 onto stack, stack:0->1, params:0  </summary>
    [ArgCount(0)]
    ZeroS,
    /// <summary> vec2 {0,0} onto stack, stack:0->1, params:0  </summary>
    [ArgCount(0)]
    ZeroV2,
    /// <summary> vec3 {0,0,0} onto stack, stack:0->1, params:0  </summary>
    [ArgCount(0)]
    ZeroV3,
    /// <summary> vec4 {0,0,0,0} onto stack, stack:0->1, params:0  </summary>
    [ArgCount(0)]
    ZeroV4,
    /// <summary> scalar 1 onto stack, stack:0->1, params:0  </summary>
    [ArgCount(0)]
    OneS,
    /// <summary> vec2 {1,1} onto stack, stack:0->1, params:0  </summary>
    [ArgCount(0)]
    OneV2,
    /// <summary> vec3 {1,1,1} onto stack, stack:0->1, params:0  </summary>
    [ArgCount(0)]
    OneV3,
    /// <summary> vec4 {1,1,1,1} onto stack, stack:0->1, params:0  </summary>
    [ArgCount(0)]
    OneV4,

    // Swizzles (picking and rearranging vector elements)

    /// <summary>
    /// pick p1 of any vector into a scalar, stack:1->1, params:1; vector slots are {X=0, Y=1, Z=2, W=3}
    ///<para>e.g. Swz2{1} is equivalent to vec.Y in GLSL</para>
    /// </summary>
    [ArgCount(1)]
    Swz1,
    /// <summary>
    /// pick p1 and p2 of any vector into vec2 {p1,p2}, stack:1->1, params:2; vector slots are {X=0, Y=1, Z=2, W=3}
    ///<para>e.g. Swz2{1,0} is equivalent to vec.YX in GLSL</para>
    /// </summary>
    [ArgCount(2)]
    Swz2,
    /// <summary>
    /// pick p1, p2, and p3 of any vector into vec2 {p1,p2,p3}, stack:1->1, params:3; vector slots are {X=0, Y=1, Z=2, W=3}
    ///<para>e.g. Swz3{1,0,1} is equivalent to vec.YXY in GLSL</para>
    /// </summary>
    [ArgCount(3)]
    Swz3,
    /// <summary>
    /// pick p1, p2, and p3 of any vector into vec2 {p1,p2,p3}, stack:1->1, params:4; vector slots are {X=0, Y=1, Z=2, W=3}
    ///<para>e.g. Swz4{0,1,0,3} is equivalent to vec.XYXW in GLSL</para>
    /// </summary>
    [ArgCount(4)]
    Swz4,

    /// <summary>
    /// pick p1 and p2 of any vector into vec2 {p1,p2} and also p3 into a scalar, stack:1->2, params:3; vector slots are {X=0, Y=1, Z=2, W=3}
    ///<para>e.g. SwzSplit3{0,1,2} is equivalent to (vec.XY, vec.Z) in GLSL</para>
    /// </summary>
    [ArgCount(3)]
    SwzSplit3,

    // Specials
    
    /// <summary> Interpolate between 2 values, choosing the midpoint, stack:2->1, params:0 </summary>
    [ArgCount(0)]
    Midpoint,
    
    /// <summary> Linear interpolate between 2 values, choosing the a scaled value where 0=a, 1=b, stack:2->1, params:1 </summary>
    [ArgCount(1)]
    Lerp,
    
    /// <summary> Sigmoid interpolate between 2 values, choosing the a scaled value where 0=a, 1=b, stack:2->1, params:1 </summary>
    [ArgCount(1)]
    SmoothStep,
    
    /// <summary> Given a vec4 for T/L/B/R edges, return a vector from the centre to one corner of a rectangle, stack:1->1, params:0 </summary>
    [ArgCount(0)]
    Rect,
    /// <summary> Component-wise minimum across 2 vectors, stack:2->1, params:0</summary>
    [ArgCount(0)]
    Lowest,
    /// <summary> Component-wise maximum across 2 vectors, stack:2->1, params:0</summary>
    [ArgCount(0)]
    Highest,
    /// <summary> Pick the maximum single component from a vector, return as scalar, stack:1->1, params:0</summary>
    [ArgCount(0)]
    MaxComponent,
    
    /// <summary> Given an angle in radians, return a unit vec2 representing that angle, stack:1->1, params:0 </summary>
    [ArgCount(0)]
    Angle,
    
    // Unused slots. We store only 6 bits and use the other 2 to set the param type
    /// <summary>
    /// Reserved for future use
    /// </summary>
    [ArgCount(0)]
    Spare0,
    
    /// <summary>
    /// Reserved for future use
    /// </summary>
    [ArgCount(0)]
    Spare1, 
    
    /// <summary>
    /// Reserved for future use
    /// </summary>
    [ArgCount(0)]
    Spare3,
    
    /// <summary>
    /// Reserved for future use
    /// </summary>
    [ArgCount(0)]
    Spare4
}

/// <summary>
/// Number of arguments for a command
/// </summary>
public class ArgCountAttribute : Attribute
{
    /// <summary>
    /// Number of arguments for a command
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Number of arguments for a command
    /// </summary>
    public ArgCountAttribute(int count)
    {
        Count = count;
    }
}