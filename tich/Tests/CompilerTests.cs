using libtich;
using NUnit.Framework;
// ReSharper disable AssignNullToNotNullAttribute

namespace Tests;

[TestFixture]
public class CompilerTests
{
    [Test]
    // ReSharper disable StringLiteralTypo
    [TestCase("5.0 - 3.0", 2.0)] // scalars either side of an operator
    [TestCase("length(p) - 3.0", 5.602325)] // scalar to right of operator
    [TestCase("8.0 - length(p)", -0.60232)] // scalar to left of operator
    [TestCase("length(vec2(5, 8)) - length(p)", 0.8316)] // functions on either side of operator
    [TestCase("pi - length(p)", -5.460)] // using constant names
    [TestCase("length(p - 1)", 7.211)] // function at top level
    [TestCase("2 * 3 - 4 * 5", -14)] // order of precedence
    [TestCase("vec3(2,3,4).y", 3)] // swizzle 1
    [TestCase("vec3(2,3,4).zx", 4)] // swizzle 2
    [TestCase("length(vec3(2,3,4).zx)", 4.472)] // swizzle 2. Length (4,2)
    [TestCase("length(vec3(2,3,4).yyy)", 5.196)] // swizzle 3. Length (3,3,3)
    [TestCase("length(vec3(2,3,4).zzxx)", 6.3245)] // swizzle 4. Length (4,4,2,2)
    [TestCase("all(vec4(len(p - vec2(2,2)), p.x < 4, p.y < 4, p.y - p.x < 4))", 0.0)] // more complex statement
    
    // TODO: assignment & multiple expressions
    // ReSharper restore StringLiteralTypo
    public void expression_tests(string expr, double expected)
    {
        Console.WriteLine($"Interpreting ({expr})");
        var postfix = Compiler.InfixToPostfix(expr);
        Assert.That(postfix, Is.Not.Null);
        Console.WriteLine(postfix.PrettyPrint());
        
        var code = Compiler.CompilePostfix(postfix).ToList();
        Assert.That(code, Is.Not.Null);
        Console.WriteLine(code.PrettyPrint());
        
        var program = new TichProgram(code);
        var result = program.CalculateForPoint(5,7); // length is ~= 8.60232
        Assert.That(result, Is.EqualTo(expected).Within(0.001));
    }

    [Test]
    public void expression_with_stack_and_param_use()
    {
        // named value
        var expr = "clamp(pi, 0, 100)"; // stack variables always go first, then parameters
        Console.WriteLine($"=============[ {expr} ]=====================");
        
        var postfix = Compiler.InfixToPostfix(expr);
        Console.WriteLine(postfix.PrettyPrint());
        var code = Compiler.CompilePostfix(postfix).ToList();
        Console.WriteLine(code.PrettyPrint());
        
        Assert.That(code[0].Cmd, Is.EqualTo(Command.Scalar), "value cmd");
        Assert.That(code[0].NumberValue, Is.EqualTo(Math.PI).Within(0.001), "scalar value");
        Assert.That(code[1].AsNumber(), Is.EqualTo(0).Within(0.001), "clamp lower");
        Assert.That(code[2].AsNumber(), Is.EqualTo(100).Within(0.001), "clamp upper");
        Assert.That(code[3].Cmd, Is.EqualTo(Command.Clamp), "clamp cmd");
        
        // simple value
        expr = "clamp(0.3, 0, 1)";
        Console.WriteLine($"=============[ {expr} ]=====================");
        
        postfix = Compiler.InfixToPostfix(expr);
        Console.WriteLine(postfix.PrettyPrint());
        code = Compiler.CompilePostfix(postfix).ToList();
        Console.WriteLine(code.PrettyPrint());
        
        Assert.That(code[0].Cmd, Is.EqualTo(Command.Scalar), "value cmd");
        Assert.That(code[0].NumberValue, Is.EqualTo(0.3).Within(0.001), "scalar value");
        Assert.That(code[1].AsNumber(), Is.EqualTo(0).Within(0.001), "clamp lower");
        Assert.That(code[2].AsNumber(), Is.EqualTo(1).Within(0.001), "clamp upper");
        Assert.That(code[3].Cmd, Is.EqualTo(Command.Clamp), "clamp cmd");
        
        // compound value
        expr = "clamp(length(p), 0, 1)";
        Console.WriteLine($"=============[ {expr} ]=====================");
        
        postfix = Compiler.InfixToPostfix(expr);
        Console.WriteLine(postfix.PrettyPrint());
        code = Compiler.CompilePostfix(postfix).ToList();
        Console.WriteLine(code.PrettyPrint());
        
        // `p` should be elided
        Assert.That(code[0].Cmd, Is.EqualTo(Command.Length), "value cmd");
        Assert.That(code[1].AsNumber(), Is.EqualTo(0).Within(0.001), "clamp lower");
        Assert.That(code[2].AsNumber(), Is.EqualTo(1).Within(0.001), "clamp upper");
        Assert.That(code[3].Cmd, Is.EqualTo(Command.Clamp), "clamp cmd");
    }

