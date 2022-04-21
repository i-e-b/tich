namespace libtich;

/// <summary>
/// Holds token information
/// </summary>
public class Token
{
    public TokenClass Class { get; set; }
    public int Precedence { get; set; }
    public string Value { get; set; }
    public Association Direction { get; set; }

    public bool IsEmpty
    {
        get { return String.IsNullOrEmpty(Value); }
    }

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

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(Token t)
    {
        return t.Value;
    }
}