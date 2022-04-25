namespace libtich;

/// <summary>
/// A single program line (for a single layer)
/// </summary>
public class TichProgram
{
    private readonly List<Cell> _program;

    /// <summary>
    /// Wrap a program in a runner
    /// </summary>
    public TichProgram(IEnumerable<Cell> program)
    {
        _program = program.ToList();
    }

    /// <summary>
    /// Evaluate the program against an initial stack, which will be mutated.
    /// This is mostly used for testing. Call `CalculateForPoint`
    /// </summary>
    public void Evaluate(Stack<Variant> stack, Variant parameter)
    {
        // Run the program.
        for (var i = 0; i < _program.Count; i++)
        {
            var step = _program[i];
            if (step.Cmd == Command.P)
            {
                stack.Push(parameter);
                continue;
            }

            var end = NextStep(step, stack);
            
            if (end)
            {
                Console.WriteLine($"Early stop at {i} ({step.Cmd})");
                break;
            }
        }
    }

    /// <summary>
    /// Run this program to get the signed distance for a single point in the render space.
    /// Calls evaluate and pulls end value
    /// </summary>
    public double CalculateForPoint(double x, double y)
    {
        var stack = new Stack<Variant>();
        
        stack.Push(Variant.Vec2(x,y));

        Evaluate(stack, Variant.Vec2(x, y));

        if (stack.Count < 1) return double.NaN; // callers must handle this case
        return stack.Pop().Values[0]; // X component or scalar of stack top
    }