    [Test] // these pass P where appropriate to check the initial-P elision. 
    [TestCase("abs(p)", Command.Abs)]
    [TestCase("acos(p)", Command.Acos)]
    [TestCase("all(p)", Command.All)]
    [TestCase("and(p,p)", Command.And)]
    [TestCase("angle(p)", Command.Angle)]
    [TestCase("clamp(p, 0, 1)", Command.Clamp)]
    [TestCase("cos(p)", Command.Cos)]
    [TestCase("cross(p,p)", Command.Cross)]
    [TestCase("dot(p,p)", Command.Dot)]
    [TestCase("eq(p,p)", Command.Equal)]
    [TestCase("high(p,p)", Command.Highest)]
    [TestCase("length(p)", Command.Length)]
    [TestCase("len(p)", Command.Length)]
    [TestCase("lerp(p,p, 0.5)", Command.Lerp)]
    [TestCase("low(p,p)", Command.Lowest)]
    [TestCase("max(p,p)", Command.Max)]
    [TestCase("mul(p,1,2,3,4)", Command.MatrixMul)]
    [TestCase("mid(p,p)", Command.Midpoint)]
    [TestCase("min(p,p)", Command.Min)]
    [TestCase("neg(p)", Command.Neg)] // todo: peephole opt for expression that means this
    [TestCase("none(p)", Command.None)]
    [TestCase("norm(p)", Command.Normalise)]
    [TestCase("not(p)", Command.Not)]
    [TestCase("pow(p,p)", Command.Pow)]
    [TestCase("rec(p)", Command.Reciprocal)] // todo: peephole opt for expression that means this
    [TestCase("rect(p)", Command.Rect)]
    [TestCase("sign(p)", Command.Sign)]
    [TestCase("sin(p)", Command.Sin)]
    [TestCase("sqrt(p)", Command.Sqrt)]
    [TestCase("vec2(0,1)", Command.Vec2)]
    [TestCase("vec3(0,1,2)", Command.Vec3)]
    [TestCase("vec4(0,1,2,3)", Command.Vec4)]
    [TestCase("maxC(p)", Command.MaxComponent)]
    [TestCase("1", Command.OneS)] // optimisation
    [TestCase("vec2(1,1)", Command.OneV2)] // optimisation
    [TestCase("vec3(1,1,1)", Command.OneV3)] // optimisation
    [TestCase("vec4(1,1,1,1)", Command.OneV4)] // optimisation
    [TestCase("mix(p,p,0.1)", Command.SmoothStep)]
    //[TestCase("p.xy, p.z", Command.SwzSplit3)] // TODO: maybe pick some kind of syntax for this... maybe generalise?
    [TestCase("0.0", Command.ZeroS)] // optimisation
    [TestCase("vec2(0,0)", Command.ZeroV2)] // optimisation
    [TestCase("vec3(0,0,0)", Command.ZeroV3)] // optimisation
    [TestCase("vec4(0,0,0,0)", Command.ZeroV4)] // optimisation
    public void function_cases(string expr, Command expected)
    {
        var postfix = Compiler.InfixToPostfix(expr);
        var code = Compiler.CompilePostfix(postfix).ToList();
        Console.WriteLine(code.PrettyPrint());
        
        Assert.That(code.Last().Cmd, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("1.2 + 3.4", Command.Add, 4.6)]
    [TestCase("1.2 - 3.4", Command.Sub, -2.2)]
    [TestCase("1.2 & 3.4", Command.And, 1)]
    [TestCase("1.2 / 3.4", Command.Div, 0.35294)]
    [TestCase("1.2 = 3.4", Command.Equal, 0)]
    [TestCase("1.2 < 3.4", Command.Less, 1)]
    [TestCase("1.2 % 3.4", Command.Mod, 1.2)]
    [TestCase("1.2 > 3.4", Command.More, 0)]
    [TestCase("1.2 * 3.4", Command.Mul, 4.08)]
    [TestCase("1.2 | 3.4", Command.Or, 1)]
    [TestCase("1.2 ^ 3.4", Command.Pow, 1.8587)]
    [TestCase("1.0 / 3.4", Command.Reciprocal, 0.29411)]
    [TestCase("1.2 <= 3.4", Command.LessEq, 1)]
    [TestCase("1.2 >= 3.4", Command.MoreEq, 0)]
    [TestCase("1.2 != 3.4", Command.NotEq, 1)]
    public void operation_cases(string expr, Command cmdExpected, double valueExpected)
    {
        var postfix = Compiler.InfixToPostfix(expr);
        var code = Compiler.CompilePostfix(postfix).ToList();
        Console.WriteLine(code.PrettyPrint());
        
        Assert.That(code.Last().Cmd, Is.EqualTo(cmdExpected));
        
        var program = new TichProgram(code);
        var result = program.CalculateForPoint(0,1);
        
        Assert.That(result, Is.EqualTo(valueExpected).Within(0.001));
    }

    [Test]
    [TestCase("notClosed(1,2,3")]
    [TestCase("abs(1,2,3)")] // too many arguments
    [TestCase("length()")] // too few arguments
    public void invalid_expressions(string expr)
    {
        try
        {
            var postfix = Compiler.InfixToPostfix(expr);
            var code = Compiler.CompilePostfix(postfix).ToList();
            Console.WriteLine(code.PrettyPrint());

            Assert.Fail("Did not catch the error");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            // pass
        }
    }
}