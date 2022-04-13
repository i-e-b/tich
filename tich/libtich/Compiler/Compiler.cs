namespace libtich;

using System;
using System.Collections.Generic;
using System.Collections;

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
                                postfix.Push(token);
                                postfix.Push(new Token("-1"));
                                postfix.Push(new Token("*"));
                                break;
                            case "+":
                                // no change to operand, remove uniary
                                postfix.Push(token);
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
                                operands.Push(token); // push bracket
                                break;
                            case "+":
                                operands.Pop(); // remove uniary
                                operands.Push(token); // push bracket
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
                        postfix.Push(operands.Pop());
                    }

                    break;

                case TokenClass.BinaryOperator:
                    while (operands.HasItems()
                           && token.ShouldDisplace(operands.Peek()))
                    {
                        // compare precedence
                        postfix.Push(operands.Pop());
                    }

                    operands.Push(token);
                    break;

                case TokenClass.CloseBracket:
                    while (operands.HasItems()
                           && operands.Peek().Class != TokenClass.OpenBracket)
                    {
                        // add inner bracket contents
                        postfix.Push(operands.Pop());
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
                        postfix.Push(operands.Pop());
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
            postfix.Push(operands.Pop());
        }

        var output = new Stack<string>(postfix.Count);
        foreach (Token t in postfix) output.Push(t.Value.Trim());
        return output;
    }

    /// <summary>
    /// Compile a set of postfix tokens into Tich interpreter commands
    /// </summary>
    public static IEnumerable<Cell> CompilePostfix(Stack<string> postfix)
    {
        // very simple for the moment, doesn't handle variables or functions

        var program = new List<Cell>();

        var values = new Stack<double>();
        //values.Push(0.0); // quick hack to deal with leading uniary operators

        foreach (var token in postfix)
        {
            if (string.IsNullOrEmpty(token)) continue;
            if (double.TryParse(token, out var value))
            {
                // If these *aren't* being pulled into function argument lists, they need to be wrapped as Scalars
                // If they are being pulled in, we'll unwrap them into arg lists
                values.Push(value);
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
                case ".": // dot notation for swizzling?
                    Console.WriteLine("need to implement swizzling?");
                    program.Add(new Cell { Cmd = Command.Mod});
                    break;

                // not used in postfix notation:
                case ",":
                case "(":
                case ")":
                    throw new Exception("Unexpected token in Postfix");

                default: // anything else is probably a function, a known constant, etc
                    HandleFunctionLikeToken(token.ToLowerInvariant(), values, program);
                    break;
            }
        } // end foreach

        return program;
    }

    private static void PushRemaining(List<Cell> program, Stack<double> values)
    {
        while (values.Count > 0)
        {
            program.Add(new Cell { Cmd = Command.Scalar, Params = new[] { values.Pop() } });
        }
    }

    private static void HandleFunctionLikeToken(string token, Stack<double> values, List<Cell> program)
    {
        switch (token)
        {
            case "/length":
                program.Add(new Cell{Cmd=Command.Length});
                break;
            
            case "/vec2":
                program.Add(new Cell{Cmd=Command.Vec2, Params = PullOrError(2,values)});
                break;
            
            case "p":
                program.Add(new Cell{Cmd=Command.P});
                break;
            
            case "pi":
                values.Push(Math.PI);
                break;
            
            case "/max":
                program.Add(new Cell{Cmd = Command.Max});
                break;
            
            default: throw new Exception($"Unknown function-like token: '{token}'");
        }
    }

    private static double[] PullOrError(int count, Stack<double> values)
    {
        var output = new double[count];
        for (int i = 0; i < count; i++)
        {
            if (values.Count < 1) throw new Exception("Not enough parameters");
            output[i] = values.Pop();
        }
        return output;
    }
}