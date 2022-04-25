namespace libtich;

using System;
using System.Collections.Generic;

/// <summary>
/// Turns expression strings into Tich programs
/// </summary>
public class Compiler
{
    /// <summary>
    /// Transform an infix expression string into a set of postfix tokens.
    /// This handles order of association, parenthesis, etc.
    /// </summary>
    public static Stack<string> InfixToPostfix(string expression)
    {
        var operands = new Stack<Token>();
        var postfix = new Stack<Token>();

        foreach (var token in expression.Tokens())
        {
            if (token.IsEmpty) continue;

            switch (token.Class)
            {
                case TokenClass.Name:
                case TokenClass.Operand:

                    #region Test for uniary operator

                    if (operands.HasItems()
                        && operands.Peek().Class == TokenClass.UniaryPrefix)
                    {
                        switch (operands.Peek().Value)
                        {
                            case "-":
                                // turn a uniary minus and operand into a negative operand
                                operands.Pop();
                                postfix.PushNotEmpty(token);
                                postfix.Push(new Token("-1"));
                                postfix.Push(new Token("*"));
                                break;
                            case "+":
                                // no change to operand, remove uniary
                                postfix.PushNotEmpty(token);
                                operands.Pop();
                                break;
                            default:
                                throw new Exception("Unexpected operator");
                        }

                        #endregion
                    }
                    else
                    {
                        postfix.Push(token);
                    }

                    break;
                case TokenClass.UniaryPostfix:
                    operands.Push(token);
                    break;
                case TokenClass.UniaryPrefix:
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
                                token.Value = "-";
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

                case TokenClass.BinaryOperator:
                    while (operands.HasItems()
                           && token.ShouldDisplace(operands.Peek()))
                    {
                        // compare precedence
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
                        Token ob = operands.Pop(); // check open bracket
                        if (ob.Value == "-")
                        {
                            // invert value
                            postfix.Push(new Token("-1"));
                            postfix.Push(new Token("*"));
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

        var output = new Stack<string>(postfix.Count);
        foreach (var t in postfix) output.PushNotEmpty(t.Value.Trim());
        return output;
    }

    /// <summary>
    /// Compile a set of postfix tokens into Tich interpreter commands
    /// </summary>
    public static IEnumerable<Cell> CompilePostfix(Stack<string> postfix)
    {
        // very simple for the moment, doesn't handle variables or functions

        var program = new List<Cell>();

        var values = new List<double>();
        //values.Push(0.0); // quick hack to deal with leading uniary operators

        foreach (var token in postfix)
        {
            if (string.IsNullOrEmpty(token)) continue;
            if (double.TryParse(token, out var value))
            {
                // If these *aren't* being pulled into function argument lists, they need to be wrapped as Scalars
                // If they are being pulled in, we'll unwrap them into arg lists
                values.Add(value);
                continue;
            }

            switch (token.ToLowerInvariant())
            {
                // These binary operators need to pull in literals that might be combined. 
                case "+":
                    PushRemaining(program, values); // if values are sitting on the stack, push them as scalars
                    program.Add(new Cell { Cmd = Command.Add});
                    break;
                case "-":
                    PushRemaining(program, values);
                    program.Add(new Cell { Cmd = Command.Sub});
                    break;
                case "*":
                    PushRemaining(program, values);
                    program.Add(new Cell { Cmd = Command.Mul});
                    break;
                case "/":
                    PushRemaining(program, values);
                    program.Add(new Cell { Cmd = Command.Div});
                    break;
                case "^":
                    PushRemaining(program, values);
                    program.Add(new Cell { Cmd = Command.Pow});
                    break;
                case "%":
                    PushRemaining(program, values);
                    program.Add(new Cell { Cmd = Command.Mod});
                    break;
                case ".": // dot notation for swizzling
                    program.Add(new Cell { Cmd = Command.Mod});
                    break;

                // not used in postfix notation:
                case ",":
                case "(":
                case ")":
                    throw new Exception("Unexpected token in Postfix");

                default: // anything else is probably a function, a known constant, etc
                    if (token.StartsWith("/")) HandleFunctionLikeToken(token.ToLowerInvariant(), values, program);
                    else if (token.StartsWith(".")) HandleSwizzle(token.ToLowerInvariant(), program);
                    else
                    {
                        PushRemaining(program, values);
                        HandleConstantLikeToken(token.ToLowerInvariant(), program);
                    }

                    break;
            }
        } // end foreach

        if (program.Count > 0 && program[0].Cmd == Command.P)
        {
            program.RemoveAt(0); // we get a 'free' P at the start of the program
        }

        return program;
    }

    private static void PushRemaining(List<Cell> program, List<double> values)
    {
        program.AddRange(
            values.Select(v=>new Cell{Cmd=Command.Scalar, Params=new[]{v}})
            );
        values.Clear();
    }

    private static void HandleFunctionLikeToken(string token, List<double> values, List<Cell> program)
    {
        switch (token)
        {
            case "/abs":
                program.Add(new Cell{Cmd=Command.Abs});
                break;
            case "/acos":
                program.Add(new Cell{Cmd=Command.Acos});
                break;
            case "/all":
                program.Add(new Cell{Cmd=Command.All});
                break;
            case "/and": // move to ops?
                program.Add(new Cell{Cmd=Command.And});
                break;
            
            case "/length":
                program.Add(new Cell{Cmd=Command.Length});
                break;
            
            case "/vec2":
                program.Add(new Cell{Cmd=Command.Vec2, Params = PullOrError(2,values)});
                break;
            
            case "/vec3":
                program.Add(new Cell{Cmd=Command.Vec3, Params = PullOrError(3,values)});
                break;
            
            case "/max":
                program.Add(new Cell{Cmd = Command.Max});
                break;
            
            default: throw new Exception($"Unknown function-like token: '{token}'");
        }
    }

    private static void HandleConstantLikeToken(string token, List<Cell> program)
    {
        switch (token)
        {
            case "p":
                program.Add(new Cell{Cmd=Command.P});
                break;
            
            case "pi":
                program.Add(new Cell { Cmd = Command.Scalar, Params = new[] { Math.PI } });
                break;
            
            default: throw new Exception($"Unknown constant-like token: '{token}'");
        }
    }

    private static void HandleSwizzle(string token, List<Cell> program)
    {
        var indexes = token.ToCharArray();
        if (indexes.Length < 2) throw new Exception($"unexpected short swizzle: '{token}'");
        if (indexes.Length > 5) throw new Exception($"unexpected long swizzle: '{token}'");

        if (indexes.Length == 2) program.Add(new Cell { Cmd = Command.Swz1, Params = new[] { SwizIdx(indexes[1]) } });
        if (indexes.Length == 3) program.Add(new Cell { Cmd = Command.Swz2, Params = new[] { SwizIdx(indexes[1]), SwizIdx(indexes[2]) } });
        if (indexes.Length == 4) program.Add(new Cell { Cmd = Command.Swz3, Params = new[] { SwizIdx(indexes[1]), SwizIdx(indexes[2]), SwizIdx(indexes[3]) } });
        if (indexes.Length == 5) program.Add(new Cell { Cmd = Command.Swz4, Params = new[] { SwizIdx(indexes[1]), SwizIdx(indexes[2]), SwizIdx(indexes[3]), SwizIdx(indexes[4]) } });
        
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

    private static double[] PullOrError(int count, List<double> values)
    {
        var output = new double[count];
        for (int i = 0; i < count; i++)
        {
            if (values.Count < 1) throw new Exception("Not enough parameters");
            
            output[i] = values[0];
            values.RemoveAt(0);
        }
        return output;
    }
}

/// <summary>
/// Helper methods for stack manipulation
/// </summary>
public static class CompilerExtensions{
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
    /// Push a value if it is not null or empty
    /// </summary>
    public static void PushNotEmpty(this Stack<string> stack, string? value)
    {
        if (value is null) return;
        if (string.IsNullOrWhiteSpace(value)) return;
        stack.Push(value);
    }
}