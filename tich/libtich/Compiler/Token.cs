namespace libtich;

/// <summary>
/// Holds token information
/// </summary>
public class Token
{
    /// <summary> Kind of token </summary>
    public TokenClass Class { get; set; }
    /// <summary> Binding order </summary>
    public int Precedence { get; set; }
    /// <summary> Value in expression </summary>
    public string Value { get; set; }
    /// <summary> left or right binding </summary>
    public Association Direction { get; set; }

    /// <summary>
    /// Number of parameters, if this token is function-like.
    /// </summary>
    public int ParameterCount { get; set; }

    /// <summary>
    /// True if the token has no symbol
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

    /// <summary>
    /// Numeric value, if applicable
    /// </summary>
    public double Number { get; set; }

    /// <summary>
    /// Return a new token that represents an invalid internal state
    /// </summary>
    public static Token Invalid => new() {Class = TokenClass.Nothing, Value = ""};

    /// <summary>
    /// returns true if this token is a 'nothing' class
    /// </summary>
    public bool IsNothing => Class == TokenClass.Nothing;

    /// <summary>
    /// The value of this function should be negated
    /// </summary>
    public bool Negated { get; set; }

    /// <summary>
    /// Token for a string
    /// </summary>
    /// <param name="value"></param>
    public Token(string value)
    {
        Value = value;
        if (double.TryParse(value, out var number)) Class = TokenClass.Operand;
        else Class = value.Class();
        Number = number;
        Precedence = value.Precedence();
        Direction = value.Associativity();
    }

    /// <summary>
    /// Empty token
    /// </summary>
    internal Token()
    {
        Value = "";
    }

    /// <summary>
    /// Compare precedence, taking associativity into account
    /// </summary>
    public bool ShouldDisplace(Token other)
    {
        if (other.Class != TokenClass.BinaryOperator) return false;
        if (Direction == Association.LeftToRight)
        {
            return other.Precedence >= Precedence;
        }

        return other.Precedence > Precedence;
    }

    /// <summary>
    /// Return original expression value
    /// </summary>
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    /// Return original expression value
    /// </summary>
    public static implicit operator string(Token t)
    {
        return t.Value;
    }

    /// <summary>
    /// Create an operand token for a number value
    /// </summary>
    public static Token ForScalar(double number)
    {
        return new Token
        {
            Class = TokenClass.Operand,
            Precedence = 0,
            Value = "",
            Direction = Association.LeftToRight,
            ParameterCount = 0,
            Number = number
        };
    }

    /// <summary>
    /// Make a new token with same settings as ours, but a different value
    /// </summary>
    public Token Split(string newName)
    {
        var result = new Token(newName)
        {
            Class = Class,
            Precedence = Precedence,
            Direction = Direction,
            ParameterCount = ParameterCount,
            Number = Number
        };
        return result;
    }

    /// <summary>
    /// Create a token for an injected function call. This is given a high precedence to prevent it being moved.
    /// </summary>
    public static Token ForFunction(string name, int parameterCount)
    {
        return new Token
        {
            Class = TokenClass.Function,
            Precedence = 100,
            Value = name,
            Direction = Association.LeftToRight,
            ParameterCount = parameterCount,
            Number = 0
        };
    }
}