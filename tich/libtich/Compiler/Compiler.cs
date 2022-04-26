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
    public static Stack<Token> InfixToPostfix(string expression)
    {
        var operands = new Stack<Token>();
        var postfix = new Stack<Token>();

        var tokens = expression.Tokens();
        var ok = DefineFunctionArity(tokens);
        if (!ok) throw new Exception("Unmatched braces in expression"); // TODO: better message
        
        foreach (var token in tokens)
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

        var output = new Stack<Token>(postfix.Count);
        foreach (var t in postfix) output.PushNotEmpty(t);
        return output;
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
    /// Compile a set of postfix tokens into Tich interpreter commands
    /// </summary>
    public static IEnumerable<Cell> CompilePostfix(Stack<Token> postfix)
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
                // These binary operators need to pull in literals that might be combined. 
                case "+":
                    program.Add(new Cell { Cmd = Command.Add});
                    break;
                case "-":
                    program.Add(new Cell { Cmd = Command.Sub});
                    break;
                case "*":
                    program.Add(new Cell { Cmd = Command.Mul});
                    break;
                case "/":
                    program.Add(new Cell { Cmd = Command.Div});
                    break;
                case "^":
                    program.Add(new Cell { Cmd = Command.Pow});
                    break;
                case "%":
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
                    if (token.Class == TokenClass.Operand)
                    {
                        program.Add(new Cell { Cmd = Command.Scalar, Params = new[] { token.Number } });
                    }
                    else if (token.Value.StartsWith("/")) HandleFunctionLikeToken(token, program);
                    else if (token.Value.StartsWith(".")) HandleSwizzle(token, program);
                    else
                    {
                        HandleConstantLikeToken(token, program);
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

    private static void HandleFunctionLikeToken(Token token, List<Cell> program)
    {
        switch (token)
        {
            case "/abs":
                AssertArgumentCount(token, 1);
                program.Add(new Cell{Cmd=Command.Abs});
                break;
            case "/acos":
                AssertArgumentCount(token, 1);
                program.Add(new Cell{Cmd=Command.Acos});
                break;
            case "/all":
                AssertArgumentCount(token, 1);
                program.Add(new Cell{Cmd=Command.All});
                break;
            case "/and": // move to ops?
                AssertArgumentCount(token, 2);
                program.Add(new Cell{Cmd=Command.And});
                break;
            case "/angle":
                AssertArgumentCount(token, 1);
                program.Add(new Cell{Cmd=Command.Angle});
                break;
            case "/cos":
                AssertArgumentCount(token, 1);
                program.Add(new Cell{Cmd=Command.Cos});
                break;
            
            
            case "/max":
                AssertArgumentCount(token, 2);
                program.Add(new Cell{Cmd = Command.Max});
                break;
            
            case "/length":
                AssertArgumentCount(token, 1);
                program.Add(new Cell{Cmd=Command.Length});
                break;
            
            case "/vec2":
                AssertArgumentCount(token, 2);
                program.Add(new Cell{Cmd=Command.Vec2, Params = PullOrError(2,program)});
                break;
            
            case "/vec3":
                AssertArgumentCount(token, 3);
                program.Add(new Cell{Cmd=Command.Vec3, Params = PullOrError(3,program)});
                break;
            
            case "/clamp":
                AssertArgumentCount(token, 3); // 1 stays on the 'stack', 2 moved to args
                program.Add(new Cell{Cmd=Command.Clamp, Params = PullOrError(2,program)});
                break;
            
            default: throw new Exception($"Unknown function-like token: '{token}'");
        }
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

    /// <summary>
    /// Pick cell off the end of the program to use as command parameters.
    /// An exception is thrown if not enough cells are available, or if the cells
    /// do not contain scalar values
    /// </summary>
    private static double[] PullOrError(int count, List<Cell> program)
    {
        var output = new double[count];
        for (int i = count-1; i >= 0; i--) // order is important
        {
            if (program.Count < 1) throw new Exception("Not enough parameters");
            
            var candidate = program[^1];
            if (candidate.Cmd != Command.Scalar || candidate.Params.Length != 1) throw new Exception("Non-scalar value used for command parameter");
            
            output[i] = candidate.Params[0];
            program.RemoveAt(program.Count - 1);
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
    public static Stack<string> ToValueStack(this Stack<Token> stack)
    {
        return new Stack<string>(stack.Reverse().Select(t=>t.Value));
    }
    
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