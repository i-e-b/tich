namespace libtich;

using System;
using System.Collections.Generic;

/// <summary>
/// Turns expression strings into Tich programs
/// </summary>
public class Compiler
{
    /// <summary>
    /// Compile a full program string (including sub-expressions and assignments)
    /// into a list of executable cells.
    /// </summary>
    public static IEnumerable<Cell> CompileProgram(string programString)
    {
        const int register = 0;
        const int expression = 1;
        const StringSplitOptions clean = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        
        
        var result = new List<Cell>();
        var expressions = programString.Split(';', clean);
        
        var exprCount = 0;

        for (int i = 0; i < expressions.Length - 1; i++) // all assignment sub-expressions
        {
            var assignment = expressions[i].Split(':', clean);
            if (assignment.Length != 2)
            {
                throw new Exception($"Invalid assignment. {assignment.Length} parts, should be 2. Too many ':' in sub-expression? '{expressions[i]}'");
            }

            var assignCell = MapRegister(assignment[register]);
            
            var postfix = InfixToPostfix(assignment[expression]);
            var code = CompilePostfix(postfix, exprCount == 0);
            result.AddRange(code);
            result.Add(assignCell);
            exprCount++;
        }
        
        // Last (or only) expression:
        result.AddRange(CompilePostfix(InfixToPostfix(expressions[^1]), exprCount == 0));
        return result;
    }

    private static Cell MapRegister(string s)
    {
        s = s.Trim().ToLowerInvariant();
        return s switch
        {
            "a" => new Cell { Cmd = Command.SetA },
            "b" => new Cell { Cmd = Command.SetB },
            "c" => new Cell { Cmd = Command.SetC },
            "d" => new Cell { Cmd = Command.SetD },
            "e" => new Cell { Cmd = Command.SetE },
            "f" => new Cell { Cmd = Command.SetF },
            "g" => new Cell { Cmd = Command.SetG },
            "h" => new Cell { Cmd = Command.SetH },
            "i" => new Cell { Cmd = Command.SetI },
            "j" => new Cell { Cmd = Command.SetJ },
            "k" => new Cell { Cmd = Command.SetK },
            "l" => new Cell { Cmd = Command.SetL },
            "m" => new Cell { Cmd = Command.SetM },
            "n" => new Cell { Cmd = Command.SetN },
            
            "p" => new Cell { Cmd = Command.MoveP },
            _ => throw new Exception($"Invalid assignment: no such register '{s}'")
        };
    }