    /// <summary>
    /// Giant switch that rules the program
    /// </summary>
    private static bool NextStep(Cell step, Stack<Variant> stack)
    {
        const bool next = false;
        const bool stop = true;
        
        switch (step.Cmd)
        {
            case Command.Invalid:
                return stop;
            
            case Command.Cos:
                stack.Push(VMath.Cos(Pop(stack)));
                return next;
            
            case Command.Acos:
                stack.Push(VMath.Acos(Pop(stack)));
                return next;
            
            case Command.Sin:
                stack.Push(VMath.Sin(Pop(stack)));
                return next;
            
            case Command.Abs:
                stack.Push(VMath.Abs(Pop(stack)));
                return next;
            
            case Command.Sqrt:
                stack.Push(VMath.Sqrt(Pop(stack)));
                return next;
            
            case Command.Neg:
                stack.Push(VMath.Neg(Pop(stack)));
                return next;
            
            case Command.Length:
                stack.Push(VMath.Length(Pop(stack)));
                return next;
            
            case Command.Sign:
                stack.Push(VMath.Sign(Pop(stack)));
                return next;
            
            case Command.Reciprocal:
                stack.Push(VMath.Reciprocal(Pop(stack)));
                return next;
            
            case Command.Normalise:
                stack.Push(VMath.VectorNormal(Pop(stack)));
                return next;
            
            case Command.Max:
                stack.Push(VMath.PairwiseMax(Pop(stack), Pop(stack)));
                return next;
            
            case Command.Min:
                stack.Push(VMath.PairwiseMin(Pop(stack), Pop(stack)));
                return next;
            
            case Command.Add:
                stack.Push(VMath.PairwiseAdd(Pop(stack), Pop(stack)));
                return next;
            
            case Command.Sub:
                stack.Push(VMath.PairwiseSubtract(Pop(stack), Pop(stack)));
                return next;
            
            case Command.Div:
                stack.Push(VMath.PairwiseDivideFloor(Pop(stack), Pop(stack)));
                return next;
            
            case Command.Mod:
                stack.Push(VMath.PairwiseModulo(Pop(stack), Pop(stack)));
                return next;
            
            case Command.Mul:
                stack.Push(VMath.PairwiseMultiply(Pop(stack), Pop(stack)));
                return next;
            
            case Command.Pow:
                stack.Push(VMath.ComponentwisePower(Pop(stack), Pop(stack)));
                return next;
            
            case Command.Dot:
                stack.Push(VMath.DotProduct(Pop(stack), Pop(stack)));
                return next;
            
            case Command.Cross:
                stack.Push(VMath.CrossProduct(Pop(stack), Pop(stack)));
                return next;

            case Command.Clamp:
            {
                if (step.Params.Length < 2) return stop;
                var lower = step.Params[0];
                var upper = step.Params[1];
                if (lower > upper) { (lower,upper)=(upper,lower); }
                stack.Push(VMath.Clamp(Pop(stack), lower, upper));
                return next;
            }
            case Command.MatrixMul:
            {
                if (step.Params.Length < 4) return stop;
                
                stack.Push(VMath.MatrixMul4(Pop(stack), step.Params));
                return next;
            }
            case Command.Less:
                stack.Push(VMath.CompareLess(Pop(stack), Pop(stack)));
                return next;
            
            case Command.More:
                stack.Push(VMath.CompareGreater(Pop(stack), Pop(stack)));
                return next;
            
            case Command.LessEq:
                stack.Push(VMath.CompareLessEqual(Pop(stack), Pop(stack)));
                return next;
            case Command.MoreEq:
                stack.Push(VMath.CompareGreaterEqual(Pop(stack), Pop(stack)));
                return next;
            case Command.Equal:
                stack.Push(VMath.CompareEqual(Pop(stack), Pop(stack)));
                return next;
            case Command.NotEq:
                stack.Push(VMath.CompareNotEqual(Pop(stack), Pop(stack)));
                return next;
            
            case Command.All:
                stack.Push(VMath.ComponentAllNonZero(Pop(stack)));
                return next;
            
            case Command.None:
                stack.Push(VMath.ComponentAllZero(Pop(stack)));
                return next;
            
            case Command.And:
                stack.Push(VMath.ComponentwiseLogicalAnd(Pop(stack), Pop(stack)));
                return next;
            
            case Command.Or:
                stack.Push(VMath.ComponentwiseLogicalOr(Pop(stack), Pop(stack)));
                return next;
            
            case Command.Not:
                stack.Push(VMath.ComponentwiseLogicalNot(Pop(stack)));
                return next;
            
            case Command.Scalar:
            {
                if (step.Params.Length < 1) return stop;
                stack.Push(Variant.Scalar(step.Params[0]));
                return next;
            }
            
            case Command.Vec2:
            {
                if (step.Params.Length < 2) return stop;
                stack.Push(Variant.Vec2(step.Params[0], step.Params[1]));
                return next;
            }
            
            case Command.Vec3:
            {
                if (step.Params.Length < 3) return stop;
                stack.Push(Variant.Vec3(step.Params[0], step.Params[1], step.Params[2]));
                return next;
            }

            case Command.Vec4:
            {
                if (step.Params.Length < 4) return stop;
                stack.Push(Variant.Vec4(step.Params[0], step.Params[1], step.Params[2], step.Params[3]));
                return next;
            }

            case Command.ZeroS:
                stack.Push(Variant.Scalar(0));
                return next;
            
            case Command.ZeroV2:
                stack.Push(Variant.Vec2(0,0));
                return next;

            case Command.ZeroV3:
                stack.Push(Variant.Vec3(0,0,0));
                return next;

            case Command.ZeroV4:
                stack.Push(Variant.Vec4(0,0,0,0));
                return next;

            case Command.OneS:
                stack.Push(Variant.Scalar(1));
                return next;
            
            case Command.OneV2:
                stack.Push(Variant.Vec2(1,1));
                return next;

            case Command.OneV3:
                stack.Push(Variant.Vec3(1,1,1));
                return next;

            case Command.OneV4:
                stack.Push(Variant.Vec4(1,1,1,1));
                return next;
            
            case Command.Swz1:
            {
                if (step.Params.Length != 1) return stop;
                stack.Push(VMath.Swizzle(Pop(stack), step.Params));
                return next;
            }
            
            case Command.Swz2:
            {
                if (step.Params.Length != 2) return stop;
                stack.Push(VMath.Swizzle(Pop(stack), step.Params));
                return next;
            }
            case Command.Swz3:
            {
                if (step.Params.Length != 3) return stop;
                stack.Push(VMath.Swizzle(Pop(stack), step.Params));
                return next;
            }
            case Command.Swz4:
            {
                if (step.Params.Length != 4) return stop;
                stack.Push(VMath.Swizzle(Pop(stack), step.Params));
                return next;
            }
            
            case Command.SwzSplit3:
            {
                if (step.Params.Length != 3) return stop;
                stack.Push(VMath.SwizzleSplit(Pop(stack), step.Params, out var split));
                stack.Push(split);
                return next;
            }
            case Command.Midpoint:
                break;
            case Command.SmoothStep:
                break;
            case Command.Lerp:
                break;
            case Command.Rect:
                break;
            case Command.Lowest:
                break;
            case Command.Highest:
                break;
            case Command.MaxComponent:
                break;
            case Command.Angle:
                break;
            
            case Command.P:
                throw new InvalidOperationException("Switch was passed a `P` command, which should have been handled elsewhere.");
            default:
                throw new ArgumentOutOfRangeException($"Program has an unknown command: {step.Cmd}");
        }
        return stop;
    }

    private static Variant Pop(Stack<Variant> stack)
    {
        return stack.Count < 1 ? new Variant() : stack.Pop();
    }
}