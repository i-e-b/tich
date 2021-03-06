namespace libtich;

/// <summary>
/// Command byte values in storage
/// </summary>
public enum Command : byte
{
    // TODO: IEB: The stack and param descriptions are out-of-date. Only 'Scalar' has params.
    
    
    /// <summary>
    /// Not a valid command
    /// </summary>
    Invalid = 0,
    
    /// <summary>
    /// Represents a number value (stored differently to commands)
    /// </summary>
    Scalar,

    #region Math. Component-wise unless otherwise noted
    /// <summary> Cosine, stack:1->1, params:0 </summary>
    Cos,
    /// <summary> Arc cosine, stack:1->1, params:0 </summary>
    Acos,
    /// <summary> Arc tangent, stack:1->1, params:0 </summary>
    Atan,
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
    /// <summary> a % b, stack:2->1, params:0 </summary>
    Mod,
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

    #endregion
    
    #region Comparisons. Outcomes are false=0, true=1

    /// <summary> (a &lt; b) ? 1 : 0, stack:2->1, params:0 </summary>
    Less,
    /// <summary> (a &gt; b) ? 1 : 0, stack:2->1, params:0 </summary>
    More,
    /// <summary> (a &lt;= b) ? 1 : 0, stack:2->1, params:0 </summary>
    LessEq,
    /// <summary> (a &gt;= b) ? 1 : 0, stack:2->1, params:0 </summary>
    MoreEq,
    /// <summary> (a == b) ? 1 : 0, stack:2->1, params:0; Equality is considered a difference less than 1??10?????? </summary>
    Equal,
    /// <summary> (a != b) ? 1 : 0, stack:2->1, params:0; Equality is considered a difference less than 1??10?????? </summary>
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

    #endregion

    #region Values (either special, or packing stack)
    
    /// <summary> the point being calculated, stack:0->1, params:0  </summary>
    P,
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
    
    #endregion

    #region Swizzles (picking and rearranging vector elements)

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

    #endregion
    
    #region Specials
    
    /// <summary> Interpolate between 2 values, choosing the midpoint, stack:2->1, params:0 </summary>
    Midpoint,
    
    /// <summary> Linear interpolate between 2 values, choosing the a scaled value where 0=a, 1=b, stack:2->1, params:1 </summary>
    Lerp,
    
    /// <summary> Sigmoid interpolate between 2 values, choosing the a scaled value where 0=a, 1=b, stack:2->1, params:1 </summary>
    SmoothStep,
    
    /// <summary> Given two vec2 that are any opposite corners of a rectangle, return a vec2 from the centre to one corner of a rectangle, stack:2->1, params:0 </summary>
    Rect,
    /// <summary> Component-wise minimum across 2 vectors, stack:2->1, params:0</summary>
    Lowest,
    /// <summary> Component-wise maximum across 2 vectors, stack:2->1, params:0</summary>
    Highest,
    /// <summary> Pick the maximum single component from a vector, return as scalar, stack:1->1, params:0</summary>
    MaxComponent,
    
    /// <summary> Given an angle in radians, return a unit vec2 representing that angle, stack:1->1, params:0 </summary>
    Angle,
    
    #endregion
    
    #region Register commands ([A,B,C,D,E,F,G,H,I,J,K,L,M,N] for temp variables etc. Registers all start as scalar zero)
    
    /// <summary> Update the value of P for rest of the calculation </summary>
    MoveP,
    
    /// <summary> Update the value of A for rest of the calculation </summary>
    SetA,
    /// <summary> Push current value of A onto stack </summary>
    GetA,
    
    /// <summary> Update the value of B for rest of the calculation </summary>
    SetB,
    /// <summary> Push current value of B onto stack </summary>
    GetB,
    
    /// <summary> Update the value of C for rest of the calculation </summary>
    SetC,
    /// <summary> Push current value of C onto stack </summary>
    GetC,
    
    /// <summary> Update the value of D for rest of the calculation </summary>
    SetD,
    /// <summary> Push current value of D onto stack </summary>
    GetD,
    
    /// <summary> Update the value of E for rest of the calculation </summary>
    SetE,
    /// <summary> Push current value of E onto stack </summary>
    GetE,
    
    /// <summary> Update the value of F for rest of the calculation </summary>
    SetF,
    /// <summary> Push current value of F onto stack </summary>
    GetF,
    
    /// <summary> Update the value of G for rest of the calculation </summary>
    SetG,
    /// <summary> Push current value of G onto stack </summary>
    GetG,
    
    /// <summary> Update the value of H for rest of the calculation </summary>
    SetH,
    /// <summary> Push current value of H onto stack </summary>
    GetH,
    
    /// <summary> Update the value of I for rest of the calculation </summary>
    SetI,
    /// <summary> Push current value of I onto stack </summary>
    GetI,
    
    /// <summary> Update the value of J for rest of the calculation </summary>
    SetJ,
    /// <summary> Push current value of J onto stack </summary>
    GetJ,
    
    /// <summary> Update the value of K for rest of the calculation </summary>
    SetK,
    /// <summary> Push current value of K onto stack </summary>
    GetK,
    
    /// <summary> Update the value of L for rest of the calculation </summary>
    SetL,
    /// <summary> Push current value of L onto stack </summary>
    GetL,
    
    /// <summary> Update the value of M for rest of the calculation </summary>
    SetM,
    /// <summary> Push current value of M onto stack </summary>
    GetM,
    
    /// <summary> Update the value of N for rest of the calculation </summary>
    SetN,
    /// <summary> Push current value of N onto stack </summary>
    GetN,
    
    #endregion
    
    // any extra commands here. can't have more than 127 values, as the top bit is a flag.
}