    /// <summary>
    /// Transform an infix expression string into a set of postfix tokens.
    /// This handles order of association, parenthesis, etc.
    /// </summary>
    public static Stack<Token> InfixToPostfix(string expression)
    {
        bool ok;
        var operands = new Stack<Token>();
        var postfix = new Stack<Token>();

        var tokens = expression.Tokens();
        
        ok = DefineFunctionArity(tokens);
        if (!ok) throw new Exception("Unmatched braces in expression"); // TODO: better message
        
        tokens = FixupNegatedFunctions(tokens);
        
        foreach (var token in tokens)
        {
            if (token.IsEmpty) continue;

            switch (token.Class)
            {
                case TokenClass.Name:
                case TokenClass.Operand:

                    // Test for uniary operator
                    if (operands.HasItems()
                        && operands.Peek().Class == TokenClass.UniaryPrefix)
                    {
                        switch (operands.Peek().Value)
                        {
                            case "-":
                                // flip sign of value
                                operands.Pop();
                                if (token.Class == TokenClass.Operand)
                                {
                                    token.Number = -token.Number;
                                    token.Value = "-"+token.Value;
                                    postfix.PushNotEmpty(token);
                                }
                                else
                                {
                                    postfix.PushNotEmpty(token);
                                    postfix.Push(Token.ForFunction("/neg", 1));
                                }

                                break;
                            case "+":
                                // no change to operand, remove uniary
                                postfix.PushNotEmpty(token);
                                operands.Pop();
                                break;
                            default:
                                throw new Exception("Unexpected operator");
                        }
                    }
                    else // regular numeric value
                    {
                        postfix.Push(token);
                    }

                    break;
                case TokenClass.UniaryPostfix:
                    operands.Push(token);
                    break;
                case TokenClass.UniaryPrefix:
                    operands.Push(token);
                    break;
                case TokenClass.Function:
                case TokenClass.OpenBracket:

                    #region Test for uniary operator

                    if (operands.HasItems()
                        && operands.Peek().Class == TokenClass.UniaryPrefix)
                    {
                        switch (operands.Peek().Value)
                        {
                            case "-":
                                // change value, which will get picked up on bracket close
                                token.Value = ParenNeg;
                                operands.Pop(); // remove uniary
                                operands.PushNotEmpty(token); // push bracket
                                break;
                            case "+":
                                operands.Pop(); // remove uniary
                                operands.PushNotEmpty(token); // push bracket
                                break;
                            default:
                                throw new Exception("Unexpected operator");
                        }

                        #endregion
                    }
                    else
                    {
                        operands.Push(token);
                    }

                    break;

                case TokenClass.ArgumentSeparator:
                    while (operands.HasItems()
                           && operands.Peek().Class != TokenClass.OpenBracket)
                    {
                        if (operands.Count < 1) throw new Exception("Argument separator outside of argument list");
                        postfix.PushNotEmpty(operands.Pop());
                    }

                    break;

                case TokenClass.Equality:
                    // We tokenise all the bits of equality expressions separately,
                    // now we might need to join them back together.
                    
                    // compare precedence
                    while (operands.HasItems() && token.ShouldDisplace(operands.Peek()))
                    {
                        postfix.PushNotEmpty(operands.Pop());
                    }
                    
                    var prev = operands.HasItems() ? operands.Peek() : null;
                    if (prev?.Class == TokenClass.Equality) // fuse this equality into the previous one
                    {
                        prev.Value += token.Value;
                    }
                    else
                    {
                        operands.PushNotEmpty(token);
                    }

                    break;
                
                case TokenClass.BinaryOperator:
                    // compare precedence
                    while (operands.HasItems() && token.ShouldDisplace(operands.Peek()))
                    {
                        postfix.PushNotEmpty(operands.Pop());
                    }

                    operands.PushNotEmpty(token);
                    break;

                case TokenClass.CloseBracket:
                    while (operands.HasItems()
                           && operands.Peek().Class != TokenClass.OpenBracket)
                    {
                        // add inner bracket contents
                        postfix.PushNotEmpty(operands.Pop());
                    }

                    #region Check for previously caught uniary minus bracket "-(...)"

                    if (operands.HasItems())
                    {
                        var ob = operands.Pop(); // check open bracket
                        if (ob.Value == ParenNeg)
                        {
                            // invert value
                            postfix.Push(Token.ForFunction("/neg",1));
                        }
                    }

                    #endregion

                    if (operands.HasItems()
                        && operands.Peek().Class == TokenClass.Function)
                    {
                        // this is actually a function
                        postfix.PushNotEmpty(operands.Pop());
                    }

                    break;

                case TokenClass.Nothing:
                    break;
                
                default:
                    throw new Exception("Unexpected token: " + token.Value);
            } // end of switch
        } // end of token stream

        // deal with any remaining operators
        while (operands.HasItems())
        {
            // compare precedence
            postfix.PushNotEmpty(operands.Pop());
        }

        var output = new Stack<Token>(postfix.Count);
        foreach (var t in postfix) output.PushNotEmpty(t);
        return output;
    }

