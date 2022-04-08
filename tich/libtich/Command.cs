namespace libtich;

/// <summary>
/// Command byte values in storage
/// </summary>
public enum Command : byte
{
    /// <summary>
    /// Not a valid command
    /// </summary>
    Invalid = 0,
    
    /// <summary>
    /// End the program. The scalar at the top of the stack is our distance calculation.
    /// If the top value is a vector, the X component will be taken.
    /// If the stack is empty, an invalid result is given
    /// </summary>
    Exit,

    // Math. Component-wise unless otherwise noted
    /// <summary> Cosine, stack:1->1, params:0 </summary>
    Cos,
    /// <summary> Arc cosine, stack:1->1, params:0 </summary>
    Acos,
    /// <summary> Sine, stack:1->1, params:0 </summary>
    Sin,
    /// <summary> Absolute value, stack:1->1, params:0 </summary>
    Abs,
    /// <summary> Square root, stack:1->1, params:0 </summary>
    Sqrt,
    /// <summary> Negate value, stack:1->1, params:0 </summary>
    Neg,
    /// <summary> Length of vector, no-op for scalars, stack:1->1, params:0 </summary>
    Length,
    /// <summary> Sign of components (-1,0,1), stack:1->1, params:0 </summary>
    Sign,
    /// <summary> 1/value, stack:1->1, params:0 </summary>
    Reciprocal,
    /// <summary> Adjust all components to make length of vector = 1, no-op for scalars and zero-length vectors, stack:1->1, params:0 </summary>
    Normalise,
    
    /// <summary> Maximum values, stack:2->1, params:0; vec/scalar does component-wise max. scalar/scalar does pick max. vec/vec does component-wise max in pairs </summary>
    Max,
    /// <summary> Minimum values, stack:2->1, params:0; vec/scalar does component-wise min. scalar/scalar does pick min. vec/vec does component-wise min in pairs </summary>
    Min,
    /// <summary> a + b, stack:2->1, params:0 </summary>
    Add,
    /// <summary> a - b, stack:2->1, params:0 </summary>
    Sub,
    /// <summary> a / b, stack:2->1, params:0 </summary>
    Div,
    /// <summary> a * b, stack:2->1, params:0 </summary>
    Mul,
    /// <summary> a ^ b, stack:2->1, params:0 </summary>
    Pow,
    /// <summary> dot product a &#183; b, stack:2->1, params:0 </summary>
    Dot,
    /// <summary> cross product a &#215; b, stack:2->1, params:0 </summary>
    Cross,
    
    /// <summary> a clamped to p1..p2, stack:1->1, params:2 </summary>
    Clamp,

    /// <summary> multiply vec2 by matrix4, stack:1->1, params:4 </summary>
    MatrixMul,

    // Comparisons. Outcomes are false=0, true=1

    /// <summary> (a &lt; b) ? 1 : 0, stack:2->1, params:0 </summary>
    Less,
    /// <summary> (a &gt; b) ? 1 : 0, stack:2->1, params:0 </summary>
    More,
    /// <summary> (a &lt;= b) ? 1 : 0, stack:2->1, params:0 </summary>
    LessEq,
    /// <summary> (a &gt;= b) ? 1 : 0, stack:2->1, params:0 </summary>
    MoreEq,
    /// <summary> (a == b) ? 1 : 0, stack:2->1, params:0; Equality is considered a difference less than 1×10⁻²⁰ </summary>
    Equal,
    /// <summary> (a != b) ? 1 : 0, stack:2->1, params:0; Equality is considered a difference less than 1×10⁻²⁰ </summary>
    NotEq,

    /// <summary> 1 if all components are non-zero else 0, stack:1->1, params:0 </summary>
    All,
    /// <summary> 1 if all components are zero else 0, stack:1->1, params:0 </summary>
    None,
    /// <summary> (a != 0 &amp;&amp; b != 0) ? 1 : 0, stack:2->1, params:0  </summary>
    And,
    /// <summary> (a != 0 || b != 0) ? 1 : 0, stack:2->1, params:0  </summary>
    Or,
    /// <summary> (a == 0) ? 1 : 0, stack:1->1, params:0  </summary>
    Not,


