using libtich;
using NUnit.Framework;
// ReSharper disable AssignNullToNotNullAttribute

namespace Tests;

[TestFixture]
public class BasicProgramTests
{
    
    [Test]
    public void empty_program_returns_input_x_coord()
    {
        var subject = new TichProgram(Array.Empty<Cell>());
        
        var result = subject.CalculateForPoint(10, 20);
        
        Assert.That(result, Is.EqualTo(10));
    }
    
    [Test]
    public void return_a_single_value()
    {
        var subject = new TichProgram(new[]{
            C(1.23)
        });
        
        var result = subject.CalculateForPoint(10, 10);
        
        Assert.That(result, Is.EqualTo(1.23));
    }
    
    [Test]
    public void can_push_target_point()
    {
        var subject = new TichProgram(new[]{
            C(Command.P),
            C(Command.Add)
        });
        
        var result = subject.CalculateForPoint(10, 1);
        
        Assert.That(result, Is.EqualTo(20));
    }
    
    [Test]
    public void calculate_distance()
    {
        var subject = new TichProgram(new[]{
            C(5),
            C(3),
            C(Command.Vec2),
            C(Command.Sub),
            C(Command.Length)
        });
        
        var result = subject.CalculateForPoint(10, 10);
        var expected = Math.Sqrt(5*5 + 7*7);
        Assert.That(result, Is.EqualTo(expected));
        
        result = subject.CalculateForPoint(3, 3);
        expected = Math.Sqrt(2*2);
        Assert.That(result, Is.EqualTo(expected));
    }
    
    [Test]
    public void incorrect_program_stops()
    {
        var subject = new TichProgram(new[]{
            C(Command.Invalid)
        });
        var result = subject.CalculateForPoint(10, 10);
        Assert.That(result, Is.EqualTo(10)); // stops before pushing the value, P is still top of stack
    }

    [Test]
    public void other_values_on_stack_are_ignored()
    {
        var subject = new TichProgram(new[]{
            C((double)0),
            C(1),
            C(2),
            C(3),
            C(4),
            C(5),
        });
        var result = subject.CalculateForPoint(10, 10);
        Assert.That(result, Is.EqualTo(5)); // returns only top of stack
    }

    [Test(Description = "Exercise every operation and function to check they are implemented")]
    [TestCase("1.2 + 3.4", 4.6)]
    [TestCase("1.2 - 3.4", -2.2)]
    [TestCase("1.2 & 3.4", 1)]
    [TestCase("1.2 / 3.4", 0.35294)]
    [TestCase("1.2 = 3.4", 0)]
    [TestCase("1.2 < 3.4", 1)]
    [TestCase("1.2 % 3.4", 1.2)]
    [TestCase("1.2 > 3.4", 0)]
    [TestCase("1.2 * 3.4", 4.08)]
    [TestCase("1.2 | 3.4", 1)]
    [TestCase("1.2 ^ 3.4", 1.8587)]
    [TestCase("1.0 / 3.4", 0.29411)]
    [TestCase("1.2 <= 3.4", 1)]
    [TestCase("1.2 >= 3.4", 0)]
    [TestCase("1.2 != 3.4", 1)]
    
    [TestCase("abs(-2.3)", 2.3)]
    [TestCase("acos(0.2)", 1.3694)]
    [TestCase("all(vec3(1,1,1))", 1.0)]
    [TestCase("and(1,0)", 0.0)]
    [TestCase("angle(pi)", -1.0)]
    [TestCase("clamp(p, 0, 1)", 1.0)]
    [TestCase("cos(p)", 0.96017)]
    [TestCase("cross(p,vec2(3,2))", 36.0)]
    [TestCase("dot(p,vec2(0,1))", 8.0)]
    [TestCase("eq(p,p)", 1.0)]
    [TestCase("high(3,5)", 5)]
    [TestCase("length(p)", 10.0)]
    [TestCase("len(p)", 10.0)]
    [TestCase("lerp(vec2(0,0),p, 0.5)", 3.0)]
    [TestCase("low(vec2(0,0),p)", 0.0)]
    [TestCase("max(vec2(0,0),p)", 6.0)]
    [TestCase("mul(p,1,2,3,4)", 24.0)]
    [TestCase("mid(p,p*2)", 9.0)]
    [TestCase("min(vec2(0,0),p)", 0.0)]
    [TestCase("neg(p)", -6.0)]
    [TestCase("none(p)", 0)]
    [TestCase("norm(p)", 0.6)]
    [TestCase("not(p)", 0)]
    [TestCase("pow(p,p)", 46656.0)]
    [TestCase("rec(p)", 0.1667)]
    [TestCase("rect(vec2(-1,-2),vec2(3,4))", 2.0)]
    [TestCase("sign(p)", 1)]
    [TestCase("sin(p)", -0.279415)]
    [TestCase("sqrt(p)", 2.449489)]
    [TestCase("vec2(0,1)", 0.0)]
    [TestCase("vec3(0,1,2)", 0.0)]
    [TestCase("vec4(0,1,2,3)", 0.0)]
    [TestCase("maxC(p)", 8.0)]
    [TestCase("1.0", 1.0)] // optimisation
    [TestCase("vec2(1,1)", 1.0)] // optimisation
    [TestCase("vec3(1,1,1)", 1.0)] // optimisation
    [TestCase("vec4(1,1,1,1)", 1.0)] // optimisation
    [TestCase("mix(p,p*2,0.1)", 6.120)]
    [TestCase("0.0", 0.0)] // optimisation
    [TestCase("vec2(0,0)", 0.0)] // optimisation
    [TestCase("vec3(0,0,0)", 0.0)] // optimisation
    [TestCase("vec4(0,0,0,0)", 0.0)] // optimisation
    public void operations_and_functions(string expr, double expected)
    {
        var postfix = Compiler.InfixToPostfix(expr);
        var code = Compiler.CompilePostfix(postfix).ToList();
        Console.WriteLine(code.PrettyPrint());
        
        var program = new TichProgram(code);
        var result = program.CalculateForPoint(6, 8); // perimeter = area = 24; length = 10
        
        Assert.That(result, Is.EqualTo(expected).Within(0.001));
    }

    private Cell C(double p) => new() { Cmd = Command.Scalar, NumberValue = p};
    private Cell C(Command cmd) => new() { Cmd = cmd};
}