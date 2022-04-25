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
    /// True if the token has no symbol
    /// </summary>
    public bool IsEmpty => String.IsNullOrWhiteSpace(Value);

    /// <summary>
    /// Token for a string
    /// </summary>
    /// <param name="value"></param>
    public Token(string value)
    {
        Value = value;
        Class = value.Class();
        Precedence = value.Precedence();
        Direction = value.Associativity();
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
}