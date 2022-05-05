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
            Console.WriteLine($" -> {string.Join(",",stack.Select(s=>s.ToString()))}");
            
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
        
        Console.WriteLine($"{step} <- {string.Join(",",stack.Select(s=>s.ToString()))}");
        
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
                CheckAvailable(stack, 3);
                var upper = Pop(stack).X;
                var lower = Pop(stack).X;
                if (lower > upper) { (lower,upper)=(upper,lower); }
                stack.Push(VMath.Clamp(Pop(stack), lower, upper));
                return next;
            }
            case Command.MatrixMul: // TODO: have a special kind that takes a vec4 instead of 4 scalars.
            {
                CheckAvailable(stack, 5);
                
                stack.Push(VMath.MatrixMul4(Pop(stack), PopArray(stack, 4)));
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
                stack.Push(Variant.Scalar(step.NumberValue));
                return next;
            }
            
            case Command.Vec2:
            {
                var y = Pop(stack);
                var x = Pop(stack);
                stack.Push(Variant.Vec2(x.Value, y.Value));
                return next;
            }
            
            case Command.Vec3:
            {
                var z = Pop(stack);
                var y = Pop(stack);
                var x = Pop(stack);
                stack.Push(Variant.Vec3(x.Value, y.Value, z.Value));
                return next;
            }

            case Command.Vec4:
            {
                var w = Pop(stack);
                var z = Pop(stack);
                var y = Pop(stack);
                var x = Pop(stack);
                stack.Push(Variant.Vec4(x.Value, y.Value, z.Value, w.Value));
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
                var indexes = PopArray(stack, 1);
                stack.Push(VMath.Swizzle(Pop(stack), indexes));
                return next;
            }
            
            case Command.Swz2:
            {
                var indexes = PopArray(stack, 2);
                stack.Push(VMath.Swizzle(Pop(stack), indexes));
                return next;
            }
            case Command.Swz3:
            {
                var indexes = PopArray(stack, 3);
                stack.Push(VMath.Swizzle(Pop(stack), indexes));
                return next;
            }
            case Command.Swz4:
            {
                var indexes = PopArray(stack, 4);
                stack.Push(VMath.Swizzle(Pop(stack), indexes));
                return next;
            }
            
            case Command.SwzSplit3:
            {
                var indexes = PopArray(stack, 3);
                stack.Push(VMath.SwizzleSplit(Pop(stack), indexes, out var split));
                stack.Push(split);
                return next;
            }
            case Command.Midpoint:
            {
                stack.Push(VMath.Lerp(Pop(stack), Pop(stack), 0.5));
                return next;
            }

            case Command.Lerp:
            {
                var indexes = PopArray(stack, 1);
                stack.Push(VMath.Lerp(Pop(stack), Pop(stack), indexes[0]));
                return next;
            }

            case Command.SmoothStep: // this is a lerp, but with a gain function applied to the proportion
            {
                var prop = VMath.Gain(PopArray(stack, 1)[0], 2);
                stack.Push(VMath.Lerp(Pop(stack), Pop(stack), prop));
                return next;
            }

            case Command.Rect:
            {
                stack.Push(VMath.RectangleVector(Pop(stack),Pop(stack)));
                return next;
            }
            
            case Command.Lowest:
            {
                stack.Push(VMath.PairwiseMin(Pop(stack), Pop(stack)));
                return next;
            }
            case Command.Highest:
            {
                stack.Push(VMath.PairwiseMax(Pop(stack), Pop(stack)));
                return next;
            }
            case Command.MaxComponent:
            {
                stack.Push(VMath.ComponentMax(Pop(stack)));
                return next;
            }
            case Command.Angle:
            {
                stack.Push(VMath.Angle(Pop(stack)));
                return next;
            }
            
            case Command.P:
                throw new InvalidOperationException("Switch was passed a `P` command, which should have been handled elsewhere.");
            default:
                throw new ArgumentOutOfRangeException($"Program has an unknown command: {step.Cmd}");
        }
    }

    private static double[] PopArray(Stack<Variant> stack, int len)
    {
        if (len < 1) return Array.Empty<double>();
        
        var result = new double[len];
        for (int i = len - 1; i >= 0; i--)
        {
            result[i] = stack.Pop().Value;
        }
        return result;
    }

    private static Variant Pop(Stack<Variant> stack)
    {
        return stack.Count < 1 ? new Variant() : stack.Pop();
    }

    private static void CheckAvailable(Stack<Variant> stack, int required)
    {
        if (stack.Count < required) throw new Exception("Stack underflow");
    }
}