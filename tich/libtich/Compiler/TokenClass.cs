namespace libtich;

/// <summary>
/// Kind of token
/// </summary>
public enum TokenClass
{
    /// <summary>
    /// A value expressed as a numeric string or a constant name
    /// </summary>
    Operand,
    /// <summary>
    /// An equality symbol
    /// </summary>
    Equality,
    /// <summary>
    /// Implicit unary function to the right of a value
    /// </summary>
    UniaryPostfix,
    /// <summary>
    /// Implicit unary function to the left of a value
    /// </summary>
    UniaryPrefix,
    /// <summary>
    /// Interstitial in an argument list
    /// </summary>
    ArgumentSeparator,
    /// <summary>
    /// Implicit binary function defined by precedence rules
    /// </summary>
    BinaryOperator,
    /// <summary>
    /// Parenthesis in an expression, or start of function arguments 
    /// </summary>
    OpenBracket,
    /// <summary>
    /// Parenthesis in an expression, or end of function arguments 
    /// </summary>
    CloseBracket,
    /// <summary>
    /// Explicit variadic function by name
    /// </summary>
    Function,
    /// <summary>
    /// Name of a variable or anything else unknown
    /// </summary>
    Name // For variables and anything unknown.
}