    // Values (from params to stack)
    /// <summary> param onto stack as scalar, stack:0->1, params:1  </summary>
    Scalar,
    /// <summary> param onto stack as vec2, stack:0->1, params:2  </summary>
    Vec2,
    /// <summary> param onto stack as vec3, stack:0->1, params:3  </summary>
    Vec3,
    /// <summary> param onto stack as vec4, stack:0->1, params:4  </summary>
    Vec4,
    /// <summary> scalar 0 onto stack, stack:0->1, params:0  </summary>
    ZeroS,
    /// <summary> vec2 {0,0} onto stack, stack:0->1, params:0  </summary>
    ZeroV2,
    /// <summary> vec3 {0,0,0} onto stack, stack:0->1, params:0  </summary>
    ZeroV3,
    /// <summary> vec4 {0,0,0,0} onto stack, stack:0->1, params:0  </summary>
    ZeroV4,
    /// <summary> scalar 1 onto stack, stack:0->1, params:0  </summary>
    OneS,
    /// <summary> vec2 {1,1} onto stack, stack:0->1, params:0  </summary>
    OneV2,
    /// <summary> vec3 {1,1,1} onto stack, stack:0->1, params:0  </summary>
    OneV3,
    /// <summary> vec4 {1,1,1,1} onto stack, stack:0->1, params:0  </summary>
    OneV4,

    // Swizzles (picking and rearranging vector elements)

    /// <summary>
    /// pick p1 of any vector into a scalar, stack:1->1, params:1; vector slots are {X=0, Y=1, Z=2, W=3}
    ///<para>e.g. Swz2{1} is equivalent to vec.Y in GLSL</para>
    /// </summary>
    Swz1,
    /// <summary>
    /// pick p1 and p2 of any vector into vec2 {p1,p2}, stack:1->1, params:2; vector slots are {X=0, Y=1, Z=2, W=3}
    ///<para>e.g. Swz2{1,0} is equivalent to vec.YX in GLSL</para>
    /// </summary>
    Swz2,
    /// <summary>
    /// pick p1, p2, and p3 of any vector into vec2 {p1,p2,p3}, stack:1->1, params:3; vector slots are {X=0, Y=1, Z=2, W=3}
    ///<para>e.g. Swz3{1,0,1} is equivalent to vec.YXY in GLSL</para>
    /// </summary>
    Swz3,
    /// <summary>
    /// pick p1, p2, and p3 of any vector into vec2 {p1,p2,p3}, stack:1->1, params:4; vector slots are {X=0, Y=1, Z=2, W=3}
    ///<para>e.g. Swz4{0,1,0,3} is equivalent to vec.XYXW in GLSL</para>
    /// </summary>
    Swz4,

    /// <summary>
    /// pick p1 and p2 of any vector into vec2 {p1,p2} and also p3 into a scalar, stack:1->2, params:3; vector slots are {X=0, Y=1, Z=2, W=3}
    ///<para>e.g. SwzSplit3{0,1,2} is equivalent to (vec.XY, vec.Z) in GLSL</para>
    /// </summary>
    SwzSplit3,

    // Specials
    
    /// <summary> Interpolate between 2 values, choosing the midpoint, stack:2->1, params:0 </summary>
    Midpoint,
    
    /// <summary> Interpolate between 2 values, choosing the a scaled value where 0=a, 1=b, stack:2->1, params:1 </summary>
    Lerp,
    
    /// <summary> Given a vec4 for T/L/B/R edges, return a vector from the centre to one corner of a rectangle, stack:1->1, params:0 </summary>
    Rect,
    /// <summary> Component-wise minimum across 2 vectors, stack:2->1, params:0</summary>
    Lowest,
    /// <summary> Component-wise maximum across 2 vectors, stack:2->1, params:0</summary>
    Highest,
    /// <summary> Pick the maximum single component from a vector, return as scalar, stack:1->1, params:0</summary>
    MaxComponent,
    
    /// <summary> Given an angle in radians, return a unit vec2 representing that angle, stack:1->1, params:0 </summary>
    Angle,
}