    /// <summary>
    ///  deal with the case of `...( - func(...) )...`
    /// </summary>
    private static List<Token> FixupNegatedFunctions(List<Token> tokens)
    {
        var result = new List<Token>(tokens.Count);

        for (var index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];

            if (index < tokens.Count - 2
                && token.Value == "(" && tokens[index + 1].Value == "-" && tokens[index + 2].Value.StartsWith('/'))
            {
                result.Add(token); // keep paren
                var func = tokens[index + 2];
                func.Negated = true;
                result.Add(func); // keep function
                index += 2; // skip the negative symbol
            }
            else
            {
                result.Add(token);
            }
        }


        return result;
    }

    private const string ParenNeg = "NEG(";

    /// <summary>
    /// Compile a set of postfix tokens into Tich interpreter commands
    /// </summary>
    public static IEnumerable<Cell> CompilePostfix(Stack<Token> postfix, bool canElideP = true)
    {
        // very simple for the moment, doesn't handle variables or functions

        var program = new List<Cell>();
        
        if (postfix.Count < 1) return Array.Empty<Cell>();

        foreach (var token in postfix)
        {
            if (string.IsNullOrEmpty(token.Value)) continue;

            token.Value = token.Value.ToLowerInvariant();
            
            switch (token.Value)
            {
                // Normal binary operators, these work only on the stack.
                case "+":
                    program.Cmd(Command.Add);
                    break;
                case "-":
                    program.Cmd(Command.Sub);
                    break;
                case "*":
                    program.Cmd(Command.Mul);
                    break;
                case "/":
                    // TODO: peephole reciprocal optimisation
                    if (CanBeReciprocal(program))
                    {
                        program.RemoveAt(program.Count - 2); // remove the 1 value
                        program.Cmd(Command.Reciprocal);
                    }
                    else
                    {
                        program.Cmd(Command.Div);
                    }
                    break;
                case "^":
                    program.Cmd(Command.Pow);
                    break;
                case "%":
                    program.Cmd(Command.Mod);
                    break;
                case "|":
                    program.Cmd(Command.Or);
                    break;
                case "&":
                    program.Cmd(Command.And);
                    break;
                
                // binary equality
                case "<":
                    program.Cmd(Command.Less);
                    break;
                case "<=":
                    program.Cmd(Command.LessEq);
                    break;
                case ">":
                    program.Cmd(Command.More);
                    break;
                case ">=":
                    program.Cmd(Command.MoreEq);
                    break;
                case "=":
                    program.Cmd(Command.Equal);
                    break;
                case "!=":
                    program.Cmd(Command.NotEq);
                    break;

                // not used in postfix notation:
                case ",":
                case "(":
                case ")":
                    throw new Exception("Unexpected token in Postfix");

                default: // anything else is probably a number, a function, a known constant, etc
                    if (token.Class == TokenClass.Operand)
                    {
                        HandleOperand(program, token);
                    }
                    else if (token.Value.StartsWith("/"))
                    {
                        HandleFunctionLikeToken(token, program);
                        if (token.Negated) program.Cmd(Command.Neg);
                    }
                    else if (token.Value.StartsWith(".")) HandleSwizzle(token, program);
                    else
                    {
                        HandleConstantLikeToken(token, program);
                    }

                    break;
            }
        } // end foreach
        
        if (canElideP && program.Count > 0 && program[0].Cmd == Command.P)
        {
            program.RemoveAt(0); // we get a 'free' P at the start of the program
        }

        return program;
    }

    /// <summary>
    /// Checks to see if the program has a OneS and a Variant at the end.
    /// In which case, we can remove the OneS operand and user Reciprocal instead of divide.
    /// </summary>
    private static bool CanBeReciprocal(List<Cell> program)
    {
        if (program.Count < 2) return false;
        var end = program.Count - 1;
        if (program[end-1].Cmd != Command.OneS) return false;
        
        return program[end].Cmd is Command.Scalar or Command.Vec2 or Command.Vec3 or Command.Vec4;
    }

    /// <summary>
    /// Scan through tokens, finding functions and counting the number of parameters
    /// </summary>
    private static bool DefineFunctionArity(List<Token> tokens)
    {
        // TODO: this could be made much more efficient, by tracking all depths and their start points, scanning through only once.
        for (var index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];
            if (token.Class != TokenClass.Function) continue;
            if (index == tokens.Count - 1) return false; // can't be matching
            
            // next token should be a parenthesis.
            // we can then scan for separators until we find a matching parenthesis and set the function's arity.
            // if we find other open parens, we need to ignore until they are all closed
            // if the open paren is followed by a close, we have empty params.
            var sepCount = 0; // number of argument separators we've seen at depth == 1
            var symbolCount = 0; // number of non-paren symbols we've seen at all
            var parenDepth = 0; // current depth of parenthesis during the scan

            for (int i = index+1; i <= tokens.Count; i++) // deliberately run off the end to catch unmatched parens.
            {
                if (i >= tokens.Count) return false; // failed to find match
                
                var scanToken = tokens[i];
                if (scanToken.Class == TokenClass.OpenBracket) parenDepth++;
                else if (scanToken.Class == TokenClass.CloseBracket) parenDepth--;
                else if (scanToken.Class == TokenClass.ArgumentSeparator) sepCount += parenDepth == 1 ? 1:0;
                else symbolCount++;
                
                if (parenDepth==0) break; // last symbol closed the function call
            }
            
            if (symbolCount > 0) token.ParameterCount = sepCount + 1;
        }

        return true;
    }

    /// <summary>
    /// Turn a numeric token into a number valued cell.
    /// This does a few early phase optimisations for common constants
    /// </summary>
    private static void HandleOperand(List<Cell> program, Token token)
    {
        if (token.Number == 0.0)                      program.Add(new Cell { Cmd = Command.ZeroS });
        else if (Math.Abs(token.Number - 1.0) < 1E-6) program.Add(new Cell { Cmd = Command.OneS });
        else                                          program.Add(new Cell { Cmd = Command.Scalar, NumberValue = token.Number });
    }

    private static void HandleFunctionLikeToken(Token token, List<Cell> program)
    {
        switch (token)
        {
            case "/abs":
                AssertArgumentCount(token, 1);
                program.Cmd(Command.Abs);
                break;
            case "/acos":
                AssertArgumentCount(token, 1);
                program.Cmd(Command.Acos);
                break;
            case "/atan":
                AssertArgumentCount(token, 2);
                program.Cmd(Command.Atan);
                break;
            case "/all":
                AssertArgumentCount(token, 1);
                program.Cmd(Command.All);
                break;
            case "/and": // move to ops?
                AssertArgumentCount(token, 2);
                program.Cmd(Command.And);
                break;
            case "/angle":
                AssertArgumentCount(token, 1);
                program.Cmd(Command.Angle);
                break;
            case "/cos":
                AssertArgumentCount(token, 1);
                program.Cmd(Command.Cos);
                break;
            
            
            case "/max":
                AssertArgumentCount(token, 2);
                program.Cmd(Command.Max);
                break;
            
            case "/len":
            case "/length":
                AssertArgumentCount(token, 1);
                program.Cmd(Command.Length);
                break;

            case "/vec2":
            {
                AssertArgumentCount(token, 2);
                if (AllZeros(program, 2)) { program.Drop(2); program.Cmd(Command.ZeroV2); }
                else if (AllOnes(program, 2)) { program.Drop(2); program.Cmd(Command.OneV2); }
                else program.Cmd(Command.Vec2);
                break;
            }

            case "/vec3":
            {
                AssertArgumentCount(token, 3);
                if (AllZeros(program, 3)) { program.Drop(3); program.Cmd(Command.ZeroV3); }
                else if (AllOnes(program, 3)) { program.Drop(3); program.Cmd(Command.OneV3); }
                else program.Cmd(Command.Vec3);
                break;
            }

            case "/vec4":
            {
                AssertArgumentCount(token, 4);
                if (AllZeros(program, 4)) { program.Drop(4); program.Cmd(Command.ZeroV4); }
                else if (AllOnes(program, 4)) { program.Drop(4); program.Cmd(Command.OneV4); }
                else program.Cmd(Command.Vec4);
                break;
            }

            case "/clamp":
                AssertArgumentCount(token, 3); // 1 stays on the 'stack', 2 moved to args
                program.Cmd(Command.Clamp);
                break;
            
            case "/cross":
                AssertArgumentCount(token, 2);
                program.Cmd(Command.Cross);
                break;
            
            case "/dot":
                AssertArgumentCount(token,2);
                program.Cmd(Command.Dot);
                break;
            
            case "/eq":
                AssertArgumentCount(token, 2);
                program.Cmd(Command.Equal);
                break;
            
            case "/high":
                AssertArgumentCount(token,2);
                program.Cmd(Command.Highest);
                break;
            
            case "/lerp":
                AssertArgumentCount(token,3);
                program.Cmd(Command.Lerp);
                break;
            
            case "/low":
                AssertArgumentCount(token, 2);
                program.Cmd(Command.Lowest);
                break;
            
            // ReSharper disable once StringLiteralTypo
            case "/maxc":
                AssertArgumentCount(token, 1);
                program.Cmd(Command.MaxComponent);
                break;
            
            case "/mid":
                AssertArgumentCount(token, 2);
                program.Cmd(Command.Midpoint);
                break;
            
            case "/min":
                AssertArgumentCount(token, 2);
                program.Cmd(Command.Min);
                break;
            
            case "/mul":
                AssertArgumentCount(token, 5);
                program.Cmd(Command.MatrixMul);
                break;
            
            case "/neg":
                AssertArgumentCount(token, 1);
                program.Cmd(Command.Neg);
                break;
            
            case "/none":
                AssertArgumentCount(token, 1);
                program.Cmd(Command.None);
                break;
            
            case "/norm":
                AssertArgumentCount(token,1);
                program.Cmd(Command.Normalise);
                break;
            
            case "/not":
                AssertArgumentCount(token,1);
                program.Cmd(Command.Not);
                break;
            
            case "/pow":
                AssertArgumentCount(token,2);
                program.Cmd(Command.Pow);
                break;
            
            case "/rec":
                AssertArgumentCount(token,1);
                program.Cmd(Command.Reciprocal);
                break;
            
            case "/rect":
                AssertArgumentCount(token,2);
                program.Cmd(Command.Rect);
                break;
            
            case "/mix":
                AssertArgumentCount(token,3);
                program.Cmd(Command.SmoothStep);
                break;
            
            case "/sign":
                AssertArgumentCount(token,1);
                program.Cmd(Command.Sign);
                break;
            
            case "/sin":
                AssertArgumentCount(token,1);
                program.Cmd(Command.Sin);
                break;
            
            case "/sqrt":
                AssertArgumentCount(token,1);
                program.Cmd(Command.Sqrt);
                break;
            
            
            default: throw new Exception($"Unknown function-like token: '{token}'");
        }
    }

    private static bool AllZeros(List<Cell> program, int count)
    {
        var upper = program.Count - 1;
        var lower = upper - count + 1;
        for (int i = lower; i <= upper; i++)
        {
            if (program[i].Cmd != Command.ZeroS) return false;
        }
        return true;
    }

    private static bool AllOnes(List<Cell> program, int count)
    {
        var upper = program.Count - 1;
        var lower = upper - count + 1;
        for (int i = lower; i <= upper; i++)
        {
            if (program[i].Cmd != Command.OneS) return false;
        }
        return true;
    }

    private static void AssertArgumentCount(Token token, int expected)
    {
        if (token.ParameterCount != expected) throw new Exception($"Function {token.Value} should have {expected} parameters, but was given {token.ParameterCount}");
    }

    /// <summary>
    /// Convert a name token into a program cell
    /// </summary>
    private static void HandleConstantLikeToken(string token, List<Cell> program)
    {
        switch (token)
        {
            // special register 'P'
            case "p":
                program.Cmd(Command.P);
                break;
            
            // constants
            case "pi":
                program.Num(Math.PI);
                break;
            
            // registers
            case "a": program.Cmd(Command.GetA); break;
            case "b": program.Cmd(Command.GetB); break;
            case "c": program.Cmd(Command.GetC); break;
            case "d": program.Cmd(Command.GetD); break;
            case "e": program.Cmd(Command.GetE); break;
            case "f": program.Cmd(Command.GetF); break;
            case "g": program.Cmd(Command.GetG); break;
            case "h": program.Cmd(Command.GetH); break;
            case "i": program.Cmd(Command.GetI); break;
            case "j": program.Cmd(Command.GetJ); break;
            case "k": program.Cmd(Command.GetK); break;
            case "l": program.Cmd(Command.GetL); break;
            case "m": program.Cmd(Command.GetM); break;
            case "n": program.Cmd(Command.GetN); break;
            
            default: throw new Exception($"Unknown constant or variable: '{token}'");
        }
    }

    private static void HandleSwizzle(string token, List<Cell> program)
    {
        var indexes = token.ToCharArray();
        if (indexes.Length < 2) throw new Exception($"unexpected short swizzle: '{token}'");
        if (indexes.Length > 5) throw new Exception($"unexpected long swizzle: '{token}'");

        if (indexes.Length == 2) program.Cmd(Command.Swz1, SwizIdx(indexes[1]));
        if (indexes.Length == 3) program.Cmd(Command.Swz2, SwizIdx(indexes[1]), SwizIdx(indexes[2]));
        if (indexes.Length == 4) program.Cmd(Command.Swz3, SwizIdx(indexes[1]), SwizIdx(indexes[2]), SwizIdx(indexes[3]));
        if (indexes.Length == 5) program.Cmd(Command.Swz4, SwizIdx(indexes[1]), SwizIdx(indexes[2]), SwizIdx(indexes[3]), SwizIdx(indexes[4]));
        
    }

    private static double SwizIdx(char c)
    {
        switch (c)
        {
            case 'x' : return 0;
            case 'y' : return 1;
            case 'z' : return 2;
            case 'w' : return 3;
            default: throw new Exception($"Invalid swizzle index: '{c}'");
        }
    }
}

