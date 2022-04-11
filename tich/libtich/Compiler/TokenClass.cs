namespace libtich;

public enum TokenClass
{
    Operand,
    UniaryPostfix,
    UniaryPrefix,
    ArgumentSeparator,
    BinaryOperator,
    OpenBracket,
    CloseBracket,
    Function,
    Name // For variables and anything unknown.
}