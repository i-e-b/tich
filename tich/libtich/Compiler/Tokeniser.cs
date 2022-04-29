using System.Text.RegularExpressions;

namespace libtich;

/// <summary>
/// Split an input expression string into tokens 
/// </summary>
static class Tokeniser
{
    /// <summary>
    /// Split an expression into tokens
    /// </summary>
    public static List<Token> Tokens(this string input)
    {
        // (strings) | (nums, functions, vars) | (sci nums)
        var r = new Regex(@"(\['.*?'\])|([^0-9a-zA-Z.\$])|([0-9.]+e[+\-]?[0-9]+)");
        // anything but alpha numeric or decimals gets split
        var output = new List<Token>();
        // return splits, including split characters (because of capture group in regex)

        var frags = r.Split(input);

        foreach (var frag in frags)
        {
            if (string.IsNullOrEmpty(frag)) continue;

            var t = new Token(frag);

            var last = (output.Count > 0) ? (output.Last()) : (null);

            #region check for uniary prefix operators

            if (t.Class == TokenClass.BinaryOperator)
            {
                if (output.Count < 1)
                {
                    t.Class = TokenClass.UniaryPrefix;
                }
                else
                {
                    if (last != null)
                        switch (last.Class)
                        {
                            case TokenClass.BinaryOperator:
                            case TokenClass.UniaryPostfix:
                            case TokenClass.OpenBracket:
                            case TokenClass.ArgumentSeparator:
                                t.Class = TokenClass.UniaryPrefix;
                                break;
                        }
                }
            }

            #endregion

            #region check for functions

            // a name followed by an open bracket is taken to be a function
            if (t.Class == TokenClass.OpenBracket
             && last != null
             && output.Count > 0
             && last.Class == TokenClass.Name)
            {
                last.Class = TokenClass.Function;
                last.Value = "/" + last; // easier to find functions, plus vars and functions can share a name
            }

            #endregion
            
            #region check for name.name, and split
            if (t.Class == TokenClass.Name
                && t.Value.Contains('.'))
            {
                // note, we keep the dots, to understand the relationship later (we treat the second like a special function)
                var bits = t.Value.Split('.');
                for (int i = 0; i < bits.Length; i++)
                {
                    var prefix = i == 0 ? "" : ".";
                    output.Add(t.Split(prefix + bits[i]));
                }
                continue;
            }

            #endregion

            output.Add(t);
        }

        return output;
    }

    /// <summary>
    /// Determine the class of a token
    /// </summary>
    public static TokenClass Class(this string input)
    {
        switch (input.ToLowerInvariant())
        {
            case "+": return TokenClass.BinaryOperator;
            case "-": return TokenClass.BinaryOperator;
            case "*": return TokenClass.BinaryOperator;
            case "/": return TokenClass.BinaryOperator;
            case "^": return TokenClass.BinaryOperator;
            case "%": return TokenClass.BinaryOperator;
            case "&": return TokenClass.BinaryOperator;
            case "|": return TokenClass.BinaryOperator;
            
            case "!": return TokenClass.Equality;
            case "=": return TokenClass.Equality;
            case "<": return TokenClass.Equality;
            case ">": return TokenClass.Equality;
            
            case "(": return TokenClass.OpenBracket;
            case ")": return TokenClass.CloseBracket;
            
            case ",": return TokenClass.ArgumentSeparator;
            default: // not a simple token. Check for number or other
                double dummy;
                if (double.TryParse(input, out dummy)) return TokenClass.Operand; // inefficient, but gets the job done
                return TokenClass.Name; // no idea what this token is -- should be a var or func name.
        }
    }

    /// <summary>
    /// Determine the precedence of a token.
    /// Higher number are higher precedence
    /// </summary>
    public static int Precedence(this string input)
    {
        switch (input.ToLowerInvariant())
        {
            // separator
            case ",": return 1;
            
            // boolean
            case "|": return 2;
            case "&": return 2;
            
            // equality
            case "<": return 3;
            case ">": return 3;
            case "=": return 3;
            case "!": return 3;
            
            // math
            case "+": return 4;
            case "-": return 4;
            case "*": return 5;
            case "/": return 5;
            case "%": return 5;
            case "^": return 6;
            
            // swizzle
            case ".": return 7;
            
            // parenthesis
            case "(": return 8;
            case ")": return 8;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Determine the association direction of a token
    /// </summary>
    public static Association Associativity(this string input)
    {
        switch (input.ToLowerInvariant())
        {
            // L-R
            
            // separator
            case ",": return Association.LeftToRight;
            
            // boolean
            case "|": return Association.LeftToRight;
            case "&": return Association.LeftToRight;
            
            // equality
            case "<": return Association.LeftToRight;
            case ">": return Association.LeftToRight;
            case "=": return Association.LeftToRight;
            case "!": return Association.LeftToRight;
            
            // math
            case "+": return Association.LeftToRight;
            case "-": return Association.LeftToRight;
            case "*": return Association.LeftToRight;
            case "/": return Association.LeftToRight;
            case "%": return Association.LeftToRight;
            
            // swizzle
            case ".": return Association.LeftToRight;
            
            // parenthesis
            case "(": return Association.LeftToRight;
            case ")": return Association.LeftToRight;
            
            // R-L
            case "^": return Association.RightToLeft;
            
            default:
                return Association.LeftToRight;
        }
    }

    public static bool HasItems(this Stack<Token> input)
    {
        return (input.Count > 0 && input.Peek() != null);
    }
}