/// <summary>
/// Helper methods for compiler
/// </summary>
public static class CompilerExtensions
{
    /// <summary>
    /// Push a value if it is not null or empty
    /// </summary>
    public static void PushNotEmpty(this Stack<Token> stack, Token? value)
    {
        if (value is null) return;
        if (value.IsEmpty) return;
        if (string.IsNullOrWhiteSpace(value.Value)) return;
        stack.Push(value);
    }
    
    /// <summary>
    /// Remove the last `count` program cells
    /// </summary>
    public static void Drop(this List<Cell> program, double count)
    {
        for (int i = 0; i < count; i++)
        {
            program.RemoveAt(program.Count - 1);
        }
    }

    /// <summary>
    /// Add a command to the program
    /// </summary>
    public static void Cmd(this List<Cell> program, Command cmd)
    {
        program.Add(new Cell { Cmd = cmd });
    }
    
    /// <summary>
    /// Add a command to the program, pushing values onto the stack
    /// </summary>
    public static void Cmd(this List<Cell> program, Command cmd, params double[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            program.Add(new Cell { Cmd = Command.Scalar, NumberValue = values[i] });
        }

        program.Add(new Cell { Cmd = cmd });
    }
    
    /// <summary>
    /// Add a command to the program
    /// </summary>
    public static void Num(this List<Cell> program, double num)
    {
        program.Add(new Cell { Cmd = Command.Scalar, NumberValue = num});
    }
}