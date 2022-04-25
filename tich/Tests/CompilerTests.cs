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

    [Test] // these pass P where appropriate to take advantage of the initial-P elision. 
    [TestCase("abs(p)", Command.Abs)]
    [TestCase("acos(p)", Command.Acos)]
    [TestCase("all(p)", Command.All)]
    [TestCase("and(p,p)", Command.And)]
    public void function_cases(string expr, Command expected)
    {
        var postfix = Compiler.InfixToPostfix(expr);
        var code = Compiler.CompilePostfix(postfix).ToList();
        Console.WriteLine(code.PrettyPrint());
        
        Assert.That(code.Last().Cmd, Is.EqualTo(expected));
